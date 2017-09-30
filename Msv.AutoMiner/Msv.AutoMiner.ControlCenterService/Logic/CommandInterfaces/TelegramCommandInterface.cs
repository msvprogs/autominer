using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Msv.AutoMiner.ControlCenterService.Logic.CommandInterfaces
{
    public class TelegramCommandInterface : IDisposable
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly ITelegramBotClient m_Client;
        private readonly ITelegramCommandInterfaceStorage m_Storage;
        private readonly string[] m_UserWhiteList;
        private readonly IDisposable m_Disposable;
        private readonly ConcurrentDictionary<int, TelegramInterpreterState> m_InterpreterStates =
            new ConcurrentDictionary<int, TelegramInterpreterState>();

        public TelegramCommandInterface(ITelegramBotClient client, ITelegramCommandInterfaceStorage storage, string[] userWhiteList)
        {
            m_Client = client ?? throw new ArgumentNullException(nameof(client));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
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
                    .Subscribe(x =>
                    {
                        try
                        {
                            ProcessMessage(x);
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

        private async void ProcessMessage(Message message)
        {
            if (!m_UserWhiteList.Contains(message.From.Username))
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
                .ToDictionary(x => x.Id);
            //language=html
            const string rigInfoFormat = @"<i>{0}</i>:
Now mining <b>{1}</b>
Shares: <b>{2}</b> valid, <b>{3}</b> invalid, <b>{4}</b>
Video card temperatures: <b>{5}</b>
Video card usages: <b>{6}</b>
Client version: <b>{7}</b>
Last updated: <b>{8:r}</b>";

            var heartbeatStrings = m_Storage.GetLastHeartbeats(rigNames)
                .Select(x => new
                {
                    Name = x.Key,
                    NowMining = x.Value.MiningStates.EmptyIfNull().FirstOrDefault(),
                    VideoCardStates = x.Value.VideoAdapterStates.EmptyIfNull(),
                    x.Value.ClientVersion,
                    x.Value.DateTime
                })
                .Select(x => string.Format(rigInfoFormat,
                    x.Name, 
                    HtmlEntity.Entitize(x.NowMining != null ? coins[x.NowMining.CoinId].Name : "none"),
                    x.NowMining?.ValidShares,
                    x.NowMining?.InvalidShares,
                    x.NowMining != null 
                        ? HtmlEntity.Entitize(ConversionHelper.ToHashRateWithUnits(x.NowMining.HashRate.Current, coins[x.NowMining.CoinId].Algorithm.KnownValue))
                        : "",
                    string.Join(", ", x.VideoCardStates.Select(y => y.Temperature.Current + "°C")),
                    string.Join(", ", x.VideoCardStates.Select(y => y.Utilization + "%")),
                    HtmlEntity.Entitize(x.ClientVersion),
                    x.DateTime))
                .ToArray();
            if (heartbeatStrings.Any())
                await m_Client.SendTextMessageAsync(user.Id, string.Join("\n\n", heartbeatStrings), ParseMode.Html);
            else
                await m_Client.SendTextMessageAsync(user.Id, "Didn't find any information about specified rigs");
        }
    }
}
