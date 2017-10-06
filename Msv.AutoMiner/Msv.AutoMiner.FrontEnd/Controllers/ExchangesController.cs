using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Models.Exchanges;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class ExchangesController : Controller
    {
        private readonly AutoMinerDbContext m_Context;

        public ExchangesController(AutoMinerDbContext context)
            => m_Context = context;

        public async Task<IActionResult> Index()
        {
            var exchanges = await m_Context.Exchanges
                .Where(x => x.Activity != ActivityState.Deleted)
                .Select(x => new ExchangeModel
                {
                    Type = x.Type,
                    Activity = x.Activity,
                    HasKeys = x.PrivateKey != null && x.PublicKey != null
                })
                .ToArrayAsync();
            return View(exchanges);
        }

        [HttpGet]
        public IActionResult SetKeys(ExchangeType exchangeType)
            => PartialView("_EditKeysPartial", new EditKeysModel {Exchange = exchangeType});

        [HttpPost]
        public async Task<IActionResult> SetKeys(EditKeysModel model)
        {
            var exchange = await m_Context.Exchanges
                .FirstOrDefaultAsync(x => x.Type == model.Exchange);
            if (exchange == null)
                return NotFound();
            exchange.PublicKey = model.PublicKey.Trim();
            exchange.PrivateKey = model.PrivateKey.Trim();
            await m_Context.SaveChangesAsync();
            return Ok();
        }
    }
}
