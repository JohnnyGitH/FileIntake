using System.Text;
using FileIntake.Exceptions;
using FileIntake.Interfaces;
using FileIntake.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using UglyToad.PdfPig.Core;
using static TestHelpers;

namespace FileIntake.Tests;

public class FileProcessingServiceTests
{
    private readonly Mock<IFileIntakeService> _fileIntakeServiceMock;
    private readonly FileProcessingService _service;

    public FileProcessingServiceTests()
    {
        _fileIntakeServiceMock = new Mock<IFileIntakeService>();
        _service = new FileProcessingService(_fileIntakeServiceMock.Object);
    }

    [Fact]
    public async Task FileProcessingService_ProcessFile_ValidFileAndIdSuccessTrue()
    {
        // Arrange
        byte[] pdfBytes = PdfTestHelper.CreateMinimalPdf();
        var stream = new MemoryStream(pdfBytes);
        var id = 123;
        var fileName = "Sample1.pdf";
        var formFile = new FormFile(stream, 0, pdfBytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        _fileIntakeServiceMock
            .Setup(s => s.AddFileAsync(It.IsAny<FileRecord>()));

        // Act
        var result = await _service.ProcessFile(formFile,id);

        // Assert
        Assert.IsType<FileProcessingResult>(result);
        Assert.True(result.success);
        Assert.True(result.SavedToDatabase);
        Assert.IsType<FileRecord>(result.FileRecord);
        Assert.Equal(result.FileRecord.FileName, fileName);
        Assert.Equal(result.FileRecord.UserProfileId, id);
    }

    [Fact]
    public async Task FileProcessingService_ProcessFile_FailInvalidFileLength()
    {
        // Arrange
        var errorMessage = "No file selected for upload.";
        var fileName = "fileA";
        var uploadedFile = CreateMockPDFFile(fileName,true);
        var id = 123;

        _fileIntakeServiceMock
            .Setup(s => s.AddFileAsync(It.IsAny<FileRecord>()));

        // Act
        var result = await _service.ProcessFile(uploadedFile,id);

        // Assert
        Assert.IsType<FileProcessingResult>(result);
        Assert.False(result.success);
        Assert.Null(result.FileRecord);
        Assert.Equal(result.ErrorMessage, errorMessage);
    }

    [Fact]
    public async Task FileProcessingService_ProcessFile_FailFileNull()
    {
        // Arrange
        var errorMessage = "No file selected for upload.";
        var uploadedFile = (IFormFile)null;
        var id = 123;

        _fileIntakeServiceMock
            .Setup(s => s.AddFileAsync(It.IsAny<FileRecord>()));

        // Act
        var result = await _service.ProcessFile(uploadedFile,id);

        // Assert
        Assert.IsType<FileProcessingResult>(result);
        Assert.False(result.success);
        Assert.Null(result.FileRecord);
        Assert.Equal(result.ErrorMessage, errorMessage);
    }

    [Fact]
    public async Task FileProcessingService_ProcessFile_PDFOpenFailException()
    {
        // Arrange
        byte[] pdfBytes = Encoding.UTF8.GetBytes("NoT A pDf");
        var errorMessage = "Failed to extract text from the PDF.";
        var stream = new MemoryStream(pdfBytes);
        var id = 123;
        var fileName = "Sample1broken.pdf";
        var invalidFormFile = new FormFile(stream, 0, pdfBytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        _fileIntakeServiceMock
            .Setup(s => s.AddFileAsync(It.IsAny<FileRecord>()));

        // Act
        var ex = await Assert.ThrowsAsync<FileProcessingException>(()=> _service.ProcessFile(invalidFormFile,id));

        // Assert
        Assert.IsType<PdfDocumentFormatException>(ex.InnerException);
        Assert.Contains(errorMessage, ex.Message);
    }

    [Fact]
    public async Task FileProcessingService_ProcessFile_AddFileAsyncFailException()
    {
        // Arrange
        byte[] pdfBytes = PdfTestHelper.CreateMinimalPdf();
        var stream = new MemoryStream(pdfBytes);
        var id = 123;
        var errorMessage = "Error uploading file: ";
        var fileIntakeErrorMessage = "Database insert failed";
        var fileName = "Sample1.pdf";
        var formFile = new FormFile(stream, 0, pdfBytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        _fileIntakeServiceMock
            .Setup(s => s.AddFileAsync(It.IsAny<FileRecord>()))
            .ThrowsAsync(new Exception(fileIntakeErrorMessage));

        // Act
        var result = await  _service.ProcessFile(formFile,id);

        // Assert
        Assert.IsType<FileProcessingResult>(result);
        Assert.False(result.success);
        Assert.False(result.SavedToDatabase);
        Assert.Equal(fileName, result.FileRecord.FileName);
        Assert.Contains(errorMessage, result.ErrorMessage);
        Assert.Contains(fileIntakeErrorMessage, result.ErrorMessage);
    }
}