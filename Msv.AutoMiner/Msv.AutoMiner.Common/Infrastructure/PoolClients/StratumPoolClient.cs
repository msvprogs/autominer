using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using JetBrains.Annotations;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Msv.AutoMiner.Common.Infrastructure.PoolClients
{
    public class StratumPoolClient : IPoolClient
    {
        private static readonly TimeSpan M_SocketTimeout = TimeSpan.FromSeconds(25);
        private static readonly Encoding M_StratumEncoding = Encoding.ASCII;

        private readonly PoolDataModel m_Pool;
        private readonly KnownCoinAlgorithm? m_KnownCoinAlgorithm;
        private readonly ILogger m_Logger;

        public StratumPoolClient(
            [NotNull] PoolDataModel pool, KnownCoinAlgorithm? knownCoinAlgorithm, [NotNull] ILogger logger)
        {
            m_Pool = pool ?? throw new ArgumentNullException(nameof(pool));
            m_KnownCoinAlgorithm = knownCoinAlgorithm;
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public PoolAvailabilityState CheckAvailability()
        {
            using (var client = new TcpClient())
                return CheckStratumServer(client);
        }

        private PoolAvailabilityState CheckStratumServer(TcpClient client)
        {
            var poolString = $"Pool {m_Pool.Name} ({m_Pool.Url})";
            client.ReceiveTimeout = client.SendTimeout = (int) M_SocketTimeout.TotalMilliseconds;
            client.Connect(m_Pool.Url.Host, m_Pool.Url.Port);
            m_Logger.Info($"{poolString}: connection succeeded");
            if (m_Pool.Login == null)
                return PoolAvailabilityState.AuthenticationFailed;

            var requestId = new Random().Next();
            var authRequest = JsonConvert.SerializeObject(CreateAuthRequest(requestId));
            using (var stream = client.GetStream())
            {
                m_Logger.Info($"{poolString}: sending Stratum request {authRequest}");
                var bytes = M_StratumEncoding.GetBytes(authRequest + "\n");
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();

                dynamic response;
                do
                {
                    var responseStr = ReadStratumLine(stream);
                    m_Logger.Info($"{poolString}: received Stratum response {responseStr}");
                    response = JsonConvert.DeserializeObject(responseStr);
                } while ((int?) response.id != requestId
                         && response.method != null);

                if ((int?) response.id != requestId)
                    return PoolAvailabilityState.NoResponse;
                return response.result is JObject
                       && (string) response.result.status == "OK"
                       || (bool?) response.result == true
                    ? PoolAvailabilityState.Available
                    : PoolAvailabilityState.AuthenticationFailed;
            }
        }

        private object CreateAuthRequest(int requestId)
        {
            switch (m_KnownCoinAlgorithm)
            {
                case KnownCoinAlgorithm.EtHash:
                    return new
                    {
                        @params = new[] {m_Pool.Login, m_Pool.Password},
                        id = requestId,
                        method = "eth_submitLogin",
                        jsonrpc = "2.0",
                        worker = "eth1.0"
                    };
                case KnownCoinAlgorithm.CryptoNight:
                    return new
                    {
                        @params = new
                        {
                            login = m_Pool.Login,
                            pass = m_Pool.Password
                        },
                        id = requestId,
                        method = "login",
                        jsonrpc = "2.0"
                    };
                default:
                    return new
                    {
                        @params = new[] {m_Pool.Login, m_Pool.Password},
                        id = requestId,
                        method = "mining.authorize"
                    };
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
    }
}
