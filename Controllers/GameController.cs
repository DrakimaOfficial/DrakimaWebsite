using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrakimaWebsite.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}