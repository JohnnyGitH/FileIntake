using FileIntake.Data;
using FileIntake.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
namespace FileIntake.Tests;

public class FileIntakeServiceTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<FileIntakeService>> _loggerMock;

    public FileIntakeServiceTests()
    {
        _loggerMock = new Mock<ILogger<FileIntakeService>>();
        _contextMock = new Mock<ApplicationDbContext>();
    }

    [Fact]
    public void Test1()
    {
        Assert.True(true);
    }

    [Fact]
    public void FileIntakeService_SortingOrder_ValidSortingOrder()
    {
        _contextMock.Setup(c => c.).  ReturnsDbSet(new List<FileIntake.Models.FileRecord>());
        Assert.True(true);
    }
}