using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Newtonsoft.Json;
using NLog;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class PoolStatusProvider : IPoolStatusProvider
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan M_RecheckInterval = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan M_SocketTimeout = TimeSpan.FromSeconds(25);
        private static readonly Encoding M_StratumEncoding = Encoding.ASCII;

        private readonly ConcurrentDictionary<int, DateTime> m_ResponsesStoppedTimes =
            new ConcurrentDictionary<int, DateTime>();

        public bool CheckAvailability(PoolDataModel pool)
        {
            if (m_ResponsesStoppedTimes.TryGetValue(pool.Id, out var stoppedTime)
                && stoppedTime + M_RecheckInterval > DateTime.Now)
            {
                M_Logger.Warn($"Pool {pool.Name} is still unavailable");
                return false;
            }
            var result = CheckServer(pool);
            if (result)
            {
                M_Logger.Info($"Pool {pool.Name} is available, connection & authorization succeeded");
                m_ResponsesStoppedTimes.TryRemove(pool.Id, out _);
            }
            else
                m_ResponsesStoppedTimes.AddOrUpdate(pool.Id, x => DateTime.Now, (x, y) => y);
            return result;
        }

        private static bool CheckServer(PoolDataModel pool)
        {
            try
            {
                M_Logger.Info($"Pool {pool.Name} ({pool.Url}): connecting...");
                switch (pool.Protocol)
                {
                    case PoolProtocol.Stratum:
                        return CheckStratumServer(pool);
                    case PoolProtocol.JsonRpc:
                        return CheckJsonRpcServer(pool);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pool), "Unknown pool protocol");
                }
            }
            catch (Exception ex)
            {
                M_Logger.Error($"Pool {pool.Name} ({pool.Url}) didn't respond. {ex.Message}");
                return false;
            }
        }

        private static bool CheckStratumServer(PoolDataModel pool)
        {
            using (var client = new TcpClient())
            {
                var poolString = $"Pool {pool.Name} ({pool.Url})";
                client.ReceiveTimeout = client.SendTimeout = (int)M_SocketTimeout.TotalMilliseconds;
                client.Connect(pool.Url.Host, pool.Url.Port);
                M_Logger.Info($"{poolString}: connection succeeded");
                if (pool.Login == null)
                    return false;
                const int requestId = 2;
                var authRequest = JsonConvert.SerializeObject(new
                {
                    @params = new[] { pool.Login, pool.Password },
                    id = requestId,
                    method = "mining.authorize"
                });
                using (var stream = client.GetStream())
                {
                    M_Logger.Info($"{poolString}: sending Stratum request {authRequest}");
                    var bytes = M_StratumEncoding.GetBytes(authRequest + "\n");
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                    var responseStr = ReadStratumLine(stream);
                    M_Logger.Info($"{poolString}: received Stratum response {responseStr}");
                    var response = (dynamic)JsonConvert.DeserializeObject(responseStr);
                    return (string)response.method == "mining.set_difficulty"
                        || (string)response.method == "mining.notify"
                        || (int?)response.id == requestId 
                        && (bool?)response.result == true;
                }
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

        private static bool CheckJsonRpcServer(PoolDataModel pool)
        {
            var client = new JsonRpcClient(pool.Url.Host, pool.Url.Port, pool.Login, pool.Password);
            try
            {
                client.Execute<object>("ping");
            }
            catch (WebException wex) when (wex.Status == WebExceptionStatus.ProtocolError)
            {
                client.Execute<object>("getinfo");
            }
            return true;
        }
    }
}
