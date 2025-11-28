using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;

namespace FileIntake.Tests;

public class AccountControllerTests : ControllerTestBase
{
    [Fact]
    public void Login_AccountController_ViewResult()
    {
        // Act
        var result = _accountController.Login();

        // Assert
        Assert.IsType<ViewResult>(result);
    }
}