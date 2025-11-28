using FileIntake.Models;

namespace FileIntake.Tests;

public class FileUploadViewModelTests()
{
    [Fact]
    public void FileUploadViewModel_ViewModelHasUploadedFileRecord_IsFileUploadTrue()
    {
        // Arrange
        FileRecord uploaded = new FileRecord()
        {
            Id = 1,
            FileName = "",
            FileSize = 10,
            UploadedAt = DateTime.Now,
            UserProfileId = 1,
        };

        // Act
        var fileUploadedVM = new FileUploadViewModel()
        {
             FileRecords = [],
             UploadedFileRecord = uploaded
        };

        // Assert
        Assert.True(fileUploadedVM.IsFileUploaded);
    }

    [Fact]
    public void FileUploadViewModel_ViewModelHasNoUploadedFileRecord_IsFileUploadFalse()
    {
        // Act
        var fileUploadedVM = new FileUploadViewModel()
        {
             FileRecords = [],
             UploadedFileRecord = null
        };

        // Assert
        Assert.False(fileUploadedVM.IsFileUploaded);
    }
}