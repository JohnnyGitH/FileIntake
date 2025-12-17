using System.Text.Json.Serialization;

namespace FileIntake.Models.DTO;

public class AiRequestDto
{
    [JsonPropertyName("text")]
    public string Text { get; set;} = string.Empty;
}