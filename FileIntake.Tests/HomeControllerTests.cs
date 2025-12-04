using Microsoft.AspNetCore.Mvc;

namespace FileIntake.Tests;

public class HomeControllerTests : ControllerTestBase
{
    [Fact]
    public void Index_HomeController_ViewResult()
    {
        // Act
        var result = _homeController.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_HomeController_ViewResult()
    {
        // Act
        var result = _homeController.Privacy();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_HomeController_ViewResult()
    {
        // Act
        var result = _homeController.Error();

        // Assert
        Assert.IsType<ViewResult>(result);
    }
}