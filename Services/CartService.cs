using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SolutionSelling.Data;
using SolutionSelling.Models;
using System.Security.Claims;

namespace SolutionSelling.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public Cart GetCart()
        {
            var username = _httpContextAccessor.HttpContext.User.Identity.Name;
            if (username == null) { return null; }
            var cart = _context.Cart.FirstOrDefault(c => c.Username == username);
            if (cart == null)
            {
                cart = new Cart(username);
                _context.Cart.Add(cart);
                _context.SaveChanges();
            }
            cart.CartItems = _context.CartItem.Where(c => c.Cart == cart).Include("Items").ToList();
            return cart;
        }



        public void AddItem(Items product, int quantity)
        {
            Cart cart = GetCart();
            var item = cart.CartItems.FirstOrDefault(ci => ci.Items.Id == product.Id);

            if (item == null)
            {
                item = new CartItem { Items = product, Cart = cart, Quantity = quantity };
                cart.CartItems.Add(item);
            }
            else
            {
                item.Quantity += quantity;
            }
            _context.SaveChanges();
        }

        public int CartCount()
        {
            var cart = GetCart();
            return cart.CartItems.Sum(i => i.Quantity);
        }

        public int RemoveItem(int itemId, int qty = 1)
        {
            Cart cart = GetCart();
            var item = cart.CartItems.FirstOrDefault(ci => ci.Items.Id == itemId);
            if (item == null) return 0;
            item.Quantity -= qty;
            if (item.Quantity == 0)
            {
                cart.CartItems.Remove(item);
            }
            _context.SaveChanges();
            return item.Quantity;
        }

        public int TotalItem(int itemId)
        {
            Cart cart = GetCart();
            var item = cart.CartItems.FirstOrDefault(ci => ci.Items.Id == itemId);
            return item.Quantity;
        }

        public void ClearCart(string username)
        {
            var cart = _context.Cart.FirstOrDefault(c => c.Username == username);
            if (cart != null)
            {
                _context.CartItem.RemoveRange(cart.CartItems);
                _context.Cart.Remove(cart);
                _context.SaveChanges();
            }
        }

    }
}
