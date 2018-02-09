using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;
using Msv.AutoMiner.FrontEnd.Models.Coins;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class CoinsController : EntityControllerBase<Coin, CoinDisplayModel, Guid>
    {
        public const string CoinsMessageKey = "CoinsMessage";

        private readonly ICoinNetworkInfoProvider m_NetworkInfoProvider;
        private readonly ICoinValueProvider m_CoinValueProvider;
        private readonly IStoredFiatValueProvider m_FiatValueProvider;
        private readonly IImageProcessor m_ImageProcessor;
        private readonly IOverallProfitabilityCalculator m_OverallProfitabilityCalculator;
        private readonly IProfitabilityCalculator m_ProfitabilityCalculator;
        private readonly AutoMinerDbContext m_Context;

        public CoinsController(
            ICoinNetworkInfoProvider networkInfoProvider,
            ICoinValueProvider coinValueProvider,
            IStoredFiatValueProvider fiatValueProvider,
            IImageProcessor imageProcessor,
            IOverallProfitabilityCalculator overallProfitabilityCalculator,
            IProfitabilityCalculator profitabilityCalculator,
            AutoMinerDbContext context)
            : base("_CoinRowPartial", context)
        {
            m_NetworkInfoProvider = networkInfoProvider;
            m_CoinValueProvider = coinValueProvider;
            m_FiatValueProvider = fiatValueProvider;
            m_ImageProcessor = imageProcessor;
            m_OverallProfitabilityCalculator = overallProfitabilityCalculator;
            m_ProfitabilityCalculator = profitabilityCalculator;
            m_Context = context;
        }

        public IActionResult Index()
        {
            var currentBtcUsdRate = m_FiatValueProvider.GetLastBtcUsdValue().Value;
            var yesterdayBtcUsdRate = m_FiatValueProvider.GetLastBtcUsdValue(DateTime.UtcNow.AddDays(-1)).Value;
            return View(new CoinsIndexModel
            {
                Coins = GetEntityModels(null),
                BtcUsdRate = currentBtcUsdRate,
                BtcUsdRateDelta = ConversionHelper.GetDiffRatio(yesterdayBtcUsdRate, currentBtcUsdRate)
            });
        }

        public async Task<IActionResult> Create() 
            => View("Edit", new CoinEditModel
            {
                Id = Guid.NewGuid(),
                AvailableAlgorithms = await GetAvailableAlgorithms()
            });

        public async Task<IActionResult> Edit(Guid id)
        {
            var coin = await m_Context.Coins
                .Include(x => x.Algorithm)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (coin == null)
                return NotFound();
            if (!IsEditable(coin))
                return Forbid();
            var lastNetworkInfo = await m_Context.CoinNetworkInfos
                .Where(x => x.CoinId == coin.Id)
                .OrderByDescending(x => x.Created)
                .FirstOrDefaultAsync();
            var coinModel = new CoinEditModel
            {
                Id = coin.Id,
                Name = coin.Name,
                Symbol = coin.Symbol,
                Logo = coin.LogoImageBytes,
                AlgorithmId = coin.AlgorithmId,
                AvailableAlgorithms = await GetAvailableAlgorithms(),
                NetworkInfoApiType = coin.NetworkInfoApiType,
                NodeLogin = coin.NodeLogin,
                MaxTarget = coin.MaxTarget,
                NodePassword = coin.NodePassword,
                NetworkApiName = coin.NetworkInfoApiName,
                NetworkApiUrl = coin.NetworkInfoApiUrl,
                RewardCalculationJavaScript = coin.RewardCalculationJavaScript,
                NodeUrl = coin.NodeHost != null
                    ? $"http://{coin.NodeHost}:{coin.NodePort}"
                    : null,
                LastHeight = lastNetworkInfo?.Height,
                LastDifficulty = lastNetworkInfo?.Difficulty,
                LastTotalSupply = lastNetworkInfo?.TotalSupply,
                LastMasternodeCount = lastNetworkInfo?.MasternodeCount,
                AddressFormat = coin.AddressFormat,
                AddressPrefixes = coin.AddressPrefixes,
                GetDifficultyFromLastPoWBlock = coin.GetDifficultyFromLastPoWBlock
            };
            return View(coinModel);
        }

        public async Task<IActionResult> Export(Guid id)
        {
            var coin = await m_Context.Coins.FirstAsync(x => x.Id == id);
            var exportedContent = JsonConvert.SerializeObject(
                new CoinExportModel
                {
                    AddressFormat = coin.AddressFormat,
                    AddressPrefixes = coin.AddressPrefixes,
                    AlgorithmId = coin.AlgorithmId,
                    Logo = coin.LogoImageBytes,
                    MaxTarget = coin.MaxTarget,
                    Name = coin.Name,
                    NetworkApiName = coin.NetworkInfoApiName,
                    NetworkApiUrl = coin.NetworkInfoApiUrl,
                    NetworkInfoApiType = coin.NetworkInfoApiType,
                    RewardCalculationJavaScript = coin.RewardCalculationJavaScript,
                    Symbol = coin.Symbol,
                    GetDifficultyFromLastPoWBlock = coin.GetDifficultyFromLastPoWBlock
                });
            return ReturnAsJsonFile($"{coin.Name}_settings.json", exportedContent);
        }

        [HttpPost]
        public async Task<IActionResult> Save(CoinEditModel coinModel)
        {
            byte[] newLogoBytes = null;
            if (coinModel.NewLogoUrl != null)
                using (var httpClient = new HttpClient {Timeout = TimeSpan.FromSeconds(20)})
                using (var response = await httpClient.GetAsync(coinModel.NewLogoUrl))
                {
                    if (!response.IsSuccessStatusCode)
                        ModelState.AddModelError(nameof(coinModel.NewLogoUrl),
                            $"Couldn't download logo image. Result: {response.StatusCode:D} {response.ReasonPhrase}");
                    else if (!response.Content.Headers.ContentType.MediaType.StartsWith("image/"))
                        ModelState.AddModelError(nameof(coinModel.NewLogoUrl),
                            $"Couldn't download logo image. Server returned incorrect content type: {response.Content.Headers.ContentType.MediaType}");
                    else
                        newLogoBytes = await response.Content.ReadAsByteArrayAsync();
                }
            if (!coinModel.Logo.IsNullOrEmpty())
                newLogoBytes = coinModel.Logo;

            var nodeUrl = coinModel.NodeUrl != null
                ? new Uri(coinModel.NodeUrl)
                : null;
            var coin = await m_Context.Coins.FirstOrDefaultAsync(x => x.Id == coinModel.Id)
                       ?? m_Context.Coins.Add(new Coin
                       {
                           Activity = ActivityState.Active
                       }).Entity;
            if (coin.Symbol == "BTC" || coinModel.Symbol.ToUpperInvariant() == "BTC")
                ModelState.AddModelError(nameof(coinModel.Symbol), "Can't edit BitCoin data or create new coin with BTC ticker");

            if (!ModelState.IsValid)
            {
                coinModel.AvailableAlgorithms = await GetAvailableAlgorithms();
                return View("Edit", coinModel);
            }

            coin.Name = coinModel.Name;
            coin.AlgorithmId = coinModel.AlgorithmId.GetValueOrDefault();
            coin.Symbol = coinModel.Symbol.ToUpperInvariant();
            coin.NetworkInfoApiUrl = coinModel.NetworkApiUrl;
            coin.Id = coinModel.Id;
            coin.NetworkInfoApiName = coinModel.NetworkApiName;
            coin.NetworkInfoApiType = coinModel.NetworkInfoApiType;
            coin.NodeHost = nodeUrl?.Host;
            coin.NodePort = (nodeUrl?.Port).GetValueOrDefault();
            coin.NodeLogin = coinModel.NodeLogin;
            coin.NodePassword = coinModel.NodePassword;
            coin.MaxTarget = coinModel.MaxTarget;
            coin.RewardCalculationJavaScript = coinModel.RewardCalculationJavaScript;
            coin.AddressFormat = coinModel.AddressFormat;
            coin.AddressPrefixes = coinModel.AddressPrefixes;
            coin.GetDifficultyFromLastPoWBlock = coinModel.GetDifficultyFromLastPoWBlock;
            if (newLogoBytes != null)
                coin.LogoImageBytes = m_ImageProcessor.Resize(newLogoBytes, 16, 16, ImageFormats.Png);
            else if (coinModel.DeleteLogo)
                coin.LogoImageBytes = null;
            await m_Context.SaveChangesAsync();
            TempData[CoinsMessageKey] = $"Coin {coin.Name} ({coin.Symbol}) has been successfully saved";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Graph(Guid id, GraphType type, GraphPeriod period)
        {           
            var coin = await m_Context.Coins.FirstOrDefaultAsync(x => x.Id == id);
            if (coin == null)
                return NotFound();

            DateTime startDate;
            var endDate = DateTime.UtcNow;
            Func<DateTime, DateTime> dateAggregationKey;
            switch (period)
            {
                case GraphPeriod.Day:
                    startDate = endDate.AddDays(-1);
                    dateAggregationKey = x => new DateTime(x.Year, x.Month, x.Day, x.Hour, x.Minute, 0);
                    break;
                case GraphPeriod.Week:
                    startDate = endDate.AddDays(-7);
                    dateAggregationKey = x => new DateTime(x.Year, x.Month, x.Day, x.Hour, 0, 0);
                    break;
                case GraphPeriod.TwoWeeks:
                    startDate = endDate.AddDays(-14);
                    dateAggregationKey = x => new DateTime(x.Year, x.Month, x.Day, x.Hour, 0, 0);
                    break;
                case GraphPeriod.Month:
                    startDate = endDate.AddMonths(-1);
                    dateAggregationKey = x => new DateTime(x.Year, x.Month, x.Day, 0, 0, 0);
                    break;
                case GraphPeriod.SixMonths:
                    startDate = endDate.AddMonths(-6);
                    dateAggregationKey = x => new DateTime(x.Year, x.Month, x.Day, 0, 0, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(period));
            }

            Expression<Func<CoinNetworkInfo, TimestampedValue>> valueSelector;
            switch (type)
            {
                case GraphType.Difficulty:
                    valueSelector = x => new TimestampedValue {DateTime = x.Created, Value = x.Difficulty};
                    break;
                case GraphType.Height:
                    valueSelector = x => new TimestampedValue {DateTime = x.Created, Value = x.Height};
                    break;
                case GraphType.Reward:
                    valueSelector = x => new TimestampedValue {DateTime = x.Created, Value = x.BlockReward};
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            var values = m_Context.CoinNetworkInfos
                .AsNoTracking()
                .Where(x => x.CoinId == id)
                .Where(x => x.Created > startDate && x.Created <= endDate)
                .Select(valueSelector)
                .AsEnumerable()
                .GroupBy(x => dateAggregationKey(x.DateTime))
                .Select(x => (dateTime: x.Key, value: x.Average(y => Convert.ToDouble(y.Value))))
                .OrderBy(x => x.dateTime)
                .ToArray();

            return View(new GraphModel
            {
                Id = id,
                Type = type,
                CoinName = coin.Name,
                Period = period,
                Dates = values.Select(x => DateTime.SpecifyKind(x.dateTime, DateTimeKind.Utc)).ToArray(),
                Values = values.Select(x => x.value).ToArray()
            });
        }

        public async Task<IActionResult> CreateConfigFile(Guid id)
        {
            var coin = await m_Context.Coins.FirstOrDefaultAsync(x => x.Id == id);
            if (coin == null)
                return NotFound();
            var allowIpMask = coin.NodeHost.Equals("localhost", StringComparison.InvariantCultureIgnoreCase)
                              || coin.NodeHost.Equals("127.0.0.1", StringComparison.InvariantCultureIgnoreCase)
                ? "127.0.0.1"
                : "*";
            var configContents = $@"server=1
daemon=1
rpcuser={coin.NodeLogin}
rpcpassword={coin.NodePassword}
rpcport={coin.NodePort}
rpcallowip={allowIpMask}
";
            var configFileName = string.Concat(coin.Name.ToLowerInvariant().Split(Path.GetInvalidFileNameChars()))
                .Replace(" ", "-");

            return File(Encoding.ASCII.GetBytes(configContents), "application/octet-stream", $"{configFileName}.conf");
        }

        protected override bool IsEditable(Coin entity)
            => entity.Symbol != "BTC";

        protected override CoinDisplayModel[] GetEntityModels(Guid[] ids)
        {
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var lastInfos = m_NetworkInfoProvider.GetCurrentNetworkInfos(false);
            var yesterdayInfos = m_NetworkInfoProvider.GetCurrentNetworkInfos(false, yesterday);
            var lastCoinValues = m_CoinValueProvider.GetCurrentCoinValues(false);
            var yesterdayCoinValues = m_CoinValueProvider.GetCurrentCoinValues(false, yesterday);
            var btcUsdRate = m_FiatValueProvider.GetLastBtcUsdValue().Value;
            var overallHashrates = m_OverallProfitabilityCalculator.CalculateTotalPower()
                .ToDictionary(x => x.AlgorithmId, x => x.NetHashRate);

            var miningExchanges = m_Context.Wallets
                .AsNoTracking()
                .Where(x => x.IsMiningTarget && x.Activity == ActivityState.Active)
                .Where(x => x.ExchangeType != null)
                .Select(x => new {x.CoinId, x.ExchangeType})
                .ToArray();

            var coinQuery = m_Context.Coins
                .Include(x => x.Algorithm)
                .AsNoTracking()
                .Where(x => x.Activity != ActivityState.Deleted)
                .Where(x => x.Algorithm.Activity == ActivityState.Active);
            if (!ids.IsNullOrEmpty())
                coinQuery = coinQuery.Where(x => ids.Contains(x.Id));
            return coinQuery
                .AsEnumerable()
                .LeftOuterJoin(lastInfos, x => x.Id, x => x.CoinId,
                    (x, y) => (coin:x, network: y ?? new CoinNetworkInfo()))
                .LeftOuterJoin(yesterdayInfos, x => x.coin.Id, x => x.CoinId,
                    (x, y) => (x.coin, x.network, previousNetwork: y ?? new CoinNetworkInfo()))
                .LeftOuterJoin(lastCoinValues, x => x.coin.Id, x => x.CurrencyId,
                    (x, y) => (x.coin, x.network, x.previousNetwork, value: y ?? new CoinValue()))
                .LeftOuterJoin(yesterdayCoinValues, x => x.coin.Id, x => x.CurrencyId,
                    (x, y) => (x.coin, x.network, x.previousNetwork, x.value, previousValue: y ?? new CoinValue()))
                .LeftOuterJoin(miningExchanges, x => x.coin.Id, x => x.CoinId,
                    (x, y) => (x.coin, x.network, x.previousNetwork, x.value, x.previousValue, miningExchange: y))
                .Select(x => new CoinDisplayModel
                {
                    Id = x.coin.Id,
                    Name = x.coin.Name,
                    Symbol = x.coin.Symbol,
                    Algorithm = new AlgorithmBaseModel
                    {
                        Id = x.coin.AlgorithmId,
                        KnownValue = x.coin.Algorithm.KnownValue,
                        Name = x.coin.Algorithm.Name
                    },
                    ExchangePrices = x.value.ExchangePrices
                        .EmptyIfNull()
                        .Do(y => y.UsdPrice = y.Price * btcUsdRate)
                        .LeftOuterJoin(x.previousValue.ExchangePrices.EmptyIfNull(), y => y.Exchange, y => y.Exchange, 
                            (y,z) => (current: y, delta: ConversionHelper.GetDiffRatio(z?.Price ?? 0, y.Price)))
                        .Do(y => y.current.PriceDelta = y.delta)
                        .Select(y => y.current)
                        .ToArray(),
                    MiningTargetExchange = x.miningExchange?.ExchangeType,
                    Activity = x.coin.Activity,
                    BlockReward = x.network.BlockReward,
                    Difficulty = x.network.Difficulty,
                    DifficultyDelta = ConversionHelper.GetDiffRatio(x.previousNetwork.Difficulty, x.network.Difficulty),
                    NetHashRate = x.network.NetHashRate,
                    Height = x.network.Height,
                    LastBlockTime = x.network.LastBlockTime,
                    Logo = x.coin.LogoImageBytes,
                    HasLocalNode = !string.IsNullOrEmpty(x.coin.NodeHost),
                    MasternodeCount = x.network.MasternodeCount,
                    TotalSupply = x.network.TotalSupply,
                    SoloMiningTtf = m_ProfitabilityCalculator.CalculateTimeToFind(
                        x.network.Difficulty, x.coin.MaxTarget, overallHashrates.TryGetValue(x.coin.AlgorithmId)),
                    LastUpdated = x.network.Created != default
                        ? x.network.Created
                        : (DateTime?) null
                })
                .ToArray();
            
        }

        private Task<AlgorithmBaseModel[]> GetAvailableAlgorithms()
            => m_Context.CoinAlgorithms
                .Where(x => x.Activity == ActivityState.Active)
                .Select(x => new AlgorithmBaseModel
                {
                    Id = x.Id,
                    KnownValue = x.KnownValue,
                    Name = x.Name
                })
                .ToArrayAsync();

        private class TimestampedValue
        {
            public object Value { get; set; }
            public DateTime DateTime { get; set; }
        }
    }
}
