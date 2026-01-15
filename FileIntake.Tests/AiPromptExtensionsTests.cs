using FileIntake.Models.Enums;
using FileIntake.Models.Extensions;

namespace FileIntake.Tests;

public class AiPromptExtensionsTests
{
    [Fact]
    public void AiPromptExtensions_GetPrompt_WithQueryTypeSummarize()
    {
        // Arrange
        var expectedPrompt = "You are a professional text evaluator. Please summarize the following:\n\n";
        var query = AiQueryType.Summarize;

        // Act
        var prompt = AiQueryTypeExtensions.GetPrompt(query);

        // Assert
        Assert.False(string.IsNullOrEmpty(prompt));
        Assert.Equal(expectedPrompt, prompt);
    }
}