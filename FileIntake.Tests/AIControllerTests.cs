using FileIntake.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FileIntake.Tests;

public class AIControllerTests : ControllerTestBase
{
    [Fact]
    public void Index_AIController_ViewResult()
    {
        // Arrange
        var file = _fileIntakeServiceMock
            .Setup(s => s.GetFileByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new FileRecord{ Id = 1, FileName="test.pdf"});

        // Act
        var result = _aiController.Index(1);

        // Assert
        Assert.IsType<Task<IActionResult>>(result);
    }
}