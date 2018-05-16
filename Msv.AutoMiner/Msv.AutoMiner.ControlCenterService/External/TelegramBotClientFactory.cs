using System;
using System.Net;
using Msv.AutoMiner.ControlCenterService.Configuration;
using NLog;
using Telegram.Bot;

namespace Msv.AutoMiner.ControlCenterService.External
{
    public static class TelegramBotClientFactory
    {
        public static ITelegramBotClient Create(TelegramElement config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var client = config.UseProxy
                ? new TelegramBotClient(config.Token, new WebProxy(
                    new UriBuilder(
                        config.IsProxyHttps ? Uri.UriSchemeHttps : Uri.UriSchemeHttp,
                        config.ProxyHost,
                        config.ProxyPort).Uri, false))
                : new TelegramBotClient(config.Token);

            var errorLogger = LogManager.GetLogger(nameof(TelegramBotClient));
            client.OnReceiveError +=
                (s, e) => errorLogger.Error(e.ApiRequestException, $"API error, code {e.ApiRequestException.ErrorCode}");
            client.OnReceiveGeneralError += (s, e) => errorLogger.Error(e.Exception, "Telegram bot exception");

            return client;
        }
    }
}
