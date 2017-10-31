using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.FrontEnd.Models.Shared;

namespace Msv.AutoMiner.FrontEnd.ViewComponents
{
    public class PaginationViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(PaginationModel paginationModel)
            => Task.FromResult<IViewComponentResult>(View(paginationModel));
    }
}
