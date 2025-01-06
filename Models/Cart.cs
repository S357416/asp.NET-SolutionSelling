using System.ComponentModel.DataAnnotations;

namespace SolutionSelling.Models
{
    public class Cart
    {
        [Key]
        public string Username { get; set; }
        public Cart(string username)
        {
            CartItems = new List<CartItem>();
            Username = username;
        }
        public virtual IList<CartItem> CartItems { get; set; }
    }
}
