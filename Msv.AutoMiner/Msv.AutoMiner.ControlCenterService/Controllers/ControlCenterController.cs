using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Log;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Common.ServiceContracts;
using Msv.AutoMiner.ControlCenterService.Logic.Analyzers;
using Msv.AutoMiner.ControlCenterService.Security;
using Msv.AutoMiner.ControlCenterService.Security.Contracts;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Newtonsoft.Json;
using NLog;

namespace Msv.AutoMiner.ControlCenterService.Controllers
{
    [Route("api/[controller]")]
    public class ControlCenterController : Controller, IControlCenterService
    {
        private static readonly TimeSpan M_MaxInactivityInterval = TimeSpan.FromHours(2);

        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly ICertificateService m_CertificateService;
        private readonly ICoinInfoService m_CoinInfoService;
        private readonly IHeartbeatAnalyzer m_HeartbeatAnalyzer;
        private readonly IControlCenterControllerStorage m_Storage;

        public ControlCenterController(
            ICertificateService certificateService,
            ICoinInfoService coinInfoService,
            IHeartbeatAnalyzer heartbeatAnalyzer,
            IControlCenterControllerStorage storage)
        {
            m_CertificateService = certificateService;
            m_CoinInfoService = coinInfoService;
            m_HeartbeatAnalyzer = heartbeatAnalyzer;
            m_Storage = storage;
        }

        [HttpPost("registerRig")]
        public RegisterRigResponseModel RegisterRig([FromBody] RegisterRigRequestModel request)
        {
            M_Logger.Info($"Received registration request for rig {request.Name}");
            if (request.Name == null || request.Password == null || request.X509CertificateRequest.IsNullOrEmpty())
            {
                M_Logger.Warn($"Invalid registration request for {request.Name}");
                return new RegisterRigResponseModel();
            }
            var rig = m_Storage.GetRigByName(request.Name);
            if (rig == null)
            {
                M_Logger.Warn($"Rig {request.Name} not found");
                return new RegisterRigResponseModel();
            }
            if (rig.RegistrationPassword == null)
            {
                M_Logger.Warn($"Rig {request.Name} has no registration password");
                return new RegisterRigResponseModel();
            }
            if (rig.RegistrationPassword != request.Password)
            {
                M_Logger.Warn($"Rig {request.Name}: invalid registration password");
                return new RegisterRigResponseModel();
            }
            M_Logger.Info($"Creating certificate for rig {request.Name}...");
            var certificate = m_CertificateService.CreateCertificate(
                rig, SiteCertificates.PortCertificates[HttpContext.Connection.LocalPort], request.X509CertificateRequest);
            if (certificate == null)
            {
                M_Logger.Warn($"Rig {request.Name}: certificate creation failed");
                return new RegisterRigResponseModel();
            }
            rig.RegistrationPassword = null;
            rig.ClientCertificateSerial = certificate.GetSerialNumber();
            rig.ClientCertificateThumbprint = HexHelper.FromHex(certificate.Thumbprint);
            m_Storage.SaveRig(rig);
            return new RegisterRigResponseModel
            {
                IsSuccess = true,
                X509ClientCertificate = certificate.RawData,
                CaCertificate = SiteCertificates.PortCertificates[HttpContext.Connection.LocalPort].RawData
            };
        }

        [HttpGet("getAlgorithms")]
        [AuthenticateRigByCertificate]
        public Task<AlgorithmInfo[]> GetAlgorithms() 
            => m_CoinInfoService.GetAlgorithms();

        [HttpPost("sendHeartbeat")]
        [AuthenticateRigByCertificate]
        public SendHeartbeatResponseModel SendHeartbeat([FromBody] Heartbeat heartbeat)
        {
            if (heartbeat == null)
                throw new ArgumentNullException(nameof(heartbeat));

            var rigId = (int)ControllerContext.RouteData.Values["rigId"];
            M_Logger.Info($"Got heartbeat from rig {rigId}");
            m_Storage.SaveHeartbeat(new RigHeartbeat
            {
                Received = DateTime.UtcNow,
                RigId = rigId,
                ContentsJson = JsonConvert.SerializeObject(heartbeat),
                RemoteAddress = HttpContext.Connection.RemoteIpAddress.ToString()
            });
            var now = DateTime.UtcNow;
            m_Storage.SaveMiningStates(heartbeat.MiningStates
                .EmptyIfNull()
                .Where(x => x != null)
                .Select(x => new RigMiningState
                {
                    DateTime = now,
                    CoinId = x.CoinId,
                    RigId = rigId,
                    InvalidShares = x.InvalidShares,
                    ValidShares = x.ValidShares,
                    HashRate = x.HashRate.Current
                })
                .ToArray());
            m_HeartbeatAnalyzer.Analyze(rigId, heartbeat);
            var command = m_Storage.GetNextCommand(rigId);
            if (command == null)
                return new SendHeartbeatResponseModel();
            M_Logger.Info($"Sending command {command.Type} to rig {rigId}");
            m_Storage.MarkCommandAsSent(command.Id);
            return new SendHeartbeatResponseModel
            {
                PendingCommand = command.Type
            };
        }

        [HttpPost("getMiningWork")]
        [AuthenticateRigByCertificate]
        public async Task<MiningWorkModel[]> GetMiningWork([FromBody] GetMiningWorkRequestModel request)
        {
            var rigId = (int)ControllerContext.RouteData.Values["rigId"];
            M_Logger.Info($"Rig {rigId} requested new mining work");

            var coinStatistics = await m_CoinInfoService.GetProfitabilities(
                new ProfitabilityRequestModel
                {
                    AlgorithmDatas = request.AlgorithmDatas,
                    DifficultyAggregationType = ValueAggregationType.Last12Hours,
                    ElectricityCostUsd = request.ElectricityCostUsd,
                    PriceAggregationType = ValueAggregationType.Last24Hours
                });
            var coinIds = coinStatistics.Profitabilities
                .EmptyIfNull()
                .Where(x => DateTime.UtcNow - M_MaxInactivityInterval < x.LastUpdatedUtc)
                .Select(x => x.CoinId)
                .ToArray();
            var btcMiningTarget = m_Storage.GetBitCoinMiningTarget();
            var works = m_Storage.GetActivePools(coinIds)
                .GroupBy(x => x.Coin)
                .Join(coinStatistics.Profitabilities.EmptyIfNull(), x => x.Key.Id, x => x.CoinId,
                    (x, y) => (profitability:y, pools:x, miningTarget: x.Key.Wallets.FirstOrDefault(a => a.IsMiningTarget)))
                .Select(x => new MiningWorkModel
                {
                    CoinId = x.pools.Key.Id,
                    CoinName = x.pools.Key.Name,
                    CoinSymbol = x.pools.Key.Symbol,
                    CoinAlgorithmId = x.pools.Key.AlgorithmId,
                    Pools = x.pools.Select(y => CreatePoolDataModel(
                        y, x.profitability, y.UseBtcWallet ? btcMiningTarget : x.miningTarget))
                        .Where(y => y != null)
                        .ToArray()
                })
                .Where(x => x.Pools.Any())
                .ToArray();
            if (request.TestMode)
                return works;
            var now = DateTime.UtcNow;
            m_Storage.SaveProfitabilities(works
                .SelectMany(x => x.Pools.Select(y => new CoinProfitability
                {
                    CoinId = x.CoinId,
                    PoolId = y.Id,
                    BtcPerDay = y.BtcPerDay,
                    CoinsPerDay = y.CoinsPerDay,
                    ElectricityCost = y.ElectricityCost,
                    RigId = rigId,
                    UsdPerDay = y.UsdPerDay,
                    Requested = now
                }))
                .ToArray());
            return works;
        }

        [HttpGet("getLog")]
        //TODO: ONLY FOR INTERNAL SERVICE!!!!!!!!
        public Task<ServiceLogs> GetLog()
            => Task.FromResult(new ServiceLogs
            {
                Errors = MemoryBufferTarget.GetBuffer("ErrorLogBuffer"),
                Full = MemoryBufferTarget.GetBuffer("FullLogBuffer")
            });

        private static PoolDataModel CreatePoolDataModel(Pool currentPool, SingleProfitabilityData profitability, Wallet miningTarget)
        {
            if (miningTarget == null)
                return null;

            var market = SelectAppropriateMarket(
                profitability.MarketPrices,
                currentPool.UseBtcWallet ? null : miningTarget.ExchangeType);
            return new PoolDataModel
            {
                Id = currentPool.Id,
                Name = currentPool.Name,
                Protocol = currentPool.Protocol,
                CoinsPerDay = CalculateValueWithPoolFee(profitability.CoinsPerDay, currentPool.FeeRatio),
                ElectricityCost = profitability.ElectricityCostPerDay,
                BtcPerDay = CalculateValueWithPoolFee(market?.BtcPerDay, currentPool.FeeRatio),
                UsdPerDay = CalculateValueWithPoolFee(market?.UsdPerDay, currentPool.FeeRatio),
                Priority = currentPool.Priority,
                Login = currentPool.GetLogin(miningTarget),
                Password = string.IsNullOrEmpty(currentPool.WorkerPassword) ? "x" : currentPool.WorkerPassword,
                Url = currentPool.GetUrl()
            };

            double CalculateValueWithPoolFee(double? value, double poolFee)
                => value.GetValueOrDefault() * (1 - poolFee / 100);

            MarketPriceData SelectAppropriateMarket(MarketPriceData[] markets, ExchangeType? exchangeType)
                => markets.Where(z => exchangeType == null || z.Exchange == exchangeType)
                    .OrderByDescending(z => z.BtcPerDay)
                    .FirstOrDefault();
        }
    }
}
