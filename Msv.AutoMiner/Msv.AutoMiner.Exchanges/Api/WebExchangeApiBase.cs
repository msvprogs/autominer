using System;
using System.Collections.Generic;
using Msv.AutoMiner.Common.External.Contracts;

namespace Msv.AutoMiner.Exchanges.Api
{
    public abstract class WebExchangeApiBase : IExchangeApi
    {
        protected IWebClient WebClient { get; }

        protected WebExchangeApiBase(IWebClient webClient) 
            => WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public abstract dynamic ExecutePublic(string method, IDictionary<string, string> parameters);
        public abstract dynamic ExecutePrivate(
            string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret);

        protected static string CreateNonce()
            => DateTime.UtcNow.Ticks.ToString();
    }
}
