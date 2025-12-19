using System.Collections.Generic;
using FileIntake.Models;
using FileIntake.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FileIntake;

public class AiPageViewModel
{
        public FileRecord? UploadedFileRecord { get; set; }
        public string? AIPromptResponse { get; set; }
        public bool HasFile => UploadedFileRecord != null;

        // Dropdown properties for the Ai query
        public List<SelectListItem> QueryTypes { get; set; } = new List<SelectListItem>();
        public AiQueryType SelectedQueryType { get; set; }

}