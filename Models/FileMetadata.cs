namespace FileIntake.Models
{
    public class FileMetadata
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        // Foreign key to associate with FileRecord
        public FileRecord? FileRecord { get; set; }
    }
}