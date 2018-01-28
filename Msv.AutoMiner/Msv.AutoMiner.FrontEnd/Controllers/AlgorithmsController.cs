using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;
using Msv.AutoMiner.FrontEnd.Models.Miners;
using Newtonsoft.Json;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class AlgorithmsController : EntityControllerBase<CoinAlgorithm, AlgorithmDisplayModel, Guid>
    {
        public const string AlgorithmsMessageKey = "AlgorithmsMessage";

        private readonly AutoMinerDbContext m_Context;

        public AlgorithmsController(AutoMinerDbContext context)
            : base("_AlgorithmRowPartial", context) 
            => m_Context = context;

        public IActionResult Index()
            => View(GetEntityModels(null));

        public async Task<IActionResult> Create()
            => View("Edit", new AlgorithmEditModel
            {
                Id = Guid.NewGuid(),
                IsNewEntity = true,
                AvailableMiners = await GetAvailableMiners()
            });

        public async Task<IActionResult> Edit(Guid id)
        {
            var algorithm = await m_Context.CoinAlgorithms
                .FirstOrDefaultAsync(x => x.Id == id);
            if (algorithm == null)
                return NotFound();
            return View(new AlgorithmEditModel
            {
                Id = algorithm.Id,
                KnownValue = algorithm.KnownValue,
                Name = algorithm.Name,
                AdditionalArguments = algorithm.AdditionalArguments,
                MinerId = algorithm.MinerId,
                MinerAlgorithmArgument = algorithm.AlgorithmArgument,
                Intensity = algorithm.Intensity,
                AvailableMiners = await GetAvailableMiners()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Save(AlgorithmEditModel algorithmModel)
        {
            var algorithm = await m_Context.CoinAlgorithms
                    .FirstOrDefaultAsync(x => x.Id == algorithmModel.Id);
            if (algorithmModel.IsNewEntity && algorithm != null)
                ModelState.AddModelError(nameof(algorithmModel.Id), "Algorithm with this ID already exists");
            if (!ModelState.IsValid)
            {
                algorithmModel.AvailableMiners = await GetAvailableMiners();
                return View("Edit", algorithmModel);
            }
            
            if (algorithm == null)
                algorithm = (await m_Context.CoinAlgorithms.AddAsync(new CoinAlgorithm
                {
                    Id = algorithmModel.Id
                })).Entity;
            algorithm.Activity = ActivityState.Active;
            algorithm.KnownValue = algorithmModel.KnownValue;
            algorithm.Name = algorithmModel.Name;
            algorithm.AdditionalArguments = algorithmModel.AdditionalArguments;
            algorithm.MinerId = algorithmModel.MinerId;
            algorithm.AlgorithmArgument = algorithmModel.MinerAlgorithmArgument;
            algorithm.Intensity = algorithmModel.Intensity;

            await m_Context.SaveChangesAsync();
            TempData[AlgorithmsMessageKey] = $"Algorithm {algorithmModel.Name} has been successfully saved";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Export(Guid id)
        {
            var algorithm = await m_Context.CoinAlgorithms.FirstAsync(x => x.Id == id);
            var exportedContent = JsonConvert.SerializeObject(
                new AlgorithmBaseModel
                {
                    Name = algorithm.Name,
                    Id = algorithm.Id,
                    KnownValue = algorithm.KnownValue
                });
            return ReturnAsJsonFile($"{algorithm.Name}_algo_settings.json", exportedContent);
        }

        public override Task<IActionResult> ToggleActive(Guid id)
            => Task.FromResult<IActionResult>(NotFound());

        protected override bool IsEditable(CoinAlgorithm entity)
            => entity.KnownValue != KnownCoinAlgorithm.Sha256D;

        protected override AlgorithmDisplayModel[] GetEntityModels(Guid[] ids)
        {
            var algorithmQuery = m_Context.CoinAlgorithms
                    .Include(x => x.Miner)
                    .AsNoTracking()
                    .Where(x => x.Activity != ActivityState.Deleted);
            if (!ids.IsNullOrEmpty())
                algorithmQuery = algorithmQuery.Where(x => ids.Contains(x.Id));
            return algorithmQuery
                .Select(x => new AlgorithmDisplayModel
                {
                    Id = x.Id,
                    KnownValue = x.KnownValue,
                    Name = x.Name,
                    Activity = x.Activity,
                    MinerAlgorithmArgument = x.AlgorithmArgument,
                    MinerName = x.Miner.Name
                })
                .ToArray();
        }

        private Task<MinerBaseModel[]> GetAvailableMiners()
            => m_Context.Miners
                .Where(x => x.Activity == ActivityState.Active)
                .Select(x => new MinerBaseModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToArrayAsync();
    }
}
