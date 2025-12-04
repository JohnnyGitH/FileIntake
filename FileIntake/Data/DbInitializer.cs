using System;
using System.Linq;
using System.Threading.Tasks;
using FileIntake.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FileIntake.Data;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        // Ensure the database is created
        context.Database.EnsureCreated();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await SeedUserAsync(userManager, context,
            email: "johnny@example.com",
            password: "Password123!",
            firstName: "Johnny",
            lastName: "Cockrem");

        await SeedUserAsync(userManager, context,
            email: "test@example.com",
            password: "Password123!",
            firstName: "test",
            lastName: "Man");

        if (!context.UserProfiles.Any(up => up.Email == "pam.beesly@dundermifflin.com"))
        {
            // Seed Office staff first
            SeedSampleUserProfiles(context);
            await context.SaveChangesAsync();

            // Only seed sample files *if enough sample users exist*
            var count = context.UserProfiles.Count();
            if (count >= 25) 
            {
                SeedSampleFiles(context);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedUserAsync(UserManager<IdentityUser> userManager, ApplicationDbContext context, string email, string password, string firstName, string lastName)
    {
        var existing = await userManager.FindByEmailAsync(email);

        if (existing == null)
        {
            var user = new IdentityUser 
            { 
                UserName = email, 
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user: " + email);
            }

            context.UserProfiles.Add(new UserProfile
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IdentityUserId = user.Id
            });

            Console.WriteLine($"Created seeded user: {email}");
        }
        else
        {
            // Ensure profile exists
            if (!context.UserProfiles.Any(p => p.Email == email))
            {
                context.UserProfiles.Add(new UserProfile
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    IdentityUserId = existing.Id
                });
                Console.WriteLine($"Created missing UserProfile for: {email}");
            }

            // Make sure email is confirmed for existing users
            if (!existing.EmailConfirmed)
            {
                existing.EmailConfirmed = true;
                await userManager.UpdateAsync(existing);
            }

            // Reset password each startup inside Docker
            await userManager.RemovePasswordAsync(existing);
            await userManager.AddPasswordAsync(existing, password);

            Console.WriteLine($"{email} password reset during seeding");
        }
    }

    private static void SeedSampleUserProfiles(ApplicationDbContext context)
    {
        var users = new UserProfile[]
        {
            new UserProfile { FirstName = "Pamela", LastName = "Beesly", Email = "pam.beesly@dundermifflin.com" },
            new UserProfile { FirstName = "Jim", LastName = "Halpert", Email = "jimothy.halpert@dundermifflin.com" },
            new UserProfile { FirstName = "Dwight", LastName = "Schrute", Email = "dwight.schrute@dundermifflin.com" },
            new UserProfile { FirstName = "Michael", LastName = "Scott", Email = "micheal.scott@dundermifflin.com" },
            new UserProfile { FirstName = "Angela", LastName = "Martin", Email = "angela.martin@dundermifflin.com" },
            new UserProfile { FirstName = "Kevin", LastName = "Malone", Email = "kevin.malone@dundermifflin.com" },
            new UserProfile { FirstName = "Oscar", LastName = "Martinez", Email = "oscar.martinez@dundermifflin.com" },
            new UserProfile { FirstName = "Toby", LastName = "Flenderson", Email = "toby.flenderson@dundermifflin.com" },
            new UserProfile { FirstName = "Stanley", LastName = "Hudson", Email = "stanley.hudson@dundermifflin.com" },
            new UserProfile { FirstName = "Phyllis", LastName = "Vance", Email = "phyllis.vance@dundermifflin.com" },
            new UserProfile { FirstName = "Ryan", LastName = "Howard", Email = "ryan.howard@dundermifflin.com" },
            new UserProfile { FirstName = "Kelly", LastName = "Kapoor", Email = "kelly.kapoor@dundermifflin.com" },
            new UserProfile { FirstName = "Creed", LastName = "Bratton", Email = "creed.bratton@dundermifflin.com" },
            new UserProfile { FirstName = "Meredith", LastName = "Palmer", Email = "meredith.palmer@dundermifflin.com" },
            new UserProfile { FirstName = "Andy", LastName = "Bernard", Email = "andrew.bernard@dundermifflin.com" },
            new UserProfile { FirstName = "Darryl", LastName = "Philbin", Email = "darryl.philbin@dundermifflin.com" },
            new UserProfile { FirstName = "Jan", LastName = "Levinson", Email = "jan.levinson@dundermifflin.com" },
            new UserProfile { FirstName = "Holly", LastName = "Flax", Email = "holly.flax@dundermifflin.com" },
            new UserProfile { FirstName = "David", LastName = "Wallace", Email = "david.wallace@dundermifflin.com" },
            new UserProfile { FirstName = "Robert", LastName = "California", Email = "robert.california@dundermifflin.com" },
            new UserProfile { FirstName = "Gabe", LastName = "Lewis", Email = "gabe.lewis@sabre.com" },
            new UserProfile { FirstName = "Nellie", LastName = "Bertram", Email = "nellie.bertram@dundermifflin.com" },
            new UserProfile { FirstName = "Clark", LastName = "Green", Email = "clark.green@dundermifflin.com" },
            new UserProfile { FirstName = "Pete", LastName = "Miller", Email = "pete.miller@dundermifflin.com" },
            new UserProfile { FirstName = "Erin", LastName = "Hannon", Email = "erin.hannon@dundermifflin.com" },
            new UserProfile { FirstName = "Roy", LastName = "Anderson", Email = "roy.anderson@dundermifflin.com" },
            new UserProfile { FirstName = "Karen", LastName = "Filippelli", Email = "karen.filippelli@dundermifflin.com" }
        };

        context.UserProfiles.AddRange(users);
    }

    private static void SeedSampleFiles(ApplicationDbContext context)
    {
        var users = context.UserProfiles.ToList();

        var files = new FileRecord[]
        {
            new FileRecord { FileName = "Quarterly_Report.pdf", FilePath="C:\\Uploads\\2025\\Reports\\Quarterly_Report.pdf", UserProfileId = users[3].Id, FileSize = 320, UploadedAt = DateTime.Now.AddDays(-10)},
            new FileRecord { FileName = "Employee_Handbook.pdf", FilePath="C:\\Uploads\\2024\\HR\\Employee_Handbook.pdf", UserProfileId = users[7].Id, FileSize = 150, UploadedAt = DateTime.Now.AddDays(-20) },
            new FileRecord { FileName = "Sales_Data.pdf", FilePath="C:\\Uploads\\2025\\Finance\\Sales_Data.pdf", UserProfileId = users[1].Id, FileSize = 450, UploadedAt = DateTime.Now.AddDays(-5)},
            new FileRecord { FileName = "Project_Plan.pdf", FilePath="C:\\Uploads\\2025\\Plans\\Project_Plan.pdf", UserProfileId = users[2].Id, FileSize = 275, UploadedAt = DateTime.Now.AddDays(-15)},
            new FileRecord { FileName = "Meeting_Notes.pdf", FilePath="C:\\Uploads\\2025\\Reports\\Meeting_Notes.pdf", UserProfileId = users[0].Id, FileSize = 100, UploadedAt = DateTime.Now.AddDays(-8)},
            new FileRecord { FileName = "Budget_Overview.pdf", FilePath="C:\\Uploads\\2025\\Finance\\Budget_Overview.pdf", UserProfileId = users[6].Id, FileSize = 380, UploadedAt = DateTime.Now.AddDays(-12)},
            new FileRecord { FileName = "Marketing_Strategy.pdf", FilePath="C:\\Uploads\\2025\\Marketing\\Marketing_Strategy.pdf", UserProfileId = users[6].Id, FileSize = 290, UploadedAt = DateTime.Now.AddDays(-7)},
            new FileRecord { FileName = "Client_List.pdf",FilePath="C:\\Uploads\\2025\\Sales\\Client_List.pdf", UserProfileId = users[2].Id, FileSize = 210, UploadedAt = DateTime.Now.AddDays(-3)},
            new FileRecord { FileName = "Training_Materials.pdf", FilePath="C:\\Uploads\\2025\\HR\\Training_Materials.pdf", UserProfileId = users[7].Id, FileSize = 330, UploadedAt = DateTime.Now.AddDays(-18)},
            new FileRecord { FileName = "Annual_Review.pdf", FilePath="C:\\Uploads\\2024\\Reviews\\Annual_Review.pdf", UserProfileId = users[9].Id, FileSize = 400, UploadedAt = DateTime.Now.AddDays(-25)},
            new FileRecord { FileName = "Product_Catalog.pdf", FilePath="C:\\Uploads\\2025\\Products\\Product_Catalog.pdf", UserProfileId = users[10].Id, FileSize = 360, UploadedAt = DateTime.Now.AddDays(-6)},
            new FileRecord { FileName = "Vendor_Agreements.pdf", FilePath="C:\\Uploads\\2025\\Legal\\Vendor_Agreements.pdf", UserProfileId = users[11].Id, FileSize = 280, UploadedAt = DateTime.Now.AddDays(-14)},
            new FileRecord { FileName = "IT_Policies.pdf", FilePath="C:\\Uploads\\2025\\IT\\IT_Policies.pdf", UserProfileId = users[7].Id, FileSize = 310, UploadedAt = DateTime.Now.AddDays(-9)},
            new FileRecord { FileName = "Office_Layout.pdf", FilePath="C:\\Uploads\\2025\\Facilities\\Office_Layout.pdf", UserProfileId = users[3].Id, FileSize = 230, UploadedAt = DateTime.Now.AddDays(-4)},
            new FileRecord { FileName = "Conference_Schedule.pdf", FilePath="C:\\Uploads\\2025\\Events\\Conference_Schedule.pdf", UserProfileId = users[1].Id, FileSize = 190, UploadedAt = DateTime.Now.AddDays(-11) },
            new FileRecord { FileName = "Performance_Metrics.pdf", FilePath="C:\\Uploads\\2025\\Analytics\\Performance_Metrics.pdf", UserProfileId = users[3].Id, FileSize = 420, UploadedAt = DateTime.Now.AddDays(-13)},
            new FileRecord { FileName = "Strategic_Plan.pdf", FilePath="C:\\Uploads\\2025\\Plans\\Strategic_Plan.pdf", UserProfileId = users[18].Id, FileSize = 350, UploadedAt = DateTime.Now.AddDays(-17) },
            new FileRecord { FileName = "Customer_Feedback.pdf", FilePath="C:\\Uploads\\2025\\Support\\Customer_Feedback.pdf", UserProfileId = users[7].Id, FileSize = 260, UploadedAt = DateTime.Now.AddDays(-2) },
            new FileRecord { FileName = "Industry_Report.pdf", FilePath="C:\\Uploads\\2025\\Research\\Industry_Report.pdf", UserProfileId = users[18].Id, FileSize = 370, UploadedAt = DateTime.Now.AddDays(-16) },
            new FileRecord { FileName = "Press_Release.pdf", FilePath="C:\\Uploads\\2025\\PR\\Press_Release.pdf", UserProfileId = users[19].Id, FileSize = 240, UploadedAt = DateTime.Now.AddDays(-1) },
            new FileRecord { FileName = "Acquisition_Details.pdf", FilePath="C:\\Uploads\\2025\\Finance\\Acquisition_Details.pdf", UserProfileId = users[18].Id, FileSize = 390, UploadedAt = DateTime.Now.AddDays(-22) },
            new FileRecord { FileName = "Expansion_Plan.pdf", FilePath="C:\\Uploads\\2025\\Plans\\Expansion_Plan.pdf", UserProfileId = users[21].Id, FileSize = 340, UploadedAt = DateTime.Now.AddDays(-21)},
            new FileRecord { FileName = "Team_Structure.pdf", FilePath="C:\\Uploads\\2025\\HR\\Team_Structure.pdf", UserProfileId = users[17].Id, FileSize = 220, UploadedAt = DateTime.Now.AddDays(-19) },
            new FileRecord { FileName = "Sales_Forecast.pdf", FilePath="C:\\Uploads\\2025\\Finance\\Sales_Forecast.pdf", UserProfileId = users[5].Id, FileSize = 330, UploadedAt = DateTime.Now.AddDays(-23) },
            new FileRecord { FileName = "Internship_Program.pdf", FilePath="C:\\Uploads\\2025\\HR\\Internship_Program.pdf", UserProfileId = users[7].Id, FileSize = 180, UploadedAt = DateTime.Now.AddDays(-24) },
            new FileRecord { FileName = "Supply_Chain_Overview.pdf", FilePath="C:\\Uploads\\2025\\Logistics\\Supply_Chain_Overview.pdf", UserProfileId = users[9].Id, FileSize = 410, UploadedAt = DateTime.Now.AddDays(-27) },
            new FileRecord { FileName = "Market_Analysis.pdf", FilePath="C:\\Uploads\\2025\\Marketing\\Market_Analysis.pdf", UserProfileId = users[4].Id, FileSize = 200, UploadedAt = DateTime.Now.AddDays(-26) },
            new FileRecord { FileName = "Innovation_Strategy.pdf", FilePath="C:\\Uploads\\2025\\R&D\\Innovation_Strategy.pdf", UserProfileId = users[20].Id, FileSize = 360, UploadedAt = DateTime.Now.AddDays(-28) }
        };

        context.Files.AddRange(files);
    }
}