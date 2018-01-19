using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Storage
{
    public class ControlCenterControllerStorage : IControlCenterControllerStorage
    {
        private readonly AutoMinerDbContext m_Context;

        public ControlCenterControllerStorage(AutoMinerDbContext context) 
            => m_Context = context;

        public Task<Rig> GetRigByName(string name)
            => m_Context.Rigs.FirstOrDefaultAsync(x => x.Name == name);

        public async Task SaveRig(Rig rig)
        {
            if (rig == null)
                throw new ArgumentNullException(nameof(rig));

            m_Context.Attach(rig);
            await m_Context.SaveChangesAsync();
        }

        public async Task SaveHeartbeat(RigHeartbeat heartbeat)
        {
            if (heartbeat == null)
                throw new ArgumentNullException(nameof(heartbeat));

            await m_Context.RigHeartbeats.AddAsync(heartbeat);
            await m_Context.SaveChangesAsync();
        }

        public async Task SaveMiningStates(RigMiningState[] miningStates)
        {
            if (miningStates == null)
                throw new ArgumentNullException(nameof(miningStates));

            await m_Context.RigMiningStates.AddRangeAsync(miningStates);
            await m_Context.SaveChangesAsync();
        }

        public async Task<RigCommand> GetNextCommand(int rigId)
            => await m_Context.RigCommands
                .AsNoTracking()
                .Where(x => x.RigId == rigId && x.Sent == null)
                .OrderBy(x => x.Created)
                .FirstOrDefaultAsync();

        public async Task MarkCommandAsSent(int commandId)
        {
            var command = await m_Context.RigCommands.FirstAsync(x => x.Id == commandId);
            command.Sent = DateTime.UtcNow;
            await m_Context.SaveChangesAsync();
        }

        public async Task<Pool[]> GetActivePools(Guid[] coinIds)
        {
            if (coinIds == null)
                throw new ArgumentNullException(nameof(coinIds));

            return await m_Context.Pools
                .Include(x => x.Coin)
                .Include(x => x.Coin.Wallets)
                .AsNoTracking()
                .Where(x => coinIds.Contains(x.CoinId) && x.Activity == ActivityState.Active)
                .ToArrayAsync();
        }

        public async Task SaveProfitabilities(CoinProfitability[] profitabilities)
        {
            if (profitabilities == null)
                throw new ArgumentNullException(nameof(profitabilities));

            await m_Context.CoinProfitabilities.AddRangeAsync(profitabilities);
            await m_Context.SaveChangesAsync();
        }

        public Task<Wallet> GetBitCoinMiningTarget() 
            => m_Context.Wallets.FirstOrDefaultAsync(x => x.IsMiningTarget && x.Coin.Symbol == "BTC");
    }
}
