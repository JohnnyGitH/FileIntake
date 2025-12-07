using System.Security.Claims;
using FileIntake.Controllers;
using FileIntake.Data;
using FileIntake.Interfaces;
using FileIntake.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FileIntake.Tests;

public class ControllerTestBase
{
    protected readonly Mock<ApplicationDbContext> _context;
    protected readonly Mock<IFileIntakeService> _fileIntakeServiceMock;
    protected readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    protected readonly HomeController _homeController;
    protected readonly AccountController _accountController;
    protected readonly FileIntakeController _controller;
    protected readonly AIController _aiController;
    protected readonly FileIntakeController _fileIntakeController;

    protected readonly string TEST_USER_ID = "test-user-id-1";
    protected readonly int TEST_PROFILE_ID = 13;
    protected readonly IdentityUser TEST_IDENTITY_USER;
    protected UserProfile TEST_USER_PROFILE;

    protected ControllerTestBase()
    {
        // Init consts first
        TEST_IDENTITY_USER = new IdentityUser { Id = TEST_USER_ID, UserName = "testuser", Email = "testing123@example.com"};
        TEST_USER_PROFILE = new UserProfile
        {
            Id = TEST_PROFILE_ID,
            IdentityUserId = TEST_USER_ID,
            FirstName = "Test",
            LastName = "User",
            Email = "testing123@example.com"
        };

        // Create a mock IQueryable for the IdentityDbContext to use, even if empty
        var emptyData = new List<object>().AsQueryable(); 
        var mockEmptyDbSet = new Mock<DbSet<object>>();
        mockEmptyDbSet.As<IQueryable<object>>().Setup(m => m.Provider).Returns(emptyData.Provider);

        // Config DBset
        var userProfilesData = new List<UserProfile> { TEST_USER_PROFILE }.AsQueryable();
        var mockDbSetUserProfiles = new Mock<DbSet<UserProfile>>();

        // Mock dependencies
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

        _fileIntakeServiceMock = new Mock<IFileIntakeService>();
        _context = new Mock<ApplicationDbContext>(options);

        var _userStoreMock = new Mock<IUserStore<IdentityUser>>();
        
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            _userStoreMock.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<IdentityUser>>().Object,
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new IdentityErrorDescriber(),
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<IdentityUser>>>().Object
            );

        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(TEST_IDENTITY_USER);

        // Setup IQueryable provider for async methods (requires TestAsyncQueryProvider)
        mockDbSetUserProfiles.As<IQueryable<UserProfile>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<UserProfile>(userProfilesData.Provider)); 
        // Setup IQueryable expression
        mockDbSetUserProfiles.As<IQueryable<UserProfile>>().Setup(m => m.Expression).Returns(userProfilesData.Expression);
        mockDbSetUserProfiles.As<IQueryable<UserProfile>>().Setup(m => m.ElementType).Returns(userProfilesData.ElementType);
        // Setup IQueryable GetEnumerator
        mockDbSetUserProfiles.As<IQueryable<UserProfile>>().Setup(m => m.GetEnumerator()).Returns(userProfilesData.GetEnumerator());
        // Setup IAsyncEnumerable (requires TestAsyncEnumerable/TestAsyncEnumerator)
        mockDbSetUserProfiles.As<IAsyncEnumerable<UserProfile>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<UserProfile>(userProfilesData.GetEnumerator()));

        _context.Setup(c => c.UserProfiles).Returns(mockDbSetUserProfiles.Object);

        _controller = new FileIntakeController(
            _fileIntakeServiceMock.Object,
            _userManagerMock.Object,
            _context.Object
        );


        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, TEST_USER_ID),
                    new Claim(ClaimTypes.Name, "testuser"),
                }, "TestAuthentication"))
            }
        };

        _homeController = new HomeController();
        _homeController.ControllerContext = _controller.ControllerContext;

        _accountController = new AccountController();
        _accountController.ControllerContext = _controller.ControllerContext;

        _fileIntakeController = new FileIntakeController(
            _fileIntakeServiceMock.Object,
            _userManagerMock.Object,
            _context.Object
        );

        _aiController = new AIController(_fileIntakeServiceMock.Object);
        _aiController.ControllerContext = _controller.ControllerContext;


        // Mock ITempDataProvider and create the TempDataDictionary
        var mockTempDataProvider = new Mock<ITempDataProvider>();
        _controller.TempData = new TempDataDictionary(
            _controller.HttpContext, // Use the HttpContext we just set up
            mockTempDataProvider.Object // Provide the necessary mock service
        );
    }
}