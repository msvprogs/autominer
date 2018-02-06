using System;
using System.Linq;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ServerInitializer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Initializing AutoMiner server...");
            var factory = new AutoMinerDbContextFactory("Server=localhost;Database=autominer;Uid=root;Pwd=root;");
            Console.WriteLine("Checking DB...");
            using (var context = factory.Create())
            {
                if (context.Database.EnsureCreated())
                    Console.WriteLine("DB created");
                else
                    Console.WriteLine("DB exists");

                if (!context.CoinAlgorithms.Any())
                    InitializeAlgorithms(context);
                if (!context.Coins.Any())
                    InitializeCoins(context);
                if (!context.FiatCurrencies.Any())
                    InitializeFiatCurrencies(context);
            }
        }

        private static void InitializeCoins(AutoMinerDbContext context)
        {
            Console.WriteLine("Initializing coins...");

            var sha256 = context.CoinAlgorithms.First(x => x.KnownValue == KnownCoinAlgorithm.Sha256D);
            context.Coins.Add(new Coin
            {
                Id = new Guid("752F4D9F-8FDB-43E8-9CFE-64770D979FC3"),
                Name = "BitCoin",
                Symbol = "BTC",
                AlgorithmId = sha256.Id,
                NetworkInfoApiType = CoinNetworkInfoApiType.Special   
            });
            context.SaveChanges();

            Console.WriteLine("Coins initialized");
        }

        private static void InitializeFiatCurrencies(AutoMinerDbContext context)
        {
            Console.WriteLine("Initializing fiat currencies...");

            context.FiatCurrencies.Add(new FiatCurrency
            {
                Name = "US Dollar",
                Symbol = "USD"
            });
            context.SaveChanges();

            Console.WriteLine("Fiat currencies initialized");
        }

        private static void InitializeAlgorithms(AutoMinerDbContext context)
        {
            Console.WriteLine("Initializing algorithms...");

            context.CoinAlgorithms.AddRange(new CoinAlgorithm
                {
                    Id = Guid.Parse("1ad3e5d9-828d-6582-8a1f-6f57c5e901f2"),
                    Name = "Blake256-14",
                    KnownValue = (KnownCoinAlgorithm?) 17
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("785b605d-0709-070a-0e9a-e735416c8990"),
                    Name = "Blake2S",
                    KnownValue = (KnownCoinAlgorithm?) 24
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("114a7268-153c-6e1b-3b88-f65c747e98f9"),
                    Name = "C11",
                    KnownValue = (KnownCoinAlgorithm?) 23
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("196c2328-447c-663d-7bd9-d054342fbef1"),
                    Name = "CryptoNight",
                    KnownValue = (KnownCoinAlgorithm?) 6
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("361eccc7-ab93-494f-9436-a27bdbc0ccde"),
                    Name = "Equihash",
                    KnownValue = (KnownCoinAlgorithm?) 5
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("dfc6f93f-1fe1-479d-ac01-4dd32682041a"),
                    Name = "Equihash-192,7",
                    KnownValue = (KnownCoinAlgorithm?) 46
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("4f31ded5-b981-3060-8624-8d02c9d2e3a7"),
                    Name = "EtHash",
                    KnownValue = (KnownCoinAlgorithm?) 4
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("92aab293-fd5a-4e28-bd8c-ac031b0b542e"),
                    Name = "Fresh"
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("007d111d-7649-7f2c-4eeb-c14d011dafe8"),
                    Name = "Groestl",
                    KnownValue = (KnownCoinAlgorithm?) 7
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("0fe49efb-f9af-70b5-a864-5842e79236e7"),
                    Name = "Hmq1725",
                    KnownValue = (KnownCoinAlgorithm?) 29
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("42979bf3-fca7-3dc6-a061-2b0fef9745aa"),
                    Name = "Hsr"
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("547d0f60-6834-2b2c-33f5-c1197c03afbc"),
                    Name = "Jha",
                    KnownValue = (KnownCoinAlgorithm?) 37
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("45a86b64-0c30-3af9-3791-140878677aad"),
                    Name = "Keccak",
                    KnownValue = (KnownCoinAlgorithm?) 10
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("6291d0cc-698e-4777-9877-a8c9aa479487"),
                    Name = "KeccakC"
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("7e97bdac-daf8-01c6-ff47-2b33b0b14596"),
                    Name = "Lbry",
                    KnownValue = (KnownCoinAlgorithm?) 13
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("2cb95956-3e02-53e8-05a3-05614a556bc4"),
                    Name = "Lyra2Rev2",
                    KnownValue = (KnownCoinAlgorithm?) 11
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("63a8564e-311a-1cf9-1dac-142e525a7a8b"),
                    Name = "Lyra2Z",
                    KnownValue = (KnownCoinAlgorithm?) 25
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("70b9b79d-d0c9-0fe8-ce4d-053d81bb6b98"),
                    Name = "M7",
                    KnownValue = (KnownCoinAlgorithm?) 41
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("f8ba7102-d7c3-4286-9df1-df3f59fac00f"),
                    Name = "Mars",
                    KnownValue = (KnownCoinAlgorithm?) 45
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("48f58182-e6d6-37a4-d17b-49059e8d27a0"),
                    Name = "MyriadGroestl",
                    KnownValue = (KnownCoinAlgorithm?) 15
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("17864fbb-28ef-68d7-e8b5-3a5aa74354ff"),
                    Name = "NeoScrypt",
                    KnownValue = (KnownCoinAlgorithm?) 12
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("476c2e15-4941-383d-46d4-d00a0922beaf"),
                    Name = "Nist5",
                    KnownValue = (KnownCoinAlgorithm?) 21
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("35e489e5-eeb1-4ab5-b673-5878f98536dd"),
                    Name = "Phi1612",
                    KnownValue = (KnownCoinAlgorithm?) 44
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("e4890d92-ca69-440c-a746-3f408f877d61"),
                    Name = "Polytimos"
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("15c2848a-e3de-6a93-d97e-7e58968810fd"),
                    Name = "Scrypt",
                    KnownValue = (KnownCoinAlgorithm?) 1
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("f7f822a3-7b12-4c7c-9a0d-59bf734a6913"),
                    Name = "Scrypt-10"
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("6b4a070e-605a-141b-5dfd-f626120b9883"),
                    Name = "Sha256D",
                    KnownValue = (KnownCoinAlgorithm?) 8
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("59c2e2d1-8585-2693-8218-7e14cdee10b1"),
                    Name = "Sha256T",
                    KnownValue = (KnownCoinAlgorithm?) 31
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("2497a896-cfc2-5bc6-c552-2b698aa445cc"),
                    Name = "Skein",
                    KnownValue = (KnownCoinAlgorithm?) 28
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("5b86a58e-c2da-24d7-dd5f-3a1692a954b3"),
                    Name = "Skunk",
                    KnownValue = (KnownCoinAlgorithm?) 42
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("2e7d1c07-7b53-512c-54e6-c1631b10afc6"),
                    Name = "ThorsRiddle",
                    KnownValue = (KnownCoinAlgorithm?) 22
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("7531ca3f-ad6b-0a60-6c30-8d3823c6e39d"),
                    Name = "TimeTravel10",
                    KnownValue = (KnownCoinAlgorithm?) 19
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("5c1f3820-5f74-234e-73c2-a3113c34cdb4"),
                    Name = "TimeTravel8",
                    KnownValue = (KnownCoinAlgorithm?) 20
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("3f4a7d52-1a06-401b-0187-f6724e7198d7"),
                    Name = "Tribus",
                    KnownValue = (KnownCoinAlgorithm?) 38
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("6d6c197f-7e2b-123d-2ce3-d0206315be85"),
                    Name = "X11Evo",
                    KnownValue = (KnownCoinAlgorithm?) 36
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("61e49391-f4c5-1eb5-c269-582c8d9f3689"),
                    Name = "X11Gost",
                    KnownValue = (KnownCoinAlgorithm?) 14
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("76f58cec-ebb8-09a4-bf76-493bf080279e"),
                    Name = "X17",
                    KnownValue = (KnownCoinAlgorithm?) 30
                },
                new CoinAlgorithm
                {
                    Id = Guid.Parse("e0d53e20-db87-40f3-970a-4efb6ea43cb5"),
                    Name = "Xevan"
                });

            context.SaveChanges();
            Console.WriteLine("Algorithms initialized");
        }
    }
}
