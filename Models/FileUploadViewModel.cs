using System.Collections.Generic;

namespace FileIntake.Models;
public class FileUploadViewModel
{
    public required IEnumerable<FileRecord> FileRecords { get; set; }
    public FileRecord? UploadedFileRecord { get; set; }
}