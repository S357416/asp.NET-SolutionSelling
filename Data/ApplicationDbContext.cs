using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SolutionSelling.Models;

namespace SolutionSelling.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<SolutionSelling.Models.Items> Items { get; set; } = default!;
        public DbSet<SolutionSelling.Models.Cart> Cart { get; set; } = default!;
        public DbSet<SolutionSelling.Models.CartItem> CartItem { get; set; } = default!;
        public DbSet<SolutionSelling.Models.CartReport> CartReport { get; set; }
        }
    }