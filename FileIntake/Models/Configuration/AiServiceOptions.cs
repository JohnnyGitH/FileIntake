using System.Diagnostics.CodeAnalysis;

namespace FileIntake.Models.Configuration;

[ExcludeFromCodeCoverage]
public class AiServiceOptions
{
    public string BaseUrl { get; set;} = "http://localhost:8000";
}