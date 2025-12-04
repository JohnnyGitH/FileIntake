using Microsoft.AspNetCore.Mvc;

namespace FileIntake.Tests;

public class AIControllerTests : ControllerTestBase
{
    [Fact]
    public void Index_AIController_ViewResult()
    {
        // Act
        var result = _aiController.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }
}