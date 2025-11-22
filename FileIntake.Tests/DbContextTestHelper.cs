using Microsoft.EntityFrameworkCore;
using Moq;

namespace FileIntake.Tests;

public static class DbContextTestHelper
{
    public static Mock<DbSet<T>> CreateDbSetMock<T>()
}