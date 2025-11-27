using System.Diagnostics.CodeAnalysis;

namespace FileIntake.Models;
[ExcludeFromCodeCoverage]
public class FileTag
{
    public int Id { get; set; }
    public string TagName { get; set; } = string.Empty;

    // Foreign key to associate with FileRecord
    public FileRecord? FileRecord { get; set; }
}
