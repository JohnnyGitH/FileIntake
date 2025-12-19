using System;
using System.Linq;
using System.Reflection;
using FileIntake.Models.Attributes;
using FileIntake.Models.Enums;

namespace FileIntake.Models.Extensions;

/// <summary>
/// Extension methods for <see cref="AiQueryType"/> that provide access
/// to metadata defined via custom attributes.
/// </summary>
public static class AiQueryTypeExtensions
{
    /// <summary>
    /// Retrieves the AI system prompt associated with the given ai query type.
    /// Throws if the enum value is missing required prompt metadata.
    /// </summary>
    public static string GetPrompt(this AiQueryType queryType)
    {
        // Get the enum member from AiQueryType
        var member = typeof(AiQueryType)
            .GetMember(queryType.ToString())
            .First();
        // Use the enum member to get the correct attribute
        var attribute = member
            .GetCustomAttributes<AiPromptAttribute>()
            .FirstOrDefault();

        return attribute?
            .Prompt ?? throw new InvalidOperationException($"Missing AiPromptAttribute for {queryType}");
    }
}