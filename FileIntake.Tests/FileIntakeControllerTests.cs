using System.Security.Claims;
using FileIntake.Controllers;
using FileIntake.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.EntityFrameworkCore;
using static TestHelpers;

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
        string pdfPath = Path.Combine("TestFiles", "Sample1.pdf");
        byte[] pdfBytes = PdfTestHelper.CreateMinimalPdf();
        var stream = new MemoryStream(pdfBytes);
        var expectedId = 123;
        var successMessage = "File uploaded successfully.";
        var fileProcessingResult = 

        PdfTestHelper.SaveMinimalPdfTo(pdfPath);

        await using var pdfStream = File.OpenRead(pdfPath);

        var formFile = new FormFile(stream, 0, pdfBytes.Length, "file", "Sample1.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        _fileProcessingServiceMock
            .Setup(s => s.ProcessFile(It.IsAny<IFormFile>(), It.IsAny<int>()))
            .ReturnsAsync(new FileProcessingResult()
                {
                    success = true,
                    FileRecord = new FileRecord { Id = 123, FileName = "test" },
                    SavedToDatabase = true
                });

        // Mock service to return record with Id from GetFileByIdAsync
        // _fileIntakeServiceMock
        //     .Setup(s => s.AddFileAsync(It.IsAny<FileRecord>()))
        //     .Callback<FileRecord>(f => f.Id = expectedId)
        //     .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Upload(formFile);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(FileIntakeController.Index), redirectResult.ActionName);
        Assert.Null(redirectResult.ControllerName);
        Assert.Equal(successMessage, _controller.TempData["Success"]);
        Assert.Equal(expectedId.ToString(), _controller.TempData["UploadedFileId"]);

        _fileProcessingServiceMock.Verify(s =>s.ProcessFile(It.IsAny<IFormFile>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task FileIntakeController_FileUploadFailure_InvalidIdentityUser()
    {
        // Arrange
        var errorMessage = "User not found.";
        var fileName = "fileA";
        var expectedControllerName = "Account";
        var uploadedFile = CreateMockPDFFile(fileName);
        
        // Mock UserManager
        _userManagerMock
            .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((IdentityUser)null);

        // Act
        var result = await _controller.Upload(uploadedFile);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(AccountController.Login), redirectResult.ActionName);
        Assert.Equal(expectedControllerName, redirectResult.ControllerName);
        Assert.Equal(errorMessage, _controller.TempData["Error"]);

        _fileIntakeServiceMock.Verify(s =>s.AddFileAsync(It.IsAny<FileRecord>()), Times.Never);
    }

    [Fact]
    public async Task FileIntakeController_FileUploadFailure_FileNull()
    {
        // Arrange
        var errorMessage = "No file selected for upload.";
        var fileName = "fileA";
        var uploadedFile = CreateMockPDFFile(fileName, true);
        var nullFile = (IFormFile)null;

        _fileProcessingServiceMock
            .Setup(s => s.ProcessFile(It.IsAny<IFormFile>(), It.IsAny<int>()))
            .ReturnsAsync(new FileProcessingResult
                {
                    success = false,
                    ErrorMessage = "No file selected for upload."
                });

        // Act
        var result = await _controller.Upload(nullFile);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(FileIntakeController.Index), redirectResult.ActionName);
        Assert.Null(redirectResult.ControllerName);
        Assert.Equal(errorMessage, _controller.TempData["Error"]);

        _fileProcessingServiceMock.Verify(s =>s.ProcessFile(It.IsAny<IFormFile>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task FileIntakeController_FileUploadFailure_FileLengthZero()
    {
        // Arrange
        var errorMessage = "No file selected for upload.";
        var fileName = "fileA";
        var uploadedFile = CreateMockPDFFile(fileName, true);

        _fileProcessingServiceMock
            .Setup(s => s.ProcessFile(It.IsAny<IFormFile>(), It.IsAny<int>()))
            .ReturnsAsync(new FileProcessingResult
                {
                    success = false,
                    ErrorMessage = "No file selected for upload."
                });

        // Act
        var result = await _controller.Upload(uploadedFile);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(FileIntakeController.Index), redirectResult.ActionName);
        Assert.Null(redirectResult.ControllerName);
        Assert.Equal(errorMessage, _controller.TempData["Error"]);

        _fileIntakeServiceMock.Verify(s =>s.AddFileAsync(It.IsAny<FileRecord>()), Times.Never);
    }

    [Fact]
    public async Task FileIntakeController_FileUploadFailure_UserProfileIsNullTempDataError()
    {
        // Arrange
        var successMessage = "User profile not found.";
        var fileName = "fileA";
        var expectedControllerRedirect = "Home";
        var uploadedFile = CreateMockPDFFile(fileName);
        var identityUser = new IdentityUser{ Id ="Test-user-id" };

        _userManagerMock
            .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(identityUser);

        _context
            .Setup(c => c.UserProfiles)
            .ReturnsDbSet(new List<UserProfile>());

        _fileIntakeServiceMock
            .Setup(s => s.GetRecentFilesAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<FileRecord>());

        // Act
        var result = await _controller.Upload(uploadedFile);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(FileIntakeController.Index), redirectResult.ActionName);
        Assert.Equal(expectedControllerRedirect, redirectResult.ControllerName);
        Assert.Equal(successMessage, _controller.TempData["Error"]);

        _fileIntakeServiceMock.Verify(s =>s.AddFileAsync(It.IsAny<FileRecord>()), Times.Never);
    }

    [Fact]
    public async Task FileIntakeController_FileUploadFailure_CatchExceptionSetTempDataError()
    {
        // Arrange
        var errorMessage = "Error uploading file.";
        var uploadedFileId = 99;
        var fileName = "fileA";
        var uploadedFile = CreateMockPDFFile(fileName);

        // Mock service to return record with Id from GetFileByIdAsync
        _fileProcessingServiceMock
            .Setup(s => s.ProcessFile(It.IsAny<IFormFile>(), It.IsAny<int>()))
            .ReturnsAsync(new FileProcessingResult
                {
                    success = false,
                    ErrorMessage = errorMessage
                });
        // _fileIntakeServiceMock
        //     .Setup(s => s.AddFileAsync(It.IsAny<FileRecord>()))
        //     .ThrowsAsync(new InvalidOperationException("Error uploading file: "));

        // _fileIntakeServiceMock
        //     .Setup(s => s.GetRecentFilesAsync(It.IsAny<int>(), It.IsAny<string>()))
        //     .ReturnsAsync(new List<FileRecord>());

        // Act
        var result = await _controller.Upload(uploadedFile);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(FileIntakeController.Index), redirectResult.ActionName);
        Assert.Null(redirectResult.ControllerName);
        Assert.Equal(errorMessage, _controller.TempData["Error"]);

        _fileProcessingServiceMock.Verify(s =>s.ProcessFile(It.IsAny<IFormFile>(), It.IsAny<int>()), Times.Once);
    }
}