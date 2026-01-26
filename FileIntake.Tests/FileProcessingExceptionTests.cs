using FileIntake.Exceptions;

namespace FileIntake.Tests;

public class FileProcessingExceptionTests
{
    [Fact]
    public void FileProcessingException_ConstructorWithMessage_Works()
    {
        // Arrange
        var message = "File Processing gone wrong";

        // Act
        var exception = new FileProcessingException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.NotNull(exception);
    }

}