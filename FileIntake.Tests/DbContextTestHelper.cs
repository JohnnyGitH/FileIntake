using FileIntake.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FileIntake.Tests;

public static class DbContextTestHelper
{
    /// <summary>
    /// Creates a mock <see cref="DbSet{T}"/> backed by an in-memory
    /// <see cref="IQueryable{T}"/> so that Entity Framework LINQ queries
    /// can run against it in unit tests.
    /// </summary>
    /// <typeparam name="T">The entity type represented by the DbSet.</typeparam>
    /// <param name="data">The initial in-memory data to expose through the mock.</param>
    /// <returns>
    /// A <see cref="Mock{DbSet}"/> that behaves like a real EF Core DbSet
    /// for read-only LINQ operations.
    /// </returns>
    /// <remarks>
    /// This mock supports LINQ operations (Where, Select, First, etc.)
    /// but does not simulate EF change tracking or database behavior.
    /// Use this when you want to test service-level logic without touching
    /// a real database.
    /// </remarks>
    public static Mock<DbSet<T>> CreateDbSetMock<T>(IEnumerable<T> data) where T : class
    {
        var queryable = data.AsQueryable();

        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator()); 

        return mockSet;
    }

    /// <summary>
    /// Extension method that configures a mocked <see cref="ApplicationDbContext"/>
    /// to return a mocked <see cref="DbSet{T}"/> for a specific entity type.
    /// </summary>
    /// <typeparam name="T">The entity type represented by the DbSet.</typeparam>
    /// <param name="dbContextMock">The mock <see cref="ApplicationDbContext"/> to modify.</param>
    /// <param name="data">The in-memory data used to populate the mocked DbSet.</param>
    /// <returns>
    /// The same <see cref="Mock{ApplicationDbContext}"/> instance, configured so that
    /// the appropriate DbSet property returns a test-friendly mocked DbSet.
    /// </returns>
    /// <remarks>
    /// This helper method ensures your services receive realistic DbSet behavior
    /// when they call <c>context.Files</c> (or other DbSets you add later).
    /// Extend the <c>if</c> block to support additional DB sets as your project grows.
    /// </remarks>
    public static Mock<ApplicationDbContext> ReturnsDbSet<T>(this Mock<ApplicationDbContext> dbContextMock, IEnumerable<T> data) where T : class
    {
        if(typeof(T) == typeof(Models.FileRecord))
        {
            var mockset = CreateDbSetMock(data.Cast<Models.FileRecord>());
            dbContextMock.Setup(c => c.Files).Returns(mockset.Object);
        }

        return dbContextMock;
    }
}