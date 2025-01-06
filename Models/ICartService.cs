namespace SolutionSelling.Models
{
    public interface ICartService
    {
        void AddItem(Items item, int quantity);
        int RemoveItem(int itemId, int qty);
        int TotalItem(int itemId);
        int CartCount();
        Cart GetCart();
    }
}
