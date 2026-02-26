using System.Text.Json.Serialization;

namespace FileIntake.Models.DTO;

public sealed record AiResponseDto
{
    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;
}