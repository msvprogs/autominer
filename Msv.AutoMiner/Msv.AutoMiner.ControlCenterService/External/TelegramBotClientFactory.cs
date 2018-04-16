using System;
using System.Net;
using System.Net.Http;
using Msv.AutoMiner.ControlCenterService.Configuration;
using Telegram.Bot;

namespace Msv.AutoMiner.ControlCenterService.External
{
    public static class TelegramBotClientFactory
    {
        public static ITelegramBotClient Create(TelegramElement config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!config.UseProxy)
                return new TelegramBotClient(config.Token);
            return new TelegramBotClient(config.Token, new HttpClient(new HttpClientHandler
            {
                Proxy = new WebProxy(
                    new UriBuilder(
                        config.IsProxyHttps ? Uri.UriSchemeHttps : Uri.UriSchemeHttp,
                        config.ProxyHost,
                        config.ProxyPort).Uri, false)
            }));
        }
    }
}
