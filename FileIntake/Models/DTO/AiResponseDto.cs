using System.Text.Json.Serialization;

namespace FileIntake.Models.DTO;

public class AiResponseDto
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}