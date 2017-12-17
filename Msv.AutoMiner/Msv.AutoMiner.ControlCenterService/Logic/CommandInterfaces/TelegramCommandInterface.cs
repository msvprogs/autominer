using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = Telegram.Bot.Types.User;

namespace Msv.AutoMiner.ControlCenterService.Logic.CommandInterfaces
{
    public class TelegramCommandInterface : IDisposable
    {
        private static readonly TimeSpan M_OldestInfoPeriod = TimeSpan.FromDays(1);
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly ITelegramBotClient m_Client;
        private readonly ITelegramCommandInterfaceStorage m_Storage;
        private readonly IPoolInfoProvider m_PoolInfoProvider;
        private readonly IRigHeartbeatProvider m_RigHeartbeatProvider;
        private readonly string[] m_UserWhiteList;
        private readonly IDisposable m_Disposable;
        private readonly ConcurrentDictionary<int, TelegramInterpreterState> m_InterpreterStates =
            new ConcurrentDictionary<int, TelegramInterpreterState>();

        public TelegramCommandInterface(
            ITelegramBotClient client,
            ITelegramCommandInterfaceStorage storage,
            IPoolInfoProvider poolInfoProvider,
            IRigHeartbeatProvider rigHeartbeatProvider,
            string[] userWhiteList)
        {
            m_Client = client ?? throw new ArgumentNullException(nameof(client));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            m_PoolInfoProvider = poolInfoProvider ?? throw new ArgumentNullException(nameof(poolInfoProvider));
            m_RigHeartbeatProvider = rigHeartbeatProvider ?? throw new ArgumentNullException(nameof(rigHeartbeatProvider));
            m_UserWhiteList = userWhiteList ?? throw new ArgumentNullException(nameof(userWhiteList));
            m_Disposable = CreateRxDisposable(client);
        }

        public void Dispose() => m_Disposable.Dispose();

        private IDisposable CreateRxDisposable(ITelegramBotClient client)
        {
            var disposable = new CompositeDisposable
            {
                Disposable.Create(client.StopReceiving),
                Observable.FromEventPattern<MessageEventArgs>(
                        x => client.OnMessage += x,
                        x => client.OnMessage -= x)
                    .Select(x => x.EventArgs.Message)
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .Subscribe(x =>
                    {
                        try
                        {
                            ProcessMessage(x).GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            M_Logger.Error(ex, "Message processing error");
                        }
                    }, x => M_Logger.Fatal(x))
            };
            client.StartReceiving();
            return disposable;
        }

        private async Task ProcessMessage(Message message)
        {
            if (message.Text == null)
                return;
            if (string.IsNullOrEmpty(message.From.Username) || !m_UserWhiteList.Contains(message.From.Username))
            {
                await m_Client.SendTextMessageAsync(message.From.Id, "You're not permitted to use this bot");
                return;
            }
            m_Storage.StoreTelegramUser(new TelegramUser {Id = message.From.Id, UserName = message.From.Username});
            var interpreterState = m_InterpreterStates.GetOrAdd(message.From.Id, TelegramInterpreterState.Text);
            switch (message.Text.ToLowerInvariant())
            {
                case "/getstate":
                    if (interpreterState == TelegramInterpreterState.Text
                        || interpreterState == TelegramInterpreterState.AwaitingRigNames)
                    {
                        await m_Client.SendTextMessageAsync(message.From.Id,
                            "Enter rig names separated with comma: myrig1, hisrig2");
                        m_InterpreterStates.TryUpdate(message.From.Id, TelegramInterpreterState.AwaitingRigNames,
                            TelegramInterpreterState.Text);
                        return;
                    }
                    break;
                case "/getallstates":
                    await ProcessRigStateRequest(message.From, null);
                    break;
                default:
                    if (interpreterState == TelegramInterpreterState.AwaitingRigNames)
                        await ProcessRigStateRequest(message.From, message.Text.Split(',')
                            .Select(x => x.Trim().ToLowerInvariant())
                            .ToArray());
                    else
                        await m_Client.SendTextMessageAsync(message.From.Id, $"Hello, {message.From.FirstName} {message.From.LastName}!");
                    break;
            }
            m_InterpreterStates[message.From.Id] = TelegramInterpreterState.Text;
        }

        private async Task ProcessRigStateRequest(User user, string[] rigNames)
        {
            M_Logger.Info($"@{user.Username} requested states for rigs: {(rigNames != null ? string.Join(", ", rigNames) : "<all>")}");
            var coins = m_Storage.GetCoins()
                .Concat(new[] {new Coin {Name = "none", Algorithm = new CoinAlgorithm()}})
                .ToDictionary(x => x.Id);
            //language=html
            const string rigInfoFormat = @"<i>{0}:</i>
Now mining <b>{1} ({14})</b>
Shares: <b>{2}</b> valid, <b>{3}</b> invalid, <b>{4}</b>
Video card temperatures: <b>{5}</b>
Video card usages: <b>{6}</b>
Client version: <b>{7}</b>
Last updated: <b>{8}</b>
Pool shares: <b>{9}</b> valid, <b>{10}</b> invalid, <b>{11}</b>
Pool balance: confirmed <b>{12:N6} {14}</b>, unconfirmed <b>{13:N6} {14}</b>";

            var heartbeats = m_RigHeartbeatProvider.GetLastHeartbeats(
                m_Storage.GetRigIds(rigNames));
            var poolStates = m_PoolInfoProvider.GetCurrentPoolInfos()
                .Where(x => x.DateTime + M_OldestInfoPeriod > DateTime.UtcNow)
                .ToDictionary(x => x.PoolId);
            var rigIdNames = m_Storage.GetRigNames(heartbeats.Keys.ToArray());

            var heartbeatStrings = heartbeats
                .Where(x => x.Value.DateTime + M_OldestInfoPeriod > DateTime.UtcNow)
                .Select(x => new
                {
                    Name = rigIdNames[x.Key],
                    NowMining = x.Value.MiningStates.EmptyIfNull().FirstOrDefault() ?? new Heartbeat.MiningState(),
                    VideoCardStates = x.Value.VideoAdapterStates.EmptyIfNull(),
                    x.Value.ClientVersion,
                    x.Value.DateTime,
                    PoolState = x.Value.MiningStates?.Select(y => poolStates.TryGetValue(y.PoolId))
                        .FirstOrDefault(y => y != null) ?? new PoolAccountState()
                })
                .OrderBy(x => x.Name)
                .Select(x => string.Format(rigInfoFormat,
                    x.Name, 
                    HtmlEntity.Entitize(coins[x.NowMining.CoinId].Name),
                    x.NowMining.ValidShares,
                    x.NowMining.InvalidShares,
                    HtmlEntity.Entitize(ConversionHelper.ToHashRateWithUnits(x.NowMining.HashRate.Current, coins[x.NowMining.CoinId].Algorithm.KnownValue)),
                    string.Join(", ", x.VideoCardStates.Select(y => y.Temperature.Current + "°C")),
                    string.Join(", ", x.VideoCardStates.Select(y => y.Utilization + "%")),
                    HtmlEntity.Entitize(x.ClientVersion),
                    HtmlEntity.Entitize(DateTimeHelper.ToRelativeTime(x.DateTime)),
                    x.PoolState.ValidShares,
                    x.PoolState.InvalidShares,
                    HtmlEntity.Entitize(ConversionHelper.ToHashRateWithUnits(x.PoolState.HashRate, coins[x.NowMining.CoinId].Algorithm.KnownValue)),
                    x.PoolState.ConfirmedBalance,
                    x.PoolState.UnconfirmedBalance,
                    HtmlEntity.Entitize(coins[x.NowMining.CoinId].Symbol)))
                .ToArray();
            if (heartbeatStrings.Any())
                await m_Client.SendTextMessageAsync(user.Id, string.Join("\n\n", heartbeatStrings), ParseMode.Html);
            else
                await m_Client.SendTextMessageAsync(user.Id, "Didn't find any information about specified rigs");
        }
    }
}
