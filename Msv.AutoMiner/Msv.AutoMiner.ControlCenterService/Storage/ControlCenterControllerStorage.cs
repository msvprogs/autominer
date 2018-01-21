using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ControlCenterService.Storage
{
    public class ControlCenterControllerStorage : IControlCenterControllerStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public ControlCenterControllerStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Task<Rig> GetRigByName(string name)
        {
            using (var context = m_Factory.Create())
                return context.Rigs.FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task SaveRig(Rig rig)
        {
            if (rig == null)
                throw new ArgumentNullException(nameof(rig));

            using (var context = m_Factory.Create())
            {
                context.Attach(rig);
                await context.SaveChangesAsync();
            }
        }

        public async Task SaveHeartbeat(RigHeartbeat heartbeat)
        {
            if (heartbeat == null)
                throw new ArgumentNullException(nameof(heartbeat));

            using (var context = m_Factory.Create())
            {
                await context.RigHeartbeats.AddAsync(heartbeat);
                await context.SaveChangesAsync();
            }
        }

        public async Task SaveMiningStates(RigMiningState[] miningStates)
        {
            if (miningStates == null)
                throw new ArgumentNullException(nameof(miningStates));

            using (var context = m_Factory.Create())
            {
                await context.RigMiningStates.AddRangeAsync(miningStates);
                await context.SaveChangesAsync();
            }
        }

        public async Task<RigCommand> GetNextCommand(int rigId)
        {
            using (var context = m_Factory.Create())
            {
                return await context.RigCommands
                    .AsNoTracking()
                    .Where(x => x.RigId == rigId && x.Sent == null)
                    .OrderBy(x => x.Created)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task MarkCommandAsSent(int commandId)
        {
            using (var context = m_Factory.Create())
            {
                var command = await context.RigCommands.FirstAsync(x => x.Id == commandId);
                command.Sent = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task<Pool[]> GetActivePools(Guid[] coinIds)
        {
            if (coinIds == null)
                throw new ArgumentNullException(nameof(coinIds));

            using (var context = m_Factory.Create())
            {
                return await context.Pools
                    .Include(x => x.Coin)
                    .Include(x => x.Coin.Wallets)
                    .AsNoTracking()
                    .Where(x => coinIds.Contains(x.CoinId) && x.Activity == ActivityState.Active)
                    .ToArrayAsync();
            }
        }

        public async Task SaveProfitabilities(CoinProfitability[] profitabilities)
        {
            if (profitabilities == null)
                throw new ArgumentNullException(nameof(profitabilities));

            using (var context = m_Factory.Create())
            {
                await context.CoinProfitabilities.AddRangeAsync(profitabilities);
                await context.SaveChangesAsync();
            }
        }

        public Task<Wallet> GetBitCoinMiningTarget()
        {
            using (var context = m_Factory.Create())
                return context.Wallets.FirstOrDefaultAsync(x => x.IsMiningTarget && x.Coin.Symbol == "BTC");
        }
    }
}
