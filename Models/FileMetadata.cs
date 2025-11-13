namespace FileIntake.Models
{
    public class FileMetadata
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        // Foreign key to associate with FileRecord
        public int FileRecordId { get; set; }
        public FileRecord FileRecord { get; set; }
    }
}