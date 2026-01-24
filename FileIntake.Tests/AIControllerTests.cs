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
    public async Task AIController_GetIndex_FileServiceReturnsNull_RedirectsToFileIntakeIndex_SetsTempDataError()
    {
        // Arrange 
        _fileIntakeServiceMock
            .Setup(s =>s.GetFileByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((FileRecord)null!);

        var vModel = new AiPageViewModel();

        // Act
        var result = await _aiController.Index(1, vModel);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("FileIntake", redirect.ControllerName);
        Assert.Equal("Request file does not exist.", _aiController.TempData["Error"]);
    }

    [Fact]
    public async Task AIController_GetIndex_FileServiceReturnFile_ReturnsView_WithQueryTypes()
    {
        // Arrange 
        _fileIntakeServiceMock
            .Setup(s =>s.GetFileByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new FileRecord
            {
                Id = 1,
                FileName = "test.pdf",
                FileText = "hello file text"
            });

        var vModel = new AiPageViewModel
        {
            SelectedQueryType = AiQueryType.Summarize
        };

        // Act
        var result = await _aiController.Index(1, vModel);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AiPageViewModel>(view.Model);

        Assert.NotNull(model.UploadedFileRecord);
        Assert.Equal("hello file text", model.UploadedFileRecord.FileText);
        Assert.NotNull(model.QueryTypes);
        Assert.True(model.QueryTypes.Count > 0);
    }

    [Fact]
    public async Task AIController_PostIndex_ValidAiPageViewModel_CallsAiServiceAndReturnsWithResponse()
    {
        // Arrange
        _aiProcessingServiceMock
            .Setup(s => s.AiProcessAsync(It.IsAny<string>(),It.IsAny<AiQueryType>()))
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

        _aiProcessingServiceMock.Verify(s => s.AiProcessAsync("my pdf text",AiQueryType.Summarize), Times.Once);
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

    [Fact]
    public async Task AIController_PostIndex_AiProcessingServiceFails_CatchAndSetTempDataReturnView()
    {
        // Arrange
        _aiProcessingServiceMock
            .Setup(s => s.AiProcessAsync(It.IsAny<string>(),It.IsAny<AiQueryType>()))
            .ThrowsAsync(new Exception("boom"));

        var vModel = new AiPageViewModel
        {
            UploadedFileRecord = new FileRecord { Id = 1, FileName = "test.pdf", FileText = "hello this is pdf"},
            SelectedQueryType = AiQueryType.Summarize
        };

        // Act
        var result = await _aiController.Index(vModel);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(_aiController.TempData["Error"]);
        Assert.Contains("Ai Processing Failed. Exception :", _aiController.TempData["Error"]!.ToString());
    }

    [Fact]
    public async Task AIController_GetIndex_FileServiceReturnFile_ReturnsView_WithQueryTypeSummarize()
    {
        // Arrange 
        var expected_qType = "Summarize";
        _fileIntakeServiceMock
            .Setup(s =>s.GetFileByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new FileRecord
            {
                Id = 1,
                FileName = "test.pdf",
                FileText = "hello file text"
            });

        var vModel = new AiPageViewModel
        {
            SelectedQueryType = AiQueryType.Summarize
        };

        // Act
        var result = await _aiController.Index(1, vModel);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AiPageViewModel>(view.Model);

        Assert.NotNull(model.QueryTypes);
        Assert.Equal(expected_qType, model.SelectedQueryType.ToString());
    }

    [Fact]
    public async Task AIController_GetIndex_FileServiceReturnFile_ReturnsView_WithQueryTypeEL15()
    {
        // Arrange 
        var expected_qType = AiQueryType.ELI5;
        _fileIntakeServiceMock
            .Setup(s =>s.GetFileByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new FileRecord
            {
                Id = 1,
                FileName = "test.pdf",
                FileText = "hello file text"
            });

        var vModel = new AiPageViewModel
        {
            SelectedQueryType = AiQueryType.ELI5
        };

        // Act
        var result = await _aiController.Index(1, vModel);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AiPageViewModel>(view.Model);

        Assert.NotNull(model.QueryTypes);
        Assert.Equal(expected_qType,  model.SelectedQueryType);
    }

    [Fact]
    public async Task AIController_GetIndex_FileServiceReturnFile_ReturnsView_WithQueryTypePointForm()
    {
        // Arrange 
        var expected_qType = "PointForm";
        _fileIntakeServiceMock
            .Setup(s =>s.GetFileByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new FileRecord
            {
                Id = 1,
                FileName = "test.pdf",
                FileText = "hello file text"
            });

        var vModel = new AiPageViewModel
        {
            SelectedQueryType = AiQueryType.PointForm
        };

        // Act
        var result = await _aiController.Index(1, vModel);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AiPageViewModel>(view.Model);

        Assert.NotNull(model.QueryTypes);
        Assert.Equal(expected_qType, model.SelectedQueryType.ToString());
    }
}