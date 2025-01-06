using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolutionSelling.Data;
using SolutionSelling.Models;
using SolutionSelling.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace SolutionSelling.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Fetch the total amount spent by each user who bought from the current user
            var buyerReports = await _context.CartReport
                .Where(cr => cr.Seller == currentUserEmail)
                .GroupBy(cr => cr.Buyer)
                .Select(g => new
                {
                    Buyer = g.Key,
                    TotalSpent = g.Sum(cr => cr.TotalAmount)
                })
                .ToListAsync();

            ViewBag.BuyerReports = buyerReports;

            return View();
        }

        [Authorize(Roles="Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
