using System;
using System.Data.Entity;
using System.Linq;
using SQLite.CodeFirst;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class DbInitializer : SqliteCreateDatabaseIfNotExists<AutoMinerRigDbContext>
    {
        private const string CcminerSphash = "Ccminer Sphash";
        private const string CcminerTpruvot = "Ccminer Tpruvot";
        private const string CcminerAlexis = "Ccminer Alexis";
        private const string CcminerM7 = "Ccminer M7";
        private const string EwbfEquihash = "EWBF Equihash";

        public DbInitializer(DbModelBuilder modelBuilder) 
            : base(modelBuilder)
        { }

        public override void InitializeDatabase(AutoMinerRigDbContext context)
        {
            base.InitializeDatabase(context);

            if (!context.Miners.Any())
                InitializeMiners(context);
            if (!context.MinerAlgorithmSettings.Any())
                InitializeSettings(context);
        }

        private static void InitializeSettings(AutoMinerRigDbContext context)
        {
            var miners = context.Miners.ToDictionary(x => x.Name);
            context.MinerAlgorithmSettings.AddRange(
                new[]
                {
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("007d111d-7649-7f2c-4eeb-c14d011dafe8"),
                        MinerId = miners[CcminerSphash].Id,
                        AlgorithmArgument = "groestl"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("0fe49efb-f9af-70b5-a864-5842e79236e7"),
                        MinerId = miners[CcminerTpruvot].Id,
                        AlgorithmArgument = "hmq1725"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("114a7268-153c-6e1b-3b88-f65c747e98f9"),
                        MinerId = miners[CcminerAlexis].Id,
                        AlgorithmArgument = "c11",
                        Intensity = 21.5
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("17864fbb-28ef-68d7-e8b5-3a5aa74354ff"),
                        MinerId = miners[CcminerSphash].Id,
                        AlgorithmArgument = "neoscrypt"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("2497a896-cfc2-5bc6-c552-2b698aa445cc"),
                        MinerId = miners[CcminerAlexis].Id,
                        AlgorithmArgument = "skein"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("2cb95956-3e02-53e8-05a3-05614a556bc4"),
                        MinerId = miners[CcminerAlexis].Id,
                        AlgorithmArgument = "lyra2v2"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("2e7d1c07-7b53-512c-54e6-c1631b10afc6"),
                        MinerId = miners[CcminerAlexis].Id,
                        AlgorithmArgument = "veltor"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("361eccc7-ab93-494f-9436-a27bdbc0ccde"),
                        MinerId = miners[EwbfEquihash].Id,
                        LogFile = "/var/log/mining/ewbf.log"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("3f4a7d52-1a06-401b-0187-f6724e7198d7"),
                        MinerId = miners[CcminerTpruvot].Id,
                        AlgorithmArgument = "tribus"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("45a86b64-0c30-3af9-3791-140878677aad"),
                        MinerId = miners[CcminerAlexis].Id,
                        AlgorithmArgument = "keccak"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("476c2e15-4941-383d-46d4-d00a0922beaf"),
                        MinerId = miners[CcminerAlexis].Id,
                        AlgorithmArgument = "nist5"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("48f58182-e6d6-37a4-d17b-49059e8d27a0"),
                        MinerId = miners[CcminerAlexis].Id,
                        AlgorithmArgument = "myr-gr"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("59c2e2d1-8585-2693-8218-7e14cdee10b1"),
                        MinerId = miners[CcminerTpruvot].Id,
                        AlgorithmArgument = "sha256t"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("5b86a58e-c2da-24d7-dd5f-3a1692a954b3"),
                        MinerId = miners[CcminerTpruvot].Id,
                        AlgorithmArgument = "skunk",
                        Intensity = 25
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("5c1f3820-5f74-234e-73c2-a3113c34cdb4"),
                        MinerId = miners[CcminerTpruvot].Id,
                        AlgorithmArgument = "timetravel"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("61e49391-f4c5-1eb5-c269-582c8d9f3689"),
                        MinerId = miners[CcminerAlexis].Id,
                        AlgorithmArgument = "sib",
                        Intensity = 21
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("63a8564e-311a-1cf9-1dac-142e525a7a8b"),
                        MinerId = miners[CcminerTpruvot].Id,
                        AlgorithmArgument = "lyra2z"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("6d6c197f-7e2b-123d-2ce3-d0206315be85"),
                        MinerId = miners[CcminerTpruvot].Id,
                        AlgorithmArgument = "x11evo"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("70b9b79d-d0c9-0fe8-ce4d-053d81bb6b98"),
                        MinerId = miners[CcminerM7].Id,
                        AlgorithmArgument = "m7"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("7531ca3f-ad6b-0a60-6c30-8d3823c6e39d"),
                        MinerId = miners[CcminerTpruvot].Id,
                        AlgorithmArgument = "bitcore"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("76f58cec-ebb8-09a4-bf76-493bf080279e"),
                        MinerId = miners[CcminerAlexis].Id,
                        AlgorithmArgument = "x17",
                        Intensity = 21.5
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("785b605d-0709-070a-0e9a-e735416c8990"),
                        MinerId = miners[CcminerAlexis].Id,
                        AlgorithmArgument = "blake2s"
                    },
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("7e97bdac-daf8-01c6-ff47-2b33b0b14596"),
                        MinerId = miners[CcminerAlexis].Id,
                        AlgorithmArgument = "lbry"
                    },
                });
            context.SaveChanges();
        }

        private static void InitializeMiners(AutoMinerRigDbContext context)
        {
            context.Miners.AddRange(
                new[]
                {
                    new Miner
                    {
                        Name = CcminerSphash,
                        FileName = @"/home/yamamoto/Mine/ccminer/ccminer",
                        ServerArgument = "-o",
                        UserArgument = "-u",
                        PasswordArgument = "-p",
                        SpeedRegex = @"(?<=accepted:).*?,\s(?<speed>\d+(\.\d+)?\s[kMGT]H/s)",
                        AlgorithmArgument = "-a",
                        IntensityArgument = "-i",
                        ValidShareRegex = @"accepted: \d+.*? (yes|yay)!",
                        InvalidShareRegex = @"accepted: \d+.*? [bn]o+"
                    },
                    new Miner
                    {
                        Name = CcminerTpruvot,
                        FileName = @"/home/yamamoto/Mine/ccminer-tpruvot-new/ccminer",
                        ServerArgument = "-o",
                        UserArgument = "-u",
                        PasswordArgument = "-p",
                        SpeedRegex = @"(?<=accepted:).*?,\s(?<speed>\d+(\.\d+)?\s[kMGT]H/s)",
                        AlgorithmArgument = "-a",
                        IntensityArgument = "-i",
                        ValidShareRegex = @"accepted: \d+.*? (yes|yay)!",
                        InvalidShareRegex = @"accepted: \d+.*? [bn]o+"
                    },
                    new Miner
                    {
                        Name = CcminerAlexis,
                        FileName = @"/home/yamamoto/Mine/ccminer-alexis/ccminer",
                        ServerArgument = "-o",
                        UserArgument = "-u",
                        PasswordArgument = "-p",
                        SpeedRegex = @"\[S/A/T\]:([^,]+?,){2}\s*(?<speed>\d+(\.\d+)?[kMGH]H)",
                        AlgorithmArgument = "-a",
                        IntensityArgument = "-i",
                        ValidShareRegex = @"\[S\/A\/T\]:.*? (yes|yay)!",
                        InvalidShareRegex = @"\[S\/A\/T\]:.*? [bn]o+"
                    },
                    new Miner
                    {
                        Name = CcminerM7,
                        FileName = @"/home/yamamoto/Mine/ccminer-m7-branch/ccminer",
                        ServerArgument = "-o",
                        UserArgument = "-u",
                        PasswordArgument = "-p",
                        AdditionalArguments = "-R 2",
                        SpeedRegex = @"(?<=accepted:).*?,\s(?<speed>\d+(\.\d+)?\s[kMGT]H)",
                        AlgorithmArgument = "-a",
                        IntensityArgument = "-i",
                        ValidShareRegex = @"accepted: \d+.*?(YES|YAY)",
                        InvalidShareRegex = @"accepted: \d+.*?NO"
                    },
                    new Miner
                    {
                        Name = EwbfEquihash,
                        FileName = @"/home/yamamoto/Mine/Zcash/GPU/miner",
                        ServerArgument = "--server",
                        PortArgument = "--port",
                        UserArgument = "--user",
                        PasswordArgument = "--pass",
                        AdditionalArguments = "--log 2 --solver 0",
                        LogFileArgument = "--logfile",
                        ReadOutputFromLog = true,
                        SpeedRegex = @"^Total speed: (?<speed>\d+) Sol/s",
                        ValidShareRegex = @"GPU\d+ Accepted share \d+ms",
                        InvalidShareRegex = @"GPU\d+ Rejected share \d+ms",
                        OmitUrlSchema = true
                    }
                });
            context.SaveChanges();
        }
    }
}
