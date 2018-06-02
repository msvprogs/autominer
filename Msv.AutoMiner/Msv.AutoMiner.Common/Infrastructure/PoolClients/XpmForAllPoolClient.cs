using System;
using System.Text;
using JetBrains.Annotations;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using NetMQ;
using NetMQ.Sockets;
using NLog;

namespace Msv.AutoMiner.Common.Infrastructure.PoolClients
{
    public class XpmForAllPoolClient : IPoolClient
    {
        private const int ConnectionAttempts = 5;

        private static readonly TimeSpan M_SocketTimeout = TimeSpan.FromSeconds(7);
        private static readonly Encoding M_Encoding = Encoding.ASCII;
        private static readonly byte[] M_MagicBytes = HexHelper.FromHex(
            "08011001500b580062207f77000071120000280800008b1000003b4300004f6e00009d6f0000385678a0");

        private readonly PoolDataModel m_Pool;
        private readonly ILogger m_Logger;

        public XpmForAllPoolClient([NotNull] PoolDataModel pool, [NotNull] ILogger logger)
        {
            m_Pool = pool ?? throw new ArgumentNullException(nameof(pool));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public PoolAvailabilityState CheckAvailability()
        {
            var poolString = $"Pool {m_Pool.Name} ({m_Pool.Url})";
            for (var i = 0; i < ConnectionAttempts; i++)
            {
                using (var socket = new DealerSocket
                {
                    Options = {Linger = TimeSpan.Zero}
                })
                {
                    m_Logger.Info($"{poolString}: connecting with ZeroMQ Dealer socket, attempt {i}...");
                    socket.Connect(m_Pool.Url.ToString().TrimEnd('/'));
                    m_Logger.Info($"{poolString}: ZeroMQ socket connected");
                    m_Logger.Info($"{poolString}: sending magic bytes {BitConverter.ToString(M_MagicBytes)}...");
                    socket.SendFrame(M_MagicBytes);
                    if (!socket.TryReceiveFrameBytes(M_SocketTimeout, out var response))
                    {
                        m_Logger.Warn($"{poolString}: no response from the server socket");
                        continue;
                    }
                    m_Logger.Info($"{poolString}: received response {BitConverter.ToString(response)}...");
                    return M_Encoding.GetString(response).Contains(m_Pool.Url.Host.ToLowerInvariant())
                        ? PoolAvailabilityState.Available
                        : PoolAvailabilityState.AuthenticationFailed;
                }
            }

            return PoolAvailabilityState.NoResponse;
        }
    }
}
