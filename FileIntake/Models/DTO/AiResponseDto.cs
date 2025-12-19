using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace FileIntake.Models.DTO;

public class AiResponseDto
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}