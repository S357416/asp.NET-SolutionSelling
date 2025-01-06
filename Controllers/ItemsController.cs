using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SolutionSelling.Data;
using SolutionSelling.Models;
using SolutionSelling.Services;

namespace SolutionSelling.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ItemsController(ApplicationDbContext context, CartService cartService, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _cartService = cartService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private async Task<bool> IsUserAdminAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return false;
            }
            var roles = await _userManager.GetRolesAsync(user);
            return roles.Contains("Admin");
        }

        private async Task<bool> IsUserAuthorizedAsync(int itemId)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
                return false;

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (await IsUserAdminAsync())
                return true;

            return item.Seller == userEmail;
        }

        // GET: Items/Index
        public async Task<IActionResult> Index()
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Items' is null.");
            }
            else
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var items = _context.Items.Where(i => i.Seller == userEmail && i.Quantity > 0);
                return View(await items.ToListAsync());
            }
        }

        public async Task<IActionResult> Allitems(string itemCategory, string searchString)
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Items' is null.");
            }

            // Use LINQ to get list of genres.
            IQueryable<string> genreQuery = from m in _context.Items
                                            orderby m.Category
                                            select m.Category;
            var item = from m in _context.Items
                       where m.Quantity > 0 // Exclude items with zero quantity
                       select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                item = item.Where(s => s.Item_Name!.ToUpper().Contains(searchString.ToUpper()));
            }

            if (!string.IsNullOrEmpty(itemCategory))
            {
                item = item.Where(x => x.Category == itemCategory);
            }

            var itemCategoryVM = new CategoryViewModel
            {
                Category = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Item = await item.ToListAsync()
            };

            return View(itemCategoryVM);
        }

        // GET: Items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Items/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Items/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Item_Name,Category,Condition,Description,Price,Quantity")] Items item)
        {
            if (ModelState.IsValid)
            {
                item.Seller = User.FindFirstValue(ClaimTypes.Email);
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        // GET: Items/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return Forbid();
            }
            return View(item);
        }

        // POST: Items/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Item_Name,Category,Condition,Description,Price,Quantity")] Items item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the existing item from the database
                    var existingItem = await _context.Items.FindAsync(id);

                    if (existingItem == null)
                    {
                        return NotFound();
                    }

                    // Manually update the properties of the existing item
                    existingItem.Item_Name = item.Item_Name;
                    existingItem.Category = item.Category;
                    existingItem.Condition = item.Condition;
                    existingItem.Description = item.Description;
                    existingItem.Price = item.Price;
                    existingItem.Quantity = item.Quantity;

                    // Save the changes
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemsExists(item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }


        // GET: Items/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null || !(await IsUserAuthorizedAsync(id.Value)))
            {
                return Forbid();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await IsUserAuthorizedAsync(id))
            {
                return Forbid();
            }

            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ItemsExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }

        // GET: Items/Purchase/5
        public async Task<IActionResult> Purchase(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            if (item.Quantity > 0)
            {
                _cartService.AddItem(item, 1);
                item.Quantity--;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Cart");
            }

            return RedirectToAction("allitems");
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}
