using System;
using System.Linq;
using System.Reactive.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Newtonsoft.Json;
using NLog;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class ClaymoreDualMinerStatusProvider : IMinerStatusProvider
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();
        private static readonly ILogger M_MinerOutputLogger = LogManager.GetLogger("MinerOutput");

        public long CurrentHashRate { get; private set; }
        public long CurrentSecondaryHashRate { get; } = 0;
        public int AcceptedShares { get; private set; }
        public int RejectedShares { get; private set; }

        private readonly IWebClient m_WebClient;
        private readonly Uri m_ApiUri;
        private readonly IDisposable m_Subscription;

        public ClaymoreDualMinerStatusProvider(IWebClient webClient, Uri apiUri)
        {
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_ApiUri = apiUri ?? throw new ArgumentNullException(nameof(apiUri));
            m_Subscription = CreateSubscription();
        }

        public void Dispose()
            => m_Subscription.Dispose();

        private IDisposable CreateSubscription()
            => Observable.Timer(TimeSpan.FromSeconds(4))
                .Repeat()
                .Subscribe(x =>
                {
                    try
                    {
                        UpdateData();
                    }
                    catch (Exception ex)
                    {
                        M_Logger.Error(ex, "Couldn't get data from Claymore Dual API");
                    }
                });

        private void UpdateData()
        {
            var document = new HtmlDocument();
            document.LoadHtml(m_WebClient.DownloadString(m_ApiUri));
            var resultJsonText = document.DocumentNode.SelectSingleNode("//body")
                .ChildNodes
                .First(x => x.NodeType == HtmlNodeType.Text)
                .InnerText;
            M_MinerOutputLogger.Debug($"ClaymoreDual: {resultJsonText}");

            dynamic resultJson = JsonConvert.DeserializeObject(resultJsonText);
            var resultData = ((string)resultJson.result[2])
                .Split(';')
                .Select(int.Parse)
                .ToArray();

            CurrentHashRate = resultData[0] * 1000;
            AcceptedShares += resultData[1];
            RejectedShares += resultData[2];
        }
    }
}
