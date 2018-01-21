using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Newtonsoft.Json;
using NLog;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public class PoolAvailabilityChecker : IPoolAvailabilityChecker
    {
        private static readonly TimeSpan M_SocketTimeout = TimeSpan.FromSeconds(25);
        private static readonly Encoding M_StratumEncoding = Encoding.ASCII;

        protected static ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        public virtual bool Check(PoolDataModel pool)
        {
            var watch = Stopwatch.StartNew();
            var result = CheckServer(pool);
            if (!result) 
                return false;
            Logger.Info($"Pool {pool.Name} is available, connection & authorization succeeded (response time: {watch.ElapsedMilliseconds} msec");
            return true;
        }

        private static bool CheckServer(PoolDataModel pool)
        {
            try
            {
                Logger.Info($"Pool {pool.Name} ({pool.Url}): checking availability...");
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
                Logger.Error($"Pool {pool.Name} ({pool.Url}) didn't respond. {ex.Message}");
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
                Logger.Info($"{poolString}: connection succeeded");
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
                    Logger.Info($"{poolString}: sending Stratum request {authRequest}");
                    var bytes = M_StratumEncoding.GetBytes(authRequest + "\n");
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                    var responseStr = ReadStratumLine(stream);
                    Logger.Info($"{poolString}: received Stratum response {responseStr}");
                    var response = (dynamic)JsonConvert.DeserializeObject(responseStr);
                    return (string)response.method == "mining.set_difficulty"
                        || (string)response.method == "mining.notify"
                        || (string)response.method == "client.show_message" //well, it could be message like "I'm not working now"...
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
