﻿using System;
using System.Net;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using NLog;

namespace Msv.AutoMiner.Service.External.Rpc
{
    public class JsonRpcClient : IRpcClient
    {
        private static readonly ILogger M_Logger = LogManager.GetLogger("JsonRpcClient");

        private readonly string m_Login;
        private readonly string m_Password;
        private readonly Uri m_Uri;

        public JsonRpcClient(string address, int port, string login, string password)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));
            if (login == null)
                throw new ArgumentNullException(nameof(login));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            m_Login = login;
            m_Password = password;

            var hostString = Uri.IsWellFormedUriString(address, UriKind.Absolute)
                ? new Uri(address).Host
                : address;
            m_Uri = new UriBuilder {Scheme = Uri.UriSchemeHttp, Host = hostString, Port = port}.Uri;
        }

        public TResponse Execute<TResponse>(string method, object[] args = null)
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
                M_Logger.Info($"Sending JSON-RPC request to {m_Uri}: {requestStr}");
                var responseStr = client.UploadString(m_Uri, requestStr);
                M_Logger.Info($"Received JSON-RPC response {responseStr} from {m_Uri} ({client.ResponseHeaders[HttpResponseHeader.Server]})");
                var response = (dynamic)JsonConvert.DeserializeObject(responseStr);
                if ((int) response.id == requestId && response.error != null)
                    throw new WebException((string) response.error.ToString());
                if ((int) response.id != requestId)
                    throw new WebException("Response ID mismatch");
                return (TResponse)response.result;
            }
        }
    }
}
