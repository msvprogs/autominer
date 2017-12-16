using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.FrontEnd.Models.Shared;

namespace Msv.AutoMiner.FrontEnd.ViewComponents
{
    public class LogoCoinNameViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(LogoCoinNameModel model)
            => Task.FromResult<IViewComponentResult>(View(model));
    }
}