using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using JetBrains.Annotations;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace Msv.AutoMiner.Common.External
{
    public class WebSocketJsonRpcClient : ISessionedRpcClient
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan M_ResponseTimeout = TimeSpan.FromSeconds(40);

        private readonly Uri m_WebSocketUrl;
        private readonly WebSocket m_WebSocket;
        private readonly CompositeDisposable m_Disposable = new CompositeDisposable();

        private int m_RequestCounter;

        public WebSocketJsonRpcClient([NotNull] Uri webSocketUrl)
        {
            m_WebSocketUrl = webSocketUrl;
            m_WebSocket = new WebSocket(webSocketUrl.ToString())
                .ConcatDispose(m_Disposable);
            Observable.FromEventPattern<ErrorEventArgs>(
                    x => m_WebSocket.Error += x, x => m_WebSocket.Error -= x)
                .Subscribe(M_Logger.Error)
                .ConcatDispose(m_Disposable);
        }

        public void StartSession()
        {
            M_Logger.Info($"Opening WebSocket {m_WebSocketUrl}...");
            if (!m_WebSocket.OpenAsync().GetAwaiter().GetResult())
                throw new ExternalDataUnavailableException("Websocket opening failed");
            M_Logger.Info($"WebSocket {m_WebSocketUrl} opened");
        }

        public TResponse Execute<TResponse>(string method, params object[] args)
        {
            var requestId = Interlocked.Increment(ref m_RequestCounter);
            var responseObservable = Observable.FromEventPattern<MessageReceivedEventArgs>(
                    x => m_WebSocket.MessageReceived += x,
                    x => m_WebSocket.MessageReceived -= x)
                .Do(x => M_Logger.Info($"Received from {m_WebSocketUrl}: {x.EventArgs.Message}"))
                .Select(x => JsonConvert.DeserializeObject<dynamic>(x.EventArgs.Message))
                .Where(x => (int?) x.id == requestId)
                .Take(1)
                .Timeout(M_ResponseTimeout);

            var request = JsonConvert.SerializeObject(new
            {
                method,
                @params = args,
                id = requestId
            });
            M_Logger.Info($"Sent to {m_WebSocketUrl}: {request}");
            m_WebSocket.Send(request);
            var response = responseObservable.Wait();
            return ((JToken) response.result).ToObject<TResponse>();
        }

        public void Dispose() 
            => m_Disposable.Dispose();
    }
}
