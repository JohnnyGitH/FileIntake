using System.Text.Json.Serialization;

namespace FileIntake.Models.DTO;

public sealed record AiRequestDto
{
    [JsonPropertyName("text")]
    public string Text { get; init;} = string.Empty;
}