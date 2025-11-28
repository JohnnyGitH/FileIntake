using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;

public static class TestHelpers
{
    public static IFormFile CreateMockFile( string fileName, bool isEmpty = false)
    {
        var bytes = isEmpty ? Encoding.UTF8.GetBytes("") : Encoding.UTF8.GetBytes("Test content");
        var stream = new MemoryStream(bytes);

        var mockFile = new Mock<IFormFile>();
        
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(stream.Length);

        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        return mockFile.Object;
    }
}