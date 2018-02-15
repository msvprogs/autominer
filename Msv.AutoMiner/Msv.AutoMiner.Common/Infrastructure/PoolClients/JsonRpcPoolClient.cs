using System;
using System.Net;
using JetBrains.Annotations;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.HttpTools;

namespace Msv.AutoMiner.Common.Infrastructure.PoolClients
{
    public class JsonRpcPoolClient : IPoolClient
    {
        private readonly IWebClient m_WebClient;
        private readonly PoolDataModel m_Pool;

        public JsonRpcPoolClient([NotNull] IWebClient webClient, [NotNull] PoolDataModel pool)
        {
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_Pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        public PoolAvailabilityState CheckAvailability()
        {
            var client = new HttpJsonRpcClient(m_WebClient, m_Pool.Url.Host, m_Pool.Url.Port, m_Pool.Login, m_Pool.Password);
            try
            {
                if (!TryJsonRpc(client, "ping"))
                    return PoolAvailabilityState.AuthenticationFailed;
            }
            catch (WebException)
            {
                if (!TryJsonRpc(client, "getinfo"))
                    return PoolAvailabilityState.AuthenticationFailed;
            }
            catch (CorrectHttpException)
            {
                if (!TryJsonRpc(client, "getinfo"))
                    return PoolAvailabilityState.AuthenticationFailed;
            }
            return PoolAvailabilityState.Available;
        }

        private static bool TryJsonRpc(IRpcClient client, string method)
        {
            try
            {
                client.Execute<object>(method);
            }
            catch (CorrectHttpException ex)
            {
                if (ex.Status == HttpStatusCode.Unauthorized)
                    return false;
                throw;
            }
            catch (WebException wex) when (wex.Status == WebExceptionStatus.ProtocolError)
            {
                if (((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.Unauthorized)
                    return false;
                throw;
            }
            return true;
        }
    }
}
