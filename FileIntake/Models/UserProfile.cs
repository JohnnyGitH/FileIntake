using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace FileIntake.Models;
[ExcludeFromCodeCoverage]
public class UserProfile
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";

    // Like to IdentityUser for authentication
    public string? IdentityUserId { get; set; }
    public IdentityUser? IdentityUser { get; set; }

    // Navigation property to files
    public ICollection<FileRecord>? Files { get; set; }
}