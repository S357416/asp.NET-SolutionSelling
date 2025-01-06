using System;
using System.Collections.Generic;

namespace SolutionSelling.Models
{
    public class CartReport
    {
        public int Id { get; set; }
        public string? Buyer { get; set; }
        public string? Seller { get; set; }
        public string? Item { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
