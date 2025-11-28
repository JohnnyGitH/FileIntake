using System.Text;
using FileIntake.Controllers;
using FileIntake.Models;
using Microsoft.AspNetCore.Http;
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

    [Fact]
    public async Task Index_AfterSuccessfulUpload_SetsUploadedFileRecordInModel()
    {
        // Arrange
        var uploadedFileId = 99;
        var expectedRecord = new FileRecord()
        {
            Id = uploadedFileId,
            FileName = "FileA.pdf"
        };

        // Mock service to return record with Id from GetFileByIdAsync
        _fileIntakeServiceMock
            .Setup(s => s.GetFileByIdAsync(uploadedFileId))
            .ReturnsAsync(expectedRecord);

        // Setup Controller properties
        _controller.TempData["UploadedFileId"] = uploadedFileId.ToString();

        _fileIntakeServiceMock
            .Setup(s => s.GetRecentFilesAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<FileRecord>());

        // Act
        var result = await _controller.Index(null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<FileUploadViewModel>(viewResult.Model);

        _fileIntakeServiceMock.Verify(s =>s.GetFileByIdAsync(uploadedFileId), Times.Once);
    }

    [Fact]
    public async Task Index_WithTempDataError_NoUploadedFileSetsTempDataError()
    {
        // Arrange
        var tempDataErrorMessage = "No file selected for upload.";

        // Setup Controller properties
        _controller.TempData["Error"] = tempDataErrorMessage;

        _fileIntakeServiceMock
            .Setup(s => s.GetRecentFilesAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<FileRecord>());

        // Act
        var result = await _controller.Index(null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<FileUploadViewModel>(viewResult.Model);

        Assert.Null(viewModel.UploadedFileRecord);
        Assert.Equal(tempDataErrorMessage, _controller.TempData["Error"]);

        _fileIntakeServiceMock.Verify(s =>s.GetFileByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task FileIntakeController_FileUploadSuccessful_SetsTempDataSuccessAndUploadedFileId()
    {
        // Arrange
        var successMessage = "File uploaded successfully.";
        var uploadedFileId = 99;
        var fileName = "fileA";
        // var expectedRecord = new FileRecord()
        // {
        //     Id = uploadedFileId,
        //     FileName = "FileA.pdf"
        // };
        var uploadedFile = TestHelpers.CreateMockFile(fileName);


        // Mock service to return record with Id from GetFileByIdAsync
        _fileIntakeServiceMock
            .Setup(s => s.AddFileAsync(It.IsAny<FileRecord>()))
            .Returns(Task.CompletedTask)
            .Callback<FileRecord>(r => r.Id = uploadedFileId);

        _fileIntakeServiceMock
            .Setup(s => s.GetRecentFilesAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<FileRecord>());

        // Act
        var result = await _controller.Upload(uploadedFile);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(FileIntakeController.Index), redirectResult.ActionName);
        Assert.Null(redirectResult.ControllerName);
        Assert.Equal(successMessage, _controller.TempData["Success"]);
        Assert.Equal(uploadedFileId.ToString(), _controller.TempData["UploadedFileId"]);

        _fileIntakeServiceMock.Verify(s =>s.AddFileAsync(It.IsAny<FileRecord>()), Times.Once);
    }
}