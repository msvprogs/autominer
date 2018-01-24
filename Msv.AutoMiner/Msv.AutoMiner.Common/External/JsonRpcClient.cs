using System;
using System.Net;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Msv.AutoMiner.Common.External
{
    public class JsonRpcClient : IRpcClient
    {
        private const int MaxLoggableLength = 256;
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly Uri m_Uri;
        private readonly string m_Login;
        private readonly string m_Password;

        public JsonRpcClient(string address, int port, string login, string password)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            var hostString = Uri.IsWellFormedUriString(address, UriKind.Absolute)
                ? new Uri(address).Host
                : address;
            m_Uri = new UriBuilder { Scheme = Uri.UriSchemeHttp, Host = hostString, Port = port }.Uri;
            m_Login = login ?? throw new ArgumentNullException(nameof(login));
            m_Password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public TResponse Execute<TResponse>(string method, params object[] args)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json-rpc";
                client.Credentials = new NetworkCredential(m_Login, m_Password);
                const int requestId = 2;
                var requestStr = JsonConvert.SerializeObject(new
                {
                    jsonrpc = "1.0",
                    id = requestId,
                    method,
                    @params = args
                });
                M_Logger.Info($"Sending JSON-RPC request to {m_Uri}: {Truncate(requestStr)}");
                var responseStr = client.UploadString(m_Uri, requestStr);
                M_Logger.Info($"Received JSON-RPC response {Truncate(responseStr)} from {m_Uri} ({client.ResponseHeaders[HttpResponseHeader.Server]})");
                var response = (dynamic)JsonConvert.DeserializeObject(responseStr);
                if ((int) response.id == requestId && response.error != null)
                    throw new ExternalDataUnavailableException((string) response.error.ToString());
                if ((int) response.id != requestId)
                    throw new ExternalDataUnavailableException("Response ID mismatch");
                return ((JObject)response.result).ToObject<TResponse>();
            }
        }

        private static string Truncate(string str)
            => str.Length <= MaxLoggableLength ? str : str.Substring(0, MaxLoggableLength) + "...";
    }
}
