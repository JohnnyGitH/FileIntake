using FileIntake.Models.Attributes;

namespace FileIntake.Models.Enums;

/// <summary>
/// Defines the supported AI query modes available to the user.
/// Each enum value represents a distinct intent (e.g., summarization or simplification)
/// and is paired with an AI prompt via <see cref="AiPromptAttribute"/>.
/// </summary>
public enum AiQueryType
{
    [AiPrompt("You are a professional text evaluator. Please summarize the following:\n\n")]
    Summarize,

    [AiPrompt("Explain the following like I am 5 years old:\n\n")]
    ELI5,

    [AiPrompt("Break the following document into point-form notes:\n\n")]
    PointForm
}