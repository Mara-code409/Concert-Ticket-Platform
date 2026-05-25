using Microsoft.AspNetCore.Mvc;

namespace ConcertTicketPlatform.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}