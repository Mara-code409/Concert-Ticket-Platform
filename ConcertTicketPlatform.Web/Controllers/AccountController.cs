using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ConcertTicketPlatform.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser>   _userManager;
        private readonly SignInManager<AppUser>  _signInManager;
        private readonly AppDbContext            _db;
        private readonly IHttpClientFactory      _httpClientFactory;

        public AccountController(
            UserManager<AppUser>   userManager,
            SignInManager<AppUser>  signInManager,
            AppDbContext            db,
            IHttpClientFactory      httpClientFactory)
        {
            _userManager       = userManager;
            _signInManager     = signInManager;
            _db                = db;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email și parola sunt obligatorii.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Email sau parolă incorectă.");
                return View();
            }

            try
            {
                var client   = _httpClientFactory.CreateClient("API");
                var response = await client.PostAsJsonAsync("api/auth/login", new { email, password });
                if (response.IsSuccessStatusCode)
                {
                    var json  = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var token = json.GetProperty("token").GetString();
                    if (!string.IsNullOrEmpty(token))
                        HttpContext.Session.SetString("JwtToken", token);
                }
            }
            catch { }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string password, string fullName)
        {
            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError("", "Emailul este obligatoriu.");
            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("", "Parola este obligatorie.");
            if (string.IsNullOrWhiteSpace(fullName))
                ModelState.AddModelError("", "Numele complet este obligatoriu.");

            if (!ModelState.IsValid) return View();

            var user   = new AppUser { UserName = email, Email = email, FullName = fullName };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);

                try
                {
                    var client   = _httpClientFactory.CreateClient("API");
                    var response = await client.PostAsJsonAsync("api/auth/login", new { email, password });
                    if (response.IsSuccessStatusCode)
                    {
                        var json  = await response.Content.ReadFromJsonAsync<JsonElement>();
                        var token = json.GetProperty("token").GetString();
                        if (!string.IsNullOrEmpty(token))
                            HttpContext.Session.SetString("JwtToken", token);
                    }
                }
                catch { }

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JwtToken");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");
            return View(user);
        }

        [Authorize]
        public async Task<IActionResult> MyTickets()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var tickets = await _db.Tickets
                .Include(t => t.Concert)
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.Concert.Date)
                .ToListAsync();

            var reviewedConcertIds = (await _db.Reviews
                .Where(r => r.UserId == user.Id)
                .Select(r => r.ConcertId)
                .ToListAsync())
                .ToHashSet();

            ViewBag.ReviewedConcertIds = reviewedConcertIds;
            return View(tickets);
        }
    }
}
