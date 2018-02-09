using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Models.Shared;

namespace Msv.AutoMiner.FrontEnd.ViewComponents
{
    public class HideZeroButtonViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(string sessionKey) 
            => Task.FromResult<IViewComponentResult>(View(new HideZeroButtonModel
            {
                CurrentState = HttpContext.Session.GetBool(sessionKey).GetValueOrDefault(true),
                ActionUri = Url.Action("ToggleShowZero", null)
            }));
    }
}
