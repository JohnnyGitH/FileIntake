using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FileIntake.Models;

[ExcludeFromCodeCoverage]
public class FileRecord
{
    public int Id { get; set; }
    public required string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? ContentType { get; set; }
    public string? FileText {get; set;}

    // Foreign key to associate with UserProfile
    public int UserProfileId { get; set; }
    public UserProfile? UserProfile { get; set; }

    // Relationships
    public ICollection<FileTag>? Tags { get; set; } = new List<FileTag>();
    public ICollection<FileMetadata>? Metadata { get; set; } = new List<FileMetadata>();
}
