using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public static class DbInitializer
    {
        private static readonly Dictionary<KnownCoinAlgorithm, ProfitabilityFormulaType> M_ProfitabilityFormulaTypes =
            new Dictionary<KnownCoinAlgorithm, ProfitabilityFormulaType>
            {
                [KnownCoinAlgorithm.Equihash] = ProfitabilityFormulaType.Special,
                [KnownCoinAlgorithm.EtHash] = ProfitabilityFormulaType.EtHash,
                [KnownCoinAlgorithm.PrimeChain] = ProfitabilityFormulaType.Special,
                [KnownCoinAlgorithm.CryptoNight] = ProfitabilityFormulaType.Special,
                [KnownCoinAlgorithm.Blake2B] = ProfitabilityFormulaType.Special,
                [KnownCoinAlgorithm.Pascal] = ProfitabilityFormulaType.Special,
                [KnownCoinAlgorithm.M7] = ProfitabilityFormulaType.Special
            };

        public static void InitializeIfNotExist(AutoMinerDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.ApiKeys.Any())
                InitializeApiKeys(context);
            if (!context.CoinAlgorithms.Any())
                InitializeAlgorithms(context);
            if (!context.Coins.Any())
            {
                InitializeEssentialCoins(context);
                InitializeOtherCoins(context);
            }
            if (!context.Exchanges.Any())
                InitializeExchanges(context);
            if (!context.Pools.Any())
                InitializePools(context);
            if (!context.Wallets.Any())
                InitializeWallets(context);
        }

        private static void InitializePools(AutoMinerDbContext context)
        {
            var pools = new[]
            {
                new Pool
                {
                    CoinId = new Guid("6042BA1B-9994-4CAF-8DE3-165F685C5C88"),
                    Host = "stratum+tcp://grs.suprnova.cc",
                    Name = "Supernova_Groestl",
                    Port = 5544,
                    WorkerLogin = "msvprogs.ub",
                    WorkerPassword = "1234",
                    IsAnonymous = false,
                    FeeRatio = 8,
                    ApiProtocol = PoolApiProtocol.Qwak,
                    PoolUserId = 11865,
                    ApiKey = "856a3e841c7ec39b16e52e717253b31d1ea2d1f4786567d8a7866f04a81011fc",
                    ApiUrl = "https://grs.suprnova.cc/index.php"
                }
            };

            context.Pools.AddRange(pools);
            context.SaveChanges();
        }

        private static void InitializeWallets(AutoMinerDbContext context)
        {
            var wallets = new[]
            {
                new Wallet
                {
                    Address = "FoMX6WhTorvJK7N2buvythjZznzMExfEue",
                    CoinId = new Guid("6042BA1B-9994-4CAF-8DE3-165F685C5C88"),
                    Created = DateTime.UtcNow,
                    ExchangeType = ExchangeType.Bittrex,
                    IsMiningTarget = true
                }
            };

            context.Wallets.AddRange(wallets);
            context.SaveChanges();
        }

        private static void InitializeApiKeys(AutoMinerDbContext context)
        {
            var keyBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetNonZeroBytes(keyBytes);

            context.ApiKeys.Add(new ApiKey
            {
                Type = ApiKeyType.CoinInfoService,
                Key = HexHelper.ToHex(keyBytes)
            });
            context.SaveChanges();
        }

        private static void InitializeAlgorithms(AutoMinerDbContext context)
        {
            var originalGuid = new Guid("3A2E33C6-5492-457F-95C9-9277DA3FFCD2");
            context.CoinAlgorithms.AddRange(
                EnumHelper.GetValues<KnownCoinAlgorithm>()
                    .Select(x => new CoinAlgorithm
                    {
                        Id = DeriveGuid(originalGuid, x),
                        KnownValue = x,
                        Name = x.ToString(),
                        ProfitabilityFormulaType = M_ProfitabilityFormulaTypes.TryGetValue(x)
                    }));
            context.SaveChanges();

            Guid DeriveGuid(Guid original, KnownCoinAlgorithm algorithm)
            {
                var key = BitConverter.GetBytes(
                    new Random(unchecked(original.GetHashCode() + 371*(int)algorithm)).Next());
                var guidBytes = original.ToByteArray();
                for (var i = 0; i < guidBytes.Length; i++)
                    guidBytes[i] = (byte)(guidBytes[i] ^ key[i % key.Length]);
                return new Guid(guidBytes);
            }
        }

        private static void InitializeEssentialCoins(AutoMinerDbContext context)
        {
            var sha256 = context.CoinAlgorithms.First(x => x.KnownValue == KnownCoinAlgorithm.Sha256D);
            context.Coins.Add(new Coin
            {
                Id = new Guid("752F4D9F-8FDB-43E8-9CFE-64770D979FC3"),
                Name = "BitCoin",
                Symbol = "BTC",
                AlgorithmId = sha256.Id,
                NetworkInfoApiType = CoinNetworkInfoApiType.Special   
            });
            context.FiatCurrencies.Add(new FiatCurrency
            {
                Name = "US Dollar",
                Symbol = "USD"
            });
            context.SaveChanges();
        }

        private static void InitializeExchanges(AutoMinerDbContext context)
        {
            context.Exchanges.AddRange(
                EnumHelper.GetValues<ExchangeType>()
                    .Select(x => new Exchange
                    {
                        Type = x
                    }));
            context.SaveChanges();
        }

        private static void InitializeOtherCoins(AutoMinerDbContext context)
        {
            var algorithms = context.CoinAlgorithms
                .Where(x => x.KnownValue != null)
                .ToDictionary(x => x.KnownValue.GetValueOrDefault(), x => x.Id);

            var coins = new[]
            {
                new Coin
                {
                    Id = new Guid("8C2A151F-CC0F-4F5F-9F6D-4E9EFD111876"),
                    Name = "FeatherCoin",
                    Symbol = "FTC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.NeoScrypt],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("8E9F1DA0-842D-4421-BE57-ABC84E25E6CD"),
                    Name = "Zcash",
                    Symbol = "ZEC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Equihash],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special,
                    SolsPerDiff = 8192
                },
                new Coin
                {
                    Id = new Guid("B0D265D6-CD15-4745-8966-7AD209418940"),
                    Name = "Zclassic",
                    Symbol = "ZCL",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Equihash],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Insight,
                    NetworkInfoApiUrl = "http://zclexplorer.org/api",
                    SolsPerDiff = 8192
                },
                new Coin
                {
                    Id = new Guid("6042BA1B-9994-4CAF-8DE3-165F685C5C88"),
                    Name = "Groestl",
                    Symbol = "GRS",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Groestl],
                    NetworkInfoApiType = CoinNetworkInfoApiType.ChainzCryptoid
                },
                new Coin
                {
                    Id = new Guid("64887260-810C-46B7-8A13-FD5AAA05D39C"),
                    Name = "PhoenixCoin",
                    Symbol = "PXC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.NeoScrypt],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("B98C85D7-F7B3-4CD5-AB1E-60D1385610B5"),
                    Name = "Lbry",
                    Symbol = "LBC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Lbry],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("AD1685AA-F55B-4400-AA68-D9BEA7E9237D"),
                    Name = "Expanse",
                    Symbol = "EXP",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.EtHash],
                    NetworkInfoApiType = CoinNetworkInfoApiType.OpenEthereumPool,
                    NetworkInfoApiUrl = "http://52.165.128.189/api/stats",
                    CanonicalBlockReward = 8,
                    CanonicalBlockTimeSeconds = 60
                },
                new Coin
                {
                    Id = new Guid("69B5916E-3216-478E-8A70-6CF77657364C"),
                    Name = "VertCoin",
                    Symbol = "VTC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Lyra2Rev2],
                    NetworkInfoApiType = CoinNetworkInfoApiType.BchainInfo
                },
                new Coin
                {
                    Id = new Guid("B957B09D-732B-4CDD-8697-6EBD5B11FA82"),
                    Name = "SibCoin",
                    Symbol = "SIB",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.X11Gost],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("C03E0447-D9C8-47EB-82B8-732EA5CEFA74"),
                    Name = "MonaCoin",
                    Symbol = "MONA",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Lyra2Rev2],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Insight,
                    NetworkInfoApiUrl = "https://mona.chainsight.info/api"
                },
                new Coin
                {
                    Id = new Guid("542AAE52-5C56-4181-85F1-53ADFAB2F93A"),
                    Name = "DigiByte-Groestl",
                    Symbol = "DGB",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.MyriadGroestl],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("7A67CE4A-1E89-4C21-89A6-A5C428B83BED"),
                    Name = "Ethereum",
                    Symbol = "ETH",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.EtHash],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special,
                },
                new Coin
                {
                    Id = new Guid("1D30951E-8ACE-45A8-8A8C-3804480E84B1"),
                    Name = "Ubiq",
                    Symbol = "UBQ",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.EtHash],
                    NetworkInfoApiType = CoinNetworkInfoApiType.OpenEthereumPool,
                    NetworkInfoApiUrl = "http://ubiqpool.io/api/stats",
                    CanonicalBlockReward = 8,
                    CanonicalBlockTimeSeconds = 88
                },
                new Coin
                {
                    Id = new Guid("ACCF870D-2612-43AF-963E-50A603509DB7"),
                    Name = "Ethereum Classic",
                    Symbol = "ETC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.EtHash],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("179A5904-4A69-4AD4-917E-B2D81DDBF1B1"),
                    Name = "SiaCoin",
                    Symbol = "SC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Blake2B],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("7B79972A-10FE-4607-B66D-11A87F79001C"),
                    Name = "Decred",
                    Symbol = "DCR",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Blake256With14Rounds],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("842F9400-E2C6-494F-87C1-5551C5EC6A9C"),
                    Name = "OrbitCoin",
                    Symbol = "ORB",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.NeoScrypt],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Zpool
                },
                new Coin
                {
                    Id = new Guid("C2BB696F-C118-4EB8-9D17-2A28CAE3266C"),
                    Name = "BitCore",
                    Symbol = "BTX",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.TimeTravel10],
                    NetworkInfoApiType = CoinNetworkInfoApiType.ChainzCryptoid
                },
                new Coin
                {
                    Id = new Guid("161C39F7-0184-4EBF-80FE-2A7780302682"),
                    Name = "MachineCoin",
                    Symbol = "MAC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.TimeTravel8],
                    NetworkInfoApiType = CoinNetworkInfoApiType.ChainzCryptoid
                },
                new Coin
                {
                    Id = new Guid("C0BBB623-22B7-4ECB-8768-8CCF3471B01A"),
                    Name = "HUSH",
                    Symbol = "HUSH",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Equihash],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Insight,
                    NetworkInfoApiUrl = "https://explorer.myhush.org/api",
                    SolsPerDiff = 2
                },
                new Coin
                {
                    Id = new Guid("0A633D56-32A2-4044-8A2C-55A2380DE951"),
                    Name = "Wyvern",
                    Symbol = "WYV",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Nist5],
                    NetworkInfoApiType = CoinNetworkInfoApiType.ChainzCryptoid
                },
                new Coin
                {
                    Id = new Guid("513F5BB6-12F1-4F38-A673-13FF6FD4CBC5"),
                    Name = "Veltor",
                    Symbol = "VLT",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.ThorsRiddle],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Zpool
                },
                new Coin
                {
                    Id = new Guid("4E4B3AD0-3DB2-4A4A-ABBB-06DC3961FEDA"),
                    Name = "ChainCoin",
                    Symbol = "CHC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.C11],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Zpool
                },
                new Coin
                {
                    Id = new Guid("AC54D9D0-92F6-40B3-B743-18DE73B9A6B2"),
                    Name = "Komodo",
                    Symbol = "KMD",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Equihash],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special,
                    SolsPerDiff = 16
                },
                new Coin
                {
                    Id = new Guid("FE4DC2AF-3C5D-42FC-B6B8-C67229279E26"),
                    Name = "ZeroCoin",
                    Symbol = "XZC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Lyra2Z],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Iquidus,
                    NetworkInfoApiUrl = "http://explorer.zcoin.io"
                },
                new Coin
                {
                    Id = new Guid("C83FCEAF-89CD-4F50-80D9-B8F54210461B"),
                    Name = "Solaris",
                    Symbol = "XLR",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Nist5],
                    NetworkInfoApiType = CoinNetworkInfoApiType.ChainzCryptoid
                },
                new Coin
                {
                    Id = new Guid("501EA4C9-0F66-4636-8935-6237D85D7571"),
                    Name = "ZenCash",
                    Symbol = "ZEN",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Equihash],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Insight,
                    NetworkInfoApiUrl = "https://explorer.zensystem.io/insight-api-zen",
                    SolsPerDiff = 8192
                },
                new Coin
                {
                    Id = new Guid("73568C03-6399-4D77-835E-2670592DDE9C"),
                    Name = "PrimeCoin",
                    Symbol = "XPM",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.PrimeChain],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("BC0FB487-00F3-453B-B95F-7F3FD3AE67F1"),
                    Name = "Digibyte-Skein",
                    Symbol = "DGB",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Skein],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("BCB4C15A-1490-41FF-84B9-4ABDAA6A0AB7"),
                    Name = "Doubloon",
                    Symbol = "BOAT",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Hmq1725],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Zpool
                },
                new Coin
                {
                    Id = new Guid("EC93D37E-D448-40E6-98E2-5C5DC3B67CB7"),
                    Name = "Verge-x17",
                    Symbol = "XVG",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.X17],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Zpool
                },
                new Coin
                {
                    Id = new Guid("ABB2C94E-CC79-4313-BE4A-DEF440C91678"),
                    Name = "Verge-Blake2S",
                    Symbol = "XVG",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Blake2S],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Zpool
                },
                new Coin
                {
                    Id = new Guid("FF859EDF-0271-4C17-A2D3-209DB9DA4570"),
                    Name = "Verge-MyrGr",
                    Symbol = "XVG",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.MyriadGroestl],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Zpool
                },
                new Coin
                {
                    Id = new Guid("31845923-90C1-46C3-80CB-C2C7D6215BD4"),
                    Name = "Verge-LyraV2",
                    Symbol = "XVG",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Lyra2Rev2],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Zpool
                },
                new Coin
                {
                    Id = new Guid("E548BEF6-96F3-44B2-A40E-582C400BE932"),
                    Name = "Honey",
                    Symbol = "HONEY",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Blake2S],
                    NetworkInfoApiType = CoinNetworkInfoApiType.TheCryptoChat,
                    NetworkInfoApiName = "honey"
                },
                new Coin
                {
                    Id = new Guid("7EA23673-A527-405D-8B44-CE6105516BB0"),
                    Name = "RevolverCoin",
                    Symbol = "XRE",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.X11Evo],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Zpool
                },
                new Coin
                {
                    Id = new Guid("6F78D5A3-93D1-4724-A707-C8946CDD426C"),
                    Name = "UfoCoin",
                    Symbol = "UFO",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.NeoScrypt],
                    NetworkInfoApiType = CoinNetworkInfoApiType.ChainzCryptoid
                },
                new Coin
                {
                    Id = new Guid("614F1922-B220-4A5C-B477-BA597CAA9864"),
                    Name = "Denarius",
                    Symbol = "DNR",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Tribus],
                    NetworkInfoApiType = CoinNetworkInfoApiType.IquidusWithPos,
                    NetworkInfoApiUrl = "http://denarius.name:3001"
                },
                new Coin
                {
                    Id = new Guid("28F88B05-1F0B-4CB6-B0F0-A9B5874B9AC8"),
                    Name = "GunCoin",
                    Symbol = "GUN",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.NeoScrypt],
                    NetworkInfoApiType = CoinNetworkInfoApiType.ChainzCryptoid
                },
                new Coin
                {
                    Id = new Guid("3477EEE3-4CA7-476D-81F3-BB38F5C31927"),
                    Name = "PascalCoin",
                    Symbol = "PASC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Pascal],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("654B89C3-08B3-46C1-80CC-05E49C86BADF"),
                    Name = "MaxCoin",
                    Symbol = "MAX",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Keccak],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("4E61EA50-B0B4-49F2-9434-A3594AF241AE"),
                    Name = "BitCoinPlanet",
                    Symbol = "BTPL",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Skein],
                    NetworkInfoApiType = CoinNetworkInfoApiType.TheCryptoChat,
                    NetworkInfoApiName = "bitcoinplanet"
                },
                new Coin
                {
                    Id = new Guid("C3A79637-76EF-423E-8919-5EACA4470562"),
                    Name = "DaxxCoin",
                    Symbol = "DAXX",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.EtHash],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special,
                    CanonicalBlockReward = 30,
                    CanonicalBlockTimeSeconds = 30
                },
                new Coin
                {
                    Id = new Guid("4DABBA18-B1E3-4F21-B01B-2AA56E991048"),
                    Name = "VirtaUniqueCoin",
                    Symbol = "VUC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Nist5],
                    NetworkInfoApiType = CoinNetworkInfoApiType.ChainzCryptoid
                },
                new Coin
                {
                    Id = new Guid("AA30174C-11A9-4512-9606-2DF3C07F2590"),
                    Name = "Cryptonite",
                    Symbol = "XCN",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.M7],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Special
                },
                new Coin
                {
                    Id = new Guid("EA8D7E27-CC53-4F5C-A84D-40814FE095FD"),
                    Name = "Signatum",
                    Symbol = "SIGT",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Skunk],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Iquidus,
                    NetworkInfoApiUrl = "http://explorer.signatum.io/"
                },
                new Coin
                {
                    Id = new Guid("7D25801C-ED82-4CFE-9429-1FCFF7EEDCE2"),
                    Name = "Coimatic 2.0",
                    Symbol = "CTIC2",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Nist5],
                    NetworkInfoApiType = CoinNetworkInfoApiType.IquidusWithPos,
                    NetworkInfoApiUrl = "http://193.70.109.114:4002/"
                },
                new Coin
                {
                    Id = new Guid("4EEEF83B-CE28-401F-8D46-077FD0164661"),
                    Name = "NamoCoin",
                    Symbol = "NAMO",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Nist5],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Iquidus,
                    NetworkInfoApiUrl = "http://namocoin.dynns.com:3001/"
                },
                new Coin
                {
                    Id = new Guid("BF6A64EC-CDBF-482B-BF8F-2A38D30B34BF"),
                    Name = "Coinonat",
                    Symbol = "CXT",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Nist5],
                    NetworkInfoApiType = CoinNetworkInfoApiType.IquidusWithPos,
                    NetworkInfoApiUrl = "http://193.70.109.114:3001/"
                },
                new Coin
                {
                    Id = new Guid("4578A0C1-643C-4366-9B19-7DB1D42E7A6F"),
                    Name = "Magnatum",
                    Symbol = "MGT",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Skunk],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Iquidus,
                    NetworkInfoApiUrl = "http://explorer.magnatum.io/"
                },
                new Coin
                {
                    Id = new Guid("71EC0FEF-3400-4501-B9E0-F2B6BA6B9D70"),
                    Name = "Vivo",
                    Symbol = "VIVO",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.NeoScrypt],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Iquidus,
                    NetworkInfoApiUrl = "http://vivo.explorerz.top:3003/"
                },
                new Coin
                {
                    Id = new Guid("C015D303-138A-44EF-8765-AA55D47326D2"),
                    Name = "TrezarCoin",
                    Symbol = "TZC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.NeoScrypt],
                    NetworkInfoApiType = CoinNetworkInfoApiType.Iquidus,
                    NetworkInfoApiUrl = "http://tzc.explorerz.top:3004/"
                },
                new Coin
                {
                    Id = new Guid("2ACCA68D-CFB4-458C-8ECE-8243BF970853"),
                    Name = "OneCoin",
                    Symbol = "OC",
                    AlgorithmId = algorithms[KnownCoinAlgorithm.Sha256T],
                    NetworkInfoApiType = CoinNetworkInfoApiType.ChainzCryptoid,
                }
            };

            context.Coins.AddRange(coins);
            context.SaveChanges();
        }
    }
}
