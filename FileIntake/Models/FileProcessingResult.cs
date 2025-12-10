namespace FileIntake.Models;

public class FileProcessingResult
{
    public bool success { get; set; }
    public string? ErrorMessage { get; set; }
    public FileRecord? FileRecord { get; set; }
    public bool? SavedToDatabase {get; set; }
    
}