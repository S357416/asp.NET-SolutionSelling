using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SolutionSelling.Models;

public class CategoryViewModel
{
    public List<Items>? Item { get; set; }
    public SelectList? Category { get; set; }
    public string? ItemCategory { get; set; }
    public string? SearchString { get; set; }
}