using System.Diagnostics.CodeAnalysis;
using FileIntake.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FileIntake.Data; 
[ExcludeFromCodeCoverage]
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FileMetadata> Metadata { get; set; }
    public virtual DbSet<FileTag> FileTags { get; set; }
    public virtual DbSet<FileRecord> Files { get; set; }
    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
