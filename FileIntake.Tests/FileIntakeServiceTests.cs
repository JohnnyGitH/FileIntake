using FileIntake.Data;
using FileIntake.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
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
        _context.Files.AddRange(new List<FileIntake.Models.FileRecord>
        {
            new Models.FileRecord { Id = 1, FileName = "FileA.txt", UploadedAt = DateTime.UtcNow.AddDays(-2) },
            new Models.FileRecord { Id = 2, FileName = "FileB.txt", UploadedAt = DateTime.UtcNow.AddDays(-1) },
            new Models.FileRecord { Id = 3, FileName = "FileC.txt", UploadedAt = DateTime.UtcNow }
        });
        _context.SaveChanges();

        _service = new FileIntakeService(_context, _loggerMock.Object);
    }

    [Fact]
    public void Test1()
    {
        Assert.True(true);
    }

    // [Fact]
    // public void FileIntakeService_SortingOrder_ValidSortingOrder()
    // {

    // }
}