using Microsoft.AspNetCore.Mvc;

namespace ConcertTicketPlatform.Web.Controllers
{
    public class ConcertsController : Controller
    {
        private readonly HttpClient _httpClient;

        public ConcertsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> Index()
        {
            var concerts = await _httpClient.GetFromJsonAsync<List<dynamic>>("api/concerts");
            return View(concerts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var concert = await _httpClient.GetFromJsonAsync<dynamic>($"api/concerts/{id}");
            return View(concert);
        }
    }
}