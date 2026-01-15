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
    protected readonly Mock<IFileProcessingService> _fileProcessingServiceMock;
    protected readonly Mock<IAiProcessingService> _aiProcessingServiceMock;
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

    /// <summary>
    /// Provides a fully-mocked environment for controller unit tests.
    ///
    /// This class centralizes setup for all shared dependencies used across
    /// controller tests (UserManager, DbContext, TempData, services, HttpContext,
    /// etc.).  
    ///
    /// Why this exists:
    /// - The Controllers require many tightly-coupled ASP.NET Core components  
    ///   (UserManager, HttpContext, ClaimsPrincipal, TempData, EF DbContext).  
    /// - Re-creating that boilerplate in every test file would be repetitive,
    ///   error-prone, and extremely noisy.
    /// - This class ensures consistent, realistic controller setup while allowing
    ///   individual tests to focus strictly on behavior.
    ///
    /// What it configures:
    /// - An in-memory ApplicationDbContext + mocked DbSets for async EF queries  
    /// - A fully mocked UserManager (including identity claims)  
    /// - Shared mocks for IFileIntakeService and IFileProcessingService  
    /// - Working TempData via TempDataDictionary  
    /// - A valid HttpContext, authenticated user, and controller context  
    /// - Preconfigured FileIntakeController, HomeController, AccountController, and AIController
    ///
    /// Test classes inherit from this base so they get all dependencies pre-wired,
    /// making each test file dramatically smaller, cleaner, and easier to maintain.
    /// </summary>
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
        _fileProcessingServiceMock = new Mock<IFileProcessingService>();
        _aiProcessingServiceMock = new Mock<IAiProcessingService>();
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
            _fileProcessingServiceMock.Object,
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
            _fileProcessingServiceMock.Object,
            _userManagerMock.Object,
            _context.Object
        );

        _aiController = new AIController(
            _fileIntakeServiceMock.Object, 
            _aiProcessingServiceMock.Object
        );
        _aiController.ControllerContext = _controller.ControllerContext;


        // Mock ITempDataProvider and create the TempDataDictionary
        var mockTempDataProvider = new Mock<ITempDataProvider>();
        _controller.TempData = new TempDataDictionary(
            _controller.HttpContext, // Use the HttpContext we just set up
            mockTempDataProvider.Object // Provide the necessary mock service
        );

        _aiController.TempData = new TempDataDictionary(
            _aiController.HttpContext,
            mockTempDataProvider.Object
        );
    }
}