using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using JetBrains.Annotations;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.HttpTools;
using Newtonsoft.Json;
using NLog;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public class PoolAvailabilityChecker : IPoolAvailabilityChecker
    {       
        protected static ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        private static readonly TimeSpan M_SocketTimeout = TimeSpan.FromSeconds(25);
        private static readonly Encoding M_StratumEncoding = Encoding.ASCII;

        private readonly IWebClient m_WebClient;

        public PoolAvailabilityChecker([NotNull] IWebClient webClient) 
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public virtual PoolAvailabilityState Check(PoolDataModel pool, KnownCoinAlgorithm? knownCoinAlgorithm)
        {
            var watch = Stopwatch.StartNew();
            var result = CheckServer(pool, knownCoinAlgorithm);
            if (result != PoolAvailabilityState.Available) 
                return result;
            Logger.Info($"Pool {pool.Name} is available, connection & authorization succeeded (response time: {watch.ElapsedMilliseconds} msec)");
            return PoolAvailabilityState.Available;
        }

        private PoolAvailabilityState CheckServer(PoolDataModel pool, KnownCoinAlgorithm? knownCoinAlgorithm)
        {
            try
            {
                Logger.Info($"Pool {pool.Name} ({pool.Url}): checking availability...");
                switch (pool.Protocol)
                {
                    case PoolProtocol.Stratum:
                        using (var client = new TcpClient())
                            return CheckStratumServer(client, pool, knownCoinAlgorithm);
                    case PoolProtocol.JsonRpc:
                        return CheckJsonRpcServer(pool);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pool), "Unknown pool protocol");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Pool {pool.Name} ({pool.Url}) didn't respond. {ex.Message}");
                return PoolAvailabilityState.NoResponse;
            }
        }

        private static PoolAvailabilityState CheckStratumServer(
            TcpClient client, PoolDataModel pool, KnownCoinAlgorithm? knownCoinAlgorithm)
        {
            var poolString = $"Pool {pool.Name} ({pool.Url})";
            client.ReceiveTimeout = client.SendTimeout = (int) M_SocketTimeout.TotalMilliseconds;
            client.Connect(pool.Url.Host, pool.Url.Port);
            Logger.Info($"{poolString}: connection succeeded");
            if (pool.Login == null)
                return PoolAvailabilityState.AuthenticationFailed;

            var requestId = new Random().Next();
            object request;
            switch (knownCoinAlgorithm)
            {
                case KnownCoinAlgorithm.EtHash:
                    request = new
                    {
                        @params = new[] {pool.Login, pool.Password},
                        id = requestId,
                        method = "eth_submitLogin",
                        jsonrpc = "2.0",
                        worker = "eth1.0"
                    };
                    break;
                default:
                    request = new
                    {
                        @params = new[] {pool.Login, pool.Password},
                        id = requestId,
                        method = "mining.authorize"
                    };
                    break;
            }

            var authRequest = JsonConvert.SerializeObject(request);
            using (var stream = client.GetStream())
            {
                Logger.Info($"{poolString}: sending Stratum request {authRequest}");
                var bytes = M_StratumEncoding.GetBytes(authRequest + "\n");
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();

                dynamic response;
                do
                {
                    var responseStr = ReadStratumLine(stream);
                    Logger.Info($"{poolString}: received Stratum response {responseStr}");
                    response = JsonConvert.DeserializeObject(responseStr);
                } while ((int?) response.id != requestId
                         && response.method != null);

                if ((int?) response.id != requestId)
                    return PoolAvailabilityState.NoResponse;
                return (bool?) response.result == true
                    ? PoolAvailabilityState.Available
                    : PoolAvailabilityState.AuthenticationFailed;
            }
        }

        private static string ReadStratumLine(Stream stream)
        {
            var bytes = new List<byte>();
            while (true)
            {
                var value = stream.ReadByte();
                if (value < 0 || value == '\n')
                    return M_StratumEncoding.GetString(bytes.ToArray());
                bytes.Add((byte)value);
            }
        }

        private PoolAvailabilityState CheckJsonRpcServer(PoolDataModel pool)
        {
            var client = new JsonRpcClient(m_WebClient, pool.Url.Host, pool.Url.Port, pool.Login, pool.Password);
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
