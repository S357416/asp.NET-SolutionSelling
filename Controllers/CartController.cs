using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolutionSelling.Data;
using SolutionSelling.Models;
using SolutionSelling.Services;
using System.Security.Claims;

namespace SolutionSelling.Controllers
{
    public class CartController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public CartController(ApplicationDbContext context, CartService cartService, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _cartService = cartService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Cart/Index/
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            ViewBag.totalcost = cart.CartItems.Sum(ci => ci.Items.Price * ci.Quantity);
            return View(cart.CartItems);
        }

        // GET: Cart/Add/5
        public async Task<IActionResult> Add(int id)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            if (item.Quantity > 0)
            {
                item.Quantity--;
                await _context.SaveChangesAsync();
                _cartService.AddItem(item, 1);
            }

            return RedirectToAction("Index");
        }

        // GET: Cart/Minus/5
        public async Task<IActionResult> Minus(int id)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            int whatsLeft = _cartService.RemoveItem(id);

            item.Quantity++;
            await _context.SaveChangesAsync();

            if (whatsLeft > 0) return RedirectToAction("Index");
            else return RedirectToAction("allitems", "Items");
        }

        // GET: Cart/Minus/5
        public async Task<IActionResult> Remove(int id)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            int total = _cartService.TotalItem(id);

            _cartService.RemoveItem(id, total);

            item.Quantity += total;
            await _context.SaveChangesAsync();

            if (_cartService.CartCount() > 0) return RedirectToAction("Index");
            else return RedirectToAction("allitems", "Items");
        }

        // GET: Cart/Checkout
        public ActionResult Checkout(int id)
        {
            var username = User.FindFirstValue(ClaimTypes.Email);
            if (username == null)
            {
                return RedirectToAction("AllItems", "Items");
            }

            var cart = _context.Cart
                               .Include(c => c.CartItems)
                               .ThenInclude(ci => ci.Items)
                               .FirstOrDefault(c => c.Username == username);

            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("AllItems", "Items");
            }

            foreach (var item in cart.CartItems)
            {
                if (item.Items == null)
                {
                    continue; // Skip this item if Items is null
                }

                var reportItem = new CartReport
                {
                    Buyer = username,
                    Seller = item.Items.Seller,
                    Item = item.Items.Item_Name,
                    Quantity = item.Quantity,
                    TotalAmount = item.Quantity * item.Items.Price,
                };

                _context.CartReport.Add(reportItem);
            }

            _context.SaveChanges();

            _cartService.ClearCart(username);

            return View("Checkout");
        }

        public async Task<IActionResult> Report()
        {
            var username = User.FindFirstValue(ClaimTypes.Email);
            if (username == null)
            {
                return RedirectToAction("AllItems", "Items");
            }

            var cartReports = await _context.CartReport
                .Where(cr => cr.Seller == username)
                .ToListAsync();

            return View(cartReports);
        }

    }
}
