using FileIntake.Models;
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

        public DbSet<FileMetadata> Metadata { get; set; }
        public DbSet<FileTag> FileTags { get; set; }
        public DbSet<FileRecord> Files { get; set; }
        public DbSet<UserProfile> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}