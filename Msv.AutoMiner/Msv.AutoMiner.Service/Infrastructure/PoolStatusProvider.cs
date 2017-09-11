using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Rpc;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using Msv.AutoMiner.Service.Storage.Contracts;
using Newtonsoft.Json;
using NLog;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public class PoolStatusProvider : IPoolStatusProvider
    {
        private static readonly ILogger M_Logger = LogManager.GetLogger("PoolStatusProvider");
        private static readonly TimeSpan M_RecheckInterval = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan M_SocketTimeout = TimeSpan.FromSeconds(25);
        private static readonly Encoding M_StratumEncoding = Encoding.ASCII;

        private readonly IPoolStatusProviderStorage m_Storage;

        public PoolStatusProvider(IPoolStatusProviderStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            m_Storage = storage;
        }

        public bool CheckAvailability(int poolId)
        {
            var pool = m_Storage.GetPool(poolId);
            if (pool == null)
                return false;
            if (pool.ResponsesStoppedDate != null
                && pool.ResponsesStoppedDate.Value + M_RecheckInterval > DateTime.Now)
            {
                M_Logger.Warn($"Pool {pool.Name} is still unavailable");
                return true;
            }
            var result = CheckServer(pool);
            if (result)
            {
                M_Logger.Info($"Pool {pool.Name} is available, connection & authorization succeeded");
                pool.ResponsesStoppedDate = null;
            }
            else if (pool.ResponsesStoppedDate == null)
                pool.ResponsesStoppedDate = DateTime.Now;
            m_Storage.SavePool(pool);
            return result;
        }

        private static bool CheckServer(Pool pool)
        {
            var host = Uri.IsWellFormedUriString(pool.Address, UriKind.Absolute)
                ? new Uri(pool.Address).Host
                : pool.Address;
            try
            {
                switch (pool.Protocol)
                {
                    case PoolProtocol.Stratum:
                        return CheckStratumServer(pool, host);
                    case PoolProtocol.JsonRpc:
                        return CheckJsonRpcServer(pool);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pool), "Unknown pool protocol");
                }
            }
            catch (Exception ex)
            {
                M_Logger.Error($"Pool {pool.Name} ({host}:{pool.Port}) didn't respond. {ex.Message}");
                return false;
            }
        }

        private static bool CheckStratumServer(Pool pool, string host)
        {
            using (var client = new TcpClient())
            {
                client.ReceiveTimeout = client.SendTimeout = (int)M_SocketTimeout.TotalMilliseconds;
                client.Connect(host, pool.Port);
                M_Logger.Info($"Pool {pool.Name} ({host}:{pool.Port}): connection succeeded");
                var login = pool.GetLogin();
                if (login == null)
                    return false;
                const int requestId = 2;
                var authRequest = JsonConvert.SerializeObject(new
                {
                    @params = new[] { login, pool.GetPassword() },
                    id = requestId,
                    method = "mining.authorize"
                });
                using (var stream = client.GetStream())
                {
                    M_Logger.Info($"Pool {pool.Name} ({host}:{pool.Port}): sending Stratum request {authRequest}");
                    var bytes = M_StratumEncoding.GetBytes(authRequest + "\n");
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                    var responseStr = ReadStratumLine(stream);
                    M_Logger.Info($"Pool {pool.Name} ({host}:{pool.Port}): received Stratum response {responseStr}");
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

        private static bool CheckJsonRpcServer(Pool pool)
        {
            var client = new JsonRpcClient(pool.Address, pool.Port, pool.WorkerLogin, pool.WorkerPassword);
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
