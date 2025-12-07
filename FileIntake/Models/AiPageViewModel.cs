using FileIntake.Models;

namespace FileIntake;

public class AiPageViewModel
{
        public FileRecord? UploadedFileRecord { get; set; }
        public string? AIPromptResponse { get; set; }
        public bool HasFile => UploadedFileRecord != null;

}