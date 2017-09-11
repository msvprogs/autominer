using System;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.Storage
{
    public class ExchangeKeyStorage : IExchangeKeyStorage
    {
        public void StoreKey(ExchangeType exchangeType, string publicKey, byte[] privateKey)
        {
            if (string.IsNullOrEmpty(publicKey))
                throw new ArgumentException("Value cannot be null or empty.", nameof(publicKey));
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));

            using (var context = new AutoMinerDbContext())
            {
                var exchange = context.Exchanges.FirstOrDefault(x => x.Type == exchangeType)
                               ?? context.Exchanges.Add(new Exchange {Type = exchangeType});
                exchange.PublicKey = publicKey;
                exchange.PrivateKey = privateKey;
                context.SaveChanges();
            }
        }
    }
}
