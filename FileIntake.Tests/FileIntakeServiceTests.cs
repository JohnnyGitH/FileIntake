using FileIntake.Data;
using FileIntake.Models;
using FileIntake.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileIntake.Tests;

public class FileIntakeServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<FileIntakeService>> _loggerMock;
    private readonly FileIntakeService _service;

    public FileIntakeServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _loggerMock = new Mock<ILogger<FileIntakeService>>();
        _context = new ApplicationDbContext(options);

        // Seed test data for the whole class
        _context.Files.AddRange(new List<FileRecord>
        {
            new FileRecord 
            { 
                Id = 1, 
                FileName = "FileA.txt", 
                UploadedAt = DateTime.UtcNow.AddDays(-2),
                UserProfile = new UserProfile 
                { 
                    Id = 1, 
                    FirstName = "ATestUser" ,
                    LastName = "One",
                    Email = "user1@test.com"
                },
            },
            new FileRecord 
            { 
                Id = 2, 
                FileName = "FileB.txt", 
                UploadedAt = DateTime.UtcNow.AddDays(-1),
                UserProfile = new UserProfile 
                { 
                    Id = 2, 
                    FirstName = "BTestUser" ,
                    LastName = "Two",
                    Email = "user2@test.com"
                },
            },
            new FileRecord 
            { 
                Id = 3, 
                FileName = "FileC.txt", 
                UploadedAt = DateTime.UtcNow ,
                UserProfile = new UserProfile 
                { 
                    Id = 3, 
                    FirstName = "CTestUser" ,
                    LastName = "Three",
                    Email = "user3@test.com"
                },
            }
        });
        _context.SaveChanges();

        _service = new FileIntakeService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task FileIntakeService_SortingOrder_ValidSortingOrderNameDesc()
    {
        // Arrange
        var sortOrder = "name_desc";
        var expectedOrder = new List<string> { "FileC.txt", "FileB.txt", "FileA.txt" };

        // Act
        var result = await _service.GetRecentFilesAsync(3, sortOrder);

        // Assert
        var actualOrder = result.Select(f => f.FileName).ToList();
        Assert.Equal(expectedOrder, actualOrder);
    }

    [Fact]
    public async Task FileIntakeService_SortingOrder_ValidSortingOrderDate()
    {
        // Arrange
        var sortOrder = "Date";
        var expectedOrder = new List<string> { "FileA.txt", "FileB.txt", "FileC.txt" };

        // Act
        var result = await _service.GetRecentFilesAsync(3, sortOrder);

        // Assert
        var actualOrder = result.Select(f => f.FileName).ToList();
        Assert.Equal(expectedOrder, actualOrder);
    }

    [Fact]
    public async Task FileIntakeService_SortingOrder_ValidSortingOrderDateDesc()
    {
        // Arrange
        var sortOrder = "date_desc";
        var expectedOrder = new List<string> { "FileC.txt", "FileB.txt", "FileA.txt" };

        // Act
        var result = await _service.GetRecentFilesAsync(3, sortOrder);

        // Assert
        var actualOrder = result.Select(f => f.FileName).ToList();
        Assert.Equal(expectedOrder, actualOrder);
    }

    [Fact]
    public async Task FileIntakeService_SortingOrder_ValidSortingOrderUploader()
    {
        // Arrange
        var sortOrder = "Uploader";
        var expectedOrder = new List<string> { "ATestUser", "BTestUser", "CTestUser" };

        // Act
        var result = await _service.GetRecentFilesAsync(3, sortOrder);

        // Assert
        var actualOrder = result.Select(f => f.UserProfile.FirstName).ToList();
        Assert.Equal(expectedOrder, actualOrder);
    }

    [Fact]
    public async Task FileIntakeService_SortingOrder_ValidSortingOrderUploaderDesc()
    {
        // Arrange
        var sortOrder = "uploader_desc";
        var expectedOrder = new List<string> { "CTestUser", "BTestUser", "ATestUser" };

        // Act
        var result = await _service.GetRecentFilesAsync(3, sortOrder);

        // Assert
        var actualOrder = result.Select(f => f.UserProfile.FirstName).ToList();
        Assert.Equal(expectedOrder, actualOrder);
    }

    [Fact]
    public async Task FileIntakeService_SortingOrder_ValidSortingOrderDefault()
    {
        // Arrange
        var sortOrder = "";
        var expectedOrder = new List<string> { "FileA.txt", "FileB.txt", "FileC.txt" };

        // Act
        var result = await _service.GetRecentFilesAsync(3, sortOrder);

        // Assert
        var actualOrder = result.Select(f => f.FileName).ToList();
        Assert.Equal(expectedOrder, actualOrder);
    }

    [Fact]
    public async Task FileIntakeService_GetFileById_ExistingIdReturnsFile()
    {
        // Arrange
        var fileId = 2;
        var expectedFile = "FileB.txt";

        // Act
        var result = await _service.GetFileByIdAsync(fileId);

        // Assert
        Assert.NotNull(result);
        
        Assert.Equal(expectedFile, result.FileName);
    }

    [Fact]
    public async Task FileIntakeService_GetFileById_InvalidIdReturnsNothing()
    {
        // Arrange
        var fileId = 200;

        // Act
        var result = await _service.GetFileByIdAsync(fileId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FileIntakeService_AddFileAsyncAddFileSuccessfully()
    {
        // Arrange
        var newFile = new FileRecord
        {
            FileName = "NewFile.txt",
            UploadedAt = DateTime.UtcNow,
            UserProfile = new UserProfile 
            { 
                Id = 4, 
                FirstName = "DTestUser" ,
                LastName = "Four",
                Email = "newFileEmail@example.com"
            }
        };

        // Act
        await _service.AddFileAsync(newFile);
        var result = await _service.GetFileByIdAsync(newFile.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NewFile.txt", result.FileName);
    }

    [Fact]
    public async Task FileIntakeService_AddFileAsyncAddFileFailure()
    {
        // Arrange
        var newFile = new FileRecord
        {
            FileName = "NewFile.txt",
            UploadedAt = DateTime.UtcNow,
            UserProfile = new UserProfile 
            { 
                Id = 4, 
                FirstName = "DTestUser" ,
                LastName = "Four",
                Email = "newFileEmail@example.com"
            }
        };

        // Act
        await _service.AddFileAsync(newFile);
        var result = await _service.GetFileByIdAsync(newFile.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NewFile.txt", result.FileName);
    }
}