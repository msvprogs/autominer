using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.FrontEnd.Models;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() 
            => View();

        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
