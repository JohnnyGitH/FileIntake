namespace FileIntake.Models
{
    public class FileTag
    {
        public int Id { get; set; }
        public string TagName { get; set; }

        // Foreign key to associate with FileRecord
        public int FileRecordId { get; set; }
        public FileRecord FileRecord { get; set; }
    }
}