using FileIntake.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FileIntake.Tests;

public class FileIntakeControllerTests : ControllerTestBase
{

    [Fact]
    public async Task FileIntakeController_IndexReturnsView_WithCorrectData()
    {
        // Arrange
        var expectedFiles = new List<FileRecord>
        {
            new FileRecord { Id = 1, FileName = "FileA.txt" },
            new FileRecord { Id = 2, FileName = "FileB.txt" }
        };
        _fileIntakeServiceMock
            .Setup(s => s.GetRecentFilesAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(expectedFiles);

        // Act + Assert
        var sortOrder = "name_desc";
        var result = await _controller.Index("name_desc");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<FileUploadViewModel>(viewResult.Model);
    }
}