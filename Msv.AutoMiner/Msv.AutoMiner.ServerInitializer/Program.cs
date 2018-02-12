using System;
using System.Linq;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ServerInitializer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Initializing AutoMiner server...");
            var factory = new AutoMinerDbContextFactory("Server=localhost;Database=autominer;Uid=miner;Pwd=hnvutibh6giwf4q;");
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
                if (!context.Users.Any())
                    InitializeUsers(context);
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void InitializeUsers(AutoMinerDbContext context)
        {
            Console.WriteLine("Initializing users...");

            var rng = new CryptoRandomGenerator();
            var hasher = new Sha256PasswordHasher();
            var newPassword = Base58.Encode(rng.GenerateRandom(10));
            var salt = rng.GenerateRandom(32);
            var admin = context.Users.Add(new User
            {
                Login = "admin",
                Salt = salt,
                PasswordHash = hasher.CalculateHash(newPassword, salt)
            });
            context.SaveChanges();
            Console.WriteLine($"ADMIN CREDENTIALS - login: {admin.Entity.Login}, password: {newPassword}");
            Console.WriteLine("Do not lose them, or you won't be able to login to the frontend.");
            Console.WriteLine("Users initialized");
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
                NetworkInfoApiType = CoinNetworkInfoApiType.Special,
                LogoImageBytes = HexHelper.FromHex("0x89504E470D0A1A0A0000000D49484452000000100000001008060000001FF3FF61000002DE49444154789C65934D689C5514869F73EFFDBEF9B169126934263525A08876634BA0C6142776A514DC683711172E4C68DABA5170A7B8D74517B65A114154B028085A44C1DA598848DD087621FED406994A2AFD49279999FBDD7B8E8B41545C1C388BF3F0C2797904C00C11C100E2878F2E78EF9755ADA5D9A600C44BA77072AECAF9CDF289CFBEFE37237F2F17DF6ED5778E345E1164C517122C2A292B98128A1229849C7232B353E166FF3979BADD3743C44E1FF24CAC17D595E24CB1AD3C30B83E304C1570E28210EA58EFAA214EF135571B2BA5EAC6B3C54475902BB75502D07F77F1446DBC767870B51F45280975883791C9BDB89987B1CBE721F5C99D6FC1D7636DBC2CE38D78A2B6F4D51189EFEFDF07EE1B5353C379AA2EEECE87087B57413C36B88ED44649DF1D477FFB129A132696559C38D079A7D98E15C1447302AD2034D0B536BAD646C6660181C6ADF8FB97913BE620764555193276CCA1A9A5B10253876530C5AA4DA88F43EA113F3E44FCE011DC8EDD140B2F8246B0EC3456A0D56240F364A520302C520C2947709373E0CB2194FA90FAE8FAF7A00931952A296636E94C930C9333006CAD23E508B27D9AFCEBE7D86003BF7B092C932FBC0766C35B1D8E13CB7F14A298A9516DE2A6E7F17B56401CF98777A83E3D4AFCE429080DFC7D4B506D61AA5688E2D0CB01D3732EF0247D53F2C0CBF43C6EE77E40080FBC80BFEB31DCED7B401C76ED673005CDEA4AF3D6D7B6C4B7EEDD87D9B04609CEBA1DF1F73C4E987B168B5D646C16BBF60BF6E705D2F9E3C31F3897C5E12AE54101D87AFDEED71AA37EB577234752AFF4332D084DFC4C8BFCE347E48B5F20B5ED109A18121B63A1EC6DE493CD959F56C54EE3D9DC55C418CE94DBE4406FC38CD4574C9D347708A987190628A6AE31EA2476ED6C59A683DC72A9FA47A69776D5A7A6ED55672C87E0434E90AA0AC411BCE00B21254BC01BBF77787EF6E54B4399FEA7F3C99905EFF5999459D4CC1418DE4B471C6D5377AA3CBCF61F9DFF02C871950CDB36E8640000000049454E44AE426082")
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
                    Id = Guid.Parse("F2BBF34F-01F0-42C1-A6C6-9998ED0E8AD9"),
                    Name = "PrimeChain",
                    KnownValue = KnownCoinAlgorithm.PrimeChain
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
