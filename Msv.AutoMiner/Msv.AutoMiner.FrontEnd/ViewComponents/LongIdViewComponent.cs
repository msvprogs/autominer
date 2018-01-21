using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.FrontEnd.Models.Shared;

namespace Msv.AutoMiner.FrontEnd.ViewComponents
{
    public class LongIdViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(LongIdModel model)
            => Task.FromResult<IViewComponentResult>(View(model));
    }
}
