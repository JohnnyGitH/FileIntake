using System;

namespace FileIntake.Models.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class AiPromptAttribute : Attribute
{
    public string Prompt { get; }

    public AiPromptAttribute(string prompt)
    {
        Prompt = prompt;
    } 
}