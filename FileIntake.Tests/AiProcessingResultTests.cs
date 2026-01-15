namespace FileIntake.Tests;

public class AiProcessingResultTests()
{
    [Fact]
    public void AiProcessingResult_IsResponseUpdated_ReturnsTrue()
    {
        // Arrange / Act
        var model = new AiProcessingResult()
        {
            success = true,
            aiResponse = "this is an ai response beep boop"
        };

        // Assert
        Assert.True(model.IsResponseUpdated);
    }

    [Fact]
    public void AiProcessingResult_IsResponseUpdated_ReturnsFalse()
    {
        // Arrange / Act
        var model = new AiProcessingResult()
        {
            success = false,
        };

        // Assert
        Assert.False(model.IsResponseUpdated);
    }
} 