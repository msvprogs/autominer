using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.FrontEnd.Models.Shared;

namespace Msv.AutoMiner.FrontEnd.ViewComponents
{
    public class DatePeriodViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(DatePeriodModel model)
        {
            return Task.FromResult<IViewComponentResult>(View(model));
        }
    }
}
