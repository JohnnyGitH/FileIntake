using FileIntake.Models;

namespace FileIntake.Tests;

public class ErrorViewModelTests()
{
    [Fact]
    public void ErrorViewModel_RequestIdIsNotNullOrEmpty_ShowRequestIdIsTrue()
    {
        // Arrange / Act
        var ErrorVM = new ErrorViewModel()
        {
            RequestId = "1"
        };

        // Assert
        Assert.True(ErrorVM.ShowRequestId);
    }

    [Fact]
    public void ErrorViewModel_RequestIdIsNull_ShowRequestIdIsFalse()
    {
        // Arrange / Act
        var ErrorVM = new ErrorViewModel()
        {
            RequestId = null
        };

        // Assert
        Assert.False(ErrorVM.ShowRequestId);
    }

    [Fact]
    public void ErrorViewModel_RequestIdIsEmpty_ShowRequestIdIsFalse()
    {
        // Arrange / Act
        var ErrorVM = new ErrorViewModel()
        {
            RequestId = ""
        };

        // Assert
        Assert.False(ErrorVM.ShowRequestId);
    }
}