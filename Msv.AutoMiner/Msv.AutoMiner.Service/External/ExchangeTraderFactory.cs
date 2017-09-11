using System;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.External.Exchanges;
using Msv.AutoMiner.Service.Security.Contracts;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.External
{
    public class ExchangeTraderFactory : IExchangeTraderFactory
    {
        private readonly IStringEncryptor m_KeyEncryptor;
        private readonly IExchangeTraderFactoryStorage m_Storage;

        public ExchangeTraderFactory(IStringEncryptor keyEncryptor, IExchangeTraderFactoryStorage storage)
        {
            m_KeyEncryptor = keyEncryptor ?? throw new ArgumentNullException(nameof(keyEncryptor));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public IExchangeTrader Create(ExchangeType exchangeType)
        {
            var exchange = m_Storage.GetExchange(exchangeType);
            if (exchange == null)
                throw new InvalidOperationException($"Keys for exchange {exchangeType} are not registered");
            var privateKey = m_KeyEncryptor.Decrypt(exchange.PrivateKey);
            switch (exchangeType)
            {
                case ExchangeType.Bittrex:
                    return new BittrexExchangeTrader(exchange.PublicKey, privateKey);
                case ExchangeType.Cryptopia:
                    return new CryptopiaExchangeTrader(exchange.PublicKey, privateKey);
                case ExchangeType.Poloniex:
                    return new PoloniexExchangeTrader(exchange.PublicKey, privateKey);
                case ExchangeType.YoBit:
                    return new YoBitExchangeTrader(exchange.PublicKey, privateKey);
                case ExchangeType.TradeSatoshi:
                    return new TradeSatoshiExchangeTrader(exchange.PublicKey, privateKey);
                case ExchangeType.CoinsMarkets:
                    return new CoinsMarketsExchangeTrader(privateKey, m_Storage.GetCurrenciesForExchange(exchangeType));
                case ExchangeType.Novaexchange:
                    return new NovaexchangeExchangeTrader(exchange.PublicKey, privateKey);
                default:
                    throw new ArgumentOutOfRangeException($"Exchange {exchangeType} is not supported");
            }
        }
    }
}
