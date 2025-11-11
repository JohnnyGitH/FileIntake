using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// TODO: Customize the ApplicationDbContext as needed for my application
namespace FileIntake.Data 
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // TODO: Add my application's database tables (DbSet properties) here later.
        // Example: public DbSet<FileIntake.Models.MyFile> MyFiles { get; set; }
    }
}