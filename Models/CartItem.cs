namespace SolutionSelling.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public virtual Items Items { get; set; }
        public int Quantity { get; set; }
        public virtual Cart Cart { get; set; }
    }
}
