using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;

public static class TestHelpers
{
    public static IFormFile CreateMockFile( string fileName, string content = "Test Content")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        var mockFile = new Mock<IFormFile>();
        
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(stream.Length);

        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        return mockFile.Object;
    }
}