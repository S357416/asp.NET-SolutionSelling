
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolutionSelling.Models;

public enum Condition
{
    [Display(Name = "New")]
    New,
    [Display(Name = "Used - Like New")]
    UsedLikeNew,
    [Display(Name = "Used - Good")]
    UsedGood,
    [Display(Name = "Used - Fair")]
    UsedFair
}
public class Items
{
    public int Id { get; set; }
    public string? Seller { get; set; }
    [Display(Name = "Item Name")]
    [Required]
    public string? Item_Name { get; set; }
    [Required]
    public string? Category { get; set; }
    public Condition Condition { get; set; }
    public string? Description { get; set; }
    [DataType(DataType.Currency)]
    [Required]
    public decimal Price { get; set; }
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers or zero is allowed")]
    public int Quantity { get; set; }
}