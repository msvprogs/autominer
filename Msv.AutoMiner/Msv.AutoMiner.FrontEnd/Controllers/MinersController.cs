using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Models.Miners;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class MinersController : EntityControllerBase<Miner, MinerDisplayModel, int>
    {
        public const string MinersMessageKey = "MinersMessage";

        private readonly IUploadedFileStorage m_UploadedFileStorage;
        private readonly AutoMinerDbContext m_Context;

        public MinersController(IUploadedFileStorage uploadedFileStorage, AutoMinerDbContext context) 
            : base("_MinerRowPartial", context)
        {
            m_UploadedFileStorage = uploadedFileStorage;
            m_Context = context;
        }

        public IActionResult Index()
            => View(GetEntityModels(null));

        public IActionResult Create()
            => View("Edit", new MinerBaseModel());

        public async Task<IActionResult> Edit(int id)
        {
            var miner = await m_Context.Miners
                .FirstOrDefaultAsync(x => x.Id == id);
            if (miner == null)
                return NotFound();
            return View(new MinerBaseModel
            {
                Id = miner.Id,
                Name = miner.Name
            });
        }

        [HttpPost]
        public async Task<IActionResult> Save(MinerBaseModel minerModel)
        {
            var miner = await m_Context.Miners
                .FirstOrDefaultAsync(x => x.Id == minerModel.Id)
                ?? (await m_Context.Miners.AddAsync(new Miner
                        {
                            Activity = ActivityState.Active
                        })).Entity;
            if (!ModelState.IsValid)
                return View("Edit", minerModel);
            
            miner.Name = minerModel.Name;

            await m_Context.SaveChangesAsync();
            TempData[MinersMessageKey] = $"Miner {miner.Name} has been successfully saved";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditLastVersion(int minerId, PlatformType platform)
        {
            var version = await m_Context.MinerVersions
                .Include(x => x.Miner)
                .OrderByDescending(x => x.Uploaded)
                .FirstOrDefaultAsync(x => x.MinerId == minerId && x.Platform == platform);
            if (version == null)
                return NotFound();
            return View("EditVersion", VersionToModel(version));
        }

        public async Task<IActionResult> UploadNewVersion(int minerId, PlatformType platform)
        {
            var version = await m_Context.MinerVersions
                              .Include(x => x.Miner)
                              .OrderByDescending(x => x.Uploaded)
                              .FirstOrDefaultAsync(x => x.MinerId == minerId && x.Platform == platform)
                          ?? new MinerVersion
                          {
                              Miner = await m_Context.Miners.FirstAsync(x => x.Id == minerId),
                              Platform = platform
                          };
            var versionModel = VersionToModel(version);
            versionModel.Id = 0;
            versionModel.Version = null;
            return View("EditVersion", versionModel);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> SaveVersion(MinerVersionModel versionModel)
        {
            if (versionModel.MinerApiType != MinerApiType.Stdout && versionModel.MinerApiPort == null)
                ModelState.AddModelError(nameof(versionModel.MinerApiPort), "API port isn't specified");

            if (!ModelState.IsValid)
                return View("EditVersion", versionModel);

            var version = await m_Context.MinerVersions
                .Include(x => x.Miner)
                .OrderByDescending(x => x.Uploaded)
                .FirstOrDefaultAsync(x => x.Id == versionModel.Id);
            var isNew = version == null;
            if (isNew)
                version = (await m_Context.MinerVersions.AddAsync(new MinerVersion
                {
                    MinerId = versionModel.MinerId,
                    Uploaded = DateTime.UtcNow,
                    Platform = versionModel.Platform
                })).Entity;

            if (versionModel.ZipFile != null)
                using (var fileStream = versionModel.ZipFile.OpenReadStream())
                    try
                    {
                        using (var zipFile = new ZipArchive(fileStream, ZipArchiveMode.Read))
                            if (zipFile.GetEntry(versionModel.ExeFilePath) == null
                                && (string.IsNullOrEmpty(versionModel.ExeSecondaryFilePath)
                                    || zipFile.GetEntry(versionModel.ExeSecondaryFilePath) == null))
                            {
                                ModelState.AddModelError(nameof(versionModel.ZipFile),
                                    $"The file {versionModel.ExeSecondaryFilePath ?? versionModel.ExeFilePath} wasn't found in the root directory of the archive");
                                return View("EditVersion", versionModel);
                            }
                    }
                    catch (InvalidDataException)
                    {
                        ModelState.AddModelError(nameof(versionModel.ZipFile), "The file you've uploaded wasn't recognized as ZIP archive");
                        return View("EditVersion", versionModel);
                    }

            version.PasswordArgument = versionModel.PasswordArgument;
            version.PortArgument = versionModel.PortArgument;
            version.ServerArgument = versionModel.ServerArgument;
            version.SpeedRegex = versionModel.SpeedRegex;
            version.UserArgument = versionModel.UserArgument;
            version.ValidShareRegex = versionModel.ValidShareRegex;
            version.AdditionalArguments = versionModel.AdditionalArguments;
            version.AlgorithmArgument = versionModel.AlgorithmArgument;
            version.BenchmarkArgument = versionModel.BenchmarkArgument;
            version.BenchmarkResultRegex = versionModel.BenchmarkResultRegex;
            version.ExeFilePath = versionModel.ExeFilePath;
            version.ExeSecondaryFilePath = versionModel.ExeSecondaryFilePath;
            version.IntensityArgument = versionModel.IntensityArgument;
            version.InvalidShareRegex = versionModel.InvalidShareRegex;
            version.ApiType = versionModel.MinerApiType;
            version.ApiPort = versionModel.MinerApiPort;
            version.ApiPortArgument = versionModel.ApiPortArgument;
            version.OmitUrlSchema = versionModel.OmitUrlSchema;

            await m_Context.SaveChangesAsync();

            var miner = await m_Context.Miners
                .FirstAsync(x => x.Id == versionModel.MinerId);
            if (isNew)
            {
                version.Version = versionModel.Version;
                if (versionModel.ZipFile != null)
                {
                    version.ZipPath = CreateMinerArchiveName(
                        version.Id, versionModel.Platform, miner.Name + version.Version);
                    using (var fileStream = versionModel.ZipFile.OpenReadStream())
                        await m_UploadedFileStorage.SaveAsync(version.ZipPath, fileStream);
                }
                await m_Context.SaveChangesAsync();
            }

            TempData[MinersMessageKey] =
                $"Version {versionModel.Version} of the miner {miner.Name} has been successfully saved";
            return RedirectToAction("Index");
        }

        protected override MinerDisplayModel[] GetEntityModels(int[] ids)
        {
            var minerQuery = m_Context.Miners
                    .AsNoTracking()
                    .Where(x => x.Activity != ActivityState.Deleted);
            if (!ids.IsNullOrEmpty())
                minerQuery = minerQuery.Where(x => ids.Contains(x.Id));

            var minerVersions = m_Context.MinerVersions
                .AsNoTracking()
                .FromSql(@"SELECT ver.* FROM MinerVersions ver
  join (select MinerId, Platform, MAX(Uploaded) as MaxUploaded
    from MinerVersions
    group by MinerId, Platform) maxUploaded
  on ver.MinerId = maxUploaded.MinerId and ver.Platform = maxUploaded.Platform and ver.Uploaded = maxUploaded.MaxUploaded")
                .ToArray();

            return minerQuery
                .AsEnumerable()
                .GroupJoin(minerVersions, x => x.Id, x => x.MinerId, (x, y) => new
                {
                    Miner = x,
                    // ReSharper disable PossibleMultipleEnumeration
                    WindowsVersion = y.FirstOrDefault(z => z.Platform == PlatformType.Windows),
                    LinuxVersion = y.FirstOrDefault(z => z.Platform == PlatformType.Linux)
                    // ReSharper restore PossibleMultipleEnumeration
                })
                .Select(x => new MinerDisplayModel
                {
                    Id = x.Miner.Id,
                    Name = x.Miner.Name,
                    Activity = x.Miner.Activity,
                    LastWindowsUpdated = x.WindowsVersion?.Uploaded,
                    CurrentWindowsVersion = x.WindowsVersion?.Version,
                    LastLinuxUpdated = x.LinuxVersion?.Uploaded,
                    CurrentLinuxVersion = x.LinuxVersion?.Version
                })
                .ToArray();
        }

        private static string CreateMinerArchiveName(int minerVersionId, PlatformType platform, string name) 
            => $"Miner_{minerVersionId}_{platform}_{name}.zip".ToSafeFileName();

        private static MinerVersionModel VersionToModel(MinerVersion version)
            => new MinerVersionModel
            {
                Id = version.Id,
                MinerId = version.MinerId,
                Platform = version.Platform,
                AlgorithmArgument = version.AlgorithmArgument,
                AdditionalArguments = version.AdditionalArguments,
                BenchmarkArgument = version.BenchmarkArgument,
                BenchmarkResultRegex = version.BenchmarkResultRegex,
                ExeFilePath = version.ExeFilePath,
                ExeSecondaryFilePath = version.ExeSecondaryFilePath,
                IntensityArgument = version.IntensityArgument,
                InvalidShareRegex = version.InvalidShareRegex,
                Version = version.Version,
                MinerName = version.Miner.Name,
                OmitUrlSchema = version.OmitUrlSchema,
                PasswordArgument = version.PasswordArgument,
                PortArgument = version.PortArgument,
                ServerArgument = version.ServerArgument,
                SpeedRegex = version.SpeedRegex,
                UserArgument = version.UserArgument,
                ValidShareRegex = version.ValidShareRegex,
                MinerApiType = version.ApiType,
                MinerApiPort = version.ApiPort,
                ApiPortArgument = version.ApiPortArgument
            };
    }
}
