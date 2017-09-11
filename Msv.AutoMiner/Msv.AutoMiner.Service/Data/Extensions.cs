using System;
using System.Linq;
using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Data
{
    public static class Extensions
    {
        public static string GetLogin(this Pool pool, Coin coin = null)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));
            if (!pool.IsAnonymous)
                return pool.WorkerLogin;
            var wallet = coin?.Wallet ?? pool.Coins.FirstOrDefault(x => x.Activity == ActivityState.Active)?.Wallet;
            if (wallet == null)
                return null;
            return string.IsNullOrEmpty(pool.WorkerLogin)
                ? wallet
                : $"{wallet}.{pool.WorkerLogin}";
        }

        public static string GetPassword(this Pool pool)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            return string.IsNullOrEmpty(pool.WorkerPassword) ? "x" : pool.WorkerPassword;
        }
    }
}
