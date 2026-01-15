using System;
using System.Diagnostics.CodeAnalysis;

namespace FileIntake.Models.Attributes;

/// <summary>
/// Attribute used to associate an AI system prompt with an <see cref="AiQueryType"/> enum value.
/// This enables clean separation between UI intent and AI prompt configuration.
/// </summary>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Field)]
public sealed class AiPromptAttribute : Attribute
{
    public string Prompt { get; }

    public AiPromptAttribute(string prompt)
    {
        Prompt = prompt;
    } 
}