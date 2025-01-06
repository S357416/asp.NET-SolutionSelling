using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolutionSelling.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SolutionSelling.Models
{
    public static class SeedData
    {
        // Seed Admin data first (User and Roles)
        public static async Task Initialize(IServiceProvider serviceProvider, string adminEmail, string adminPassword)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure the "Admin" role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Check if the admin user already exists
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                // Create the admin user
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create the admin user.");
                }
            }

            // Assign the user to the Admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Once the admin user is created and assigned a role, proceed with item seeding
            InitializeItems(serviceProvider, adminEmail);
        }

        // Seed item data, ensuring the Seller is the adminEmail (already created above)
        public static void InitializeItems(IServiceProvider serviceProvider, string adminEmail)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ApplicationDbContext>>()))
            {
                // Check if any items exist.
                if (context.Items.Any())
                {
                    return;   // DB has been seeded
                }

                // Add seed items with the admin user as Seller
                context.Items.AddRange(
                    new Items
                    {
                        Seller = adminEmail,
                        Item_Name = "Soccer Ball",
                        Category = "Sport",
                        Condition = Condition.UsedLikeNew,
                        Description = "Well conditioned Soccer Ball, no longer used by ur child",
                        Price = 12.50M,
                        Quantity = 3
                    },
                    new Items
                    {
                        Seller = adminEmail,
                        Item_Name = "Wireless Headphones",
                        Category = "Electronics",
                        Condition = Condition.New,
                        Description = "Noise-cancelling wireless headphones.",
                        Price = 299.99M,
                        Quantity = 10
                    },
                    new Items
                    {
                        Seller = adminEmail,
                        Item_Name = "Mountain Bike",
                        Category = "Sports & Outdoors",
                        Condition = Condition.UsedGood,
                        Description = "A used mountain bike in fair condition. Great for trails.",
                        Price = 150.00M,
                        Quantity = 2
                    },
                    new Items
                    {
                        Seller = adminEmail,
                        Item_Name = "Antique Wooden Chair",
                        Category = "Furniture",
                        Condition = Condition.UsedFair,
                        Description = "A solid antique wooden chair from the 19th century.",
                        Price = 120.00M,
                        Quantity = 1
                    },
                    new Items
                    {
                        Seller = adminEmail,
                        Item_Name = "Gaming Laptop",
                        Category = "Electronics",
                        Condition = Condition.New,
                        Description = "High-performance gaming laptop with RTX graphics card.",
                        Price = 1299.99M,
                        Quantity = 5
                    }
                );

                // Save changes to the database
                context.SaveChanges();
            }
        }
    }
}
