using FileIntake.Models;
using FileIntake.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FileIntake.Tests;

public class AIControllerTests : ControllerTestBase
{
    [Fact]
    public async Task AIController_GetIndex_NullFileId_RedirectsToFileIntakeIndex_SetsTempDataError()
    {
        // Arrange / Act
        var result = await _aiController.Index(null!, new AiPageViewModel());

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("FileIntake", redirect.ControllerName);
        Assert.Equal("No file selected for upload.", _aiController.TempData["Error"]);
    }

    [Fact]
    public async Task AIController_PostIndex_ValidAiPageViewModel_CallsAiServiceAndReturnsWithResponse()
    {
        // Arrange
        _aiProcessingServiceMock
            .Setup(s => s.AiProcessAsync(It.IsAny<string>(),It.IsAny<string>()))
            .ReturnsAsync(new AiProcessingResult { success = true, aiResponse = "hello from ai"});

        var vModel = new AiPageViewModel
        {
            UploadedFileRecord = new FileRecord { FileText = "my pdf text", Id = 1, FileName = "TestPDF"},
            SelectedQueryType = AiQueryType.Summarize
        };

        // Act
        var result = await _aiController.Index(vModel);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<AiPageViewModel>(view.Model);

        Assert.Equal("hello from ai", returnedModel.AIPromptResponse);

        _aiProcessingServiceMock.Verify(s => s.AiProcessAsync("my pdf text","Summarize"), Times.Once);
    }
    
    [Fact]
    public async Task AIController_PostIndex_InvalidAiPageViewModel_RedirectsToFileIntakeIndex_SetsTempDataError()
    {
        // Arrange
        var vModel = new AiPageViewModel
        {
            UploadedFileRecord = null
        };

        // Act
        var result = await _aiController.Index(vModel);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("FileIntake", redirect.ControllerName);
        Assert.Equal("Request file does not exist.", _aiController.TempData["Error"]);
    }
}