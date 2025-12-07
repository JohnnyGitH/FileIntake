using Microsoft.AspNetCore.Http;
using Moq;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

public static class TestHelpers
{
    public static IFormFile CreateMockPDFFile( string fileName, bool isEmpty = false)
    {

        byte [] pdfBytes = isEmpty ? Array.Empty<byte>() : PdfTestHelper.CreateMinimalPdf();

        var ms = new MemoryStream(pdfBytes);
        var mockFile = new Mock<IFormFile>();

        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(pdfBytes.Length);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        // Need to mock the stream returning something
        mockFile.Setup(f => f.OpenReadStream()).Returns(ms);

        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream target, CancellationToken token) =>
            {
                ms.Position = 0;
                return ms.CopyToAsync(target, token);
            });
        return mockFile.Object;
    }

    public static class PdfTestHelper
    {
        public static byte[] CreateMinimalPdf()
        {
            using var ms = new MemoryStream();

            var builder = new PdfDocumentBuilder(ms);

            var page = builder.AddPage(595, 842); // A4 size

            page.AddText("Test PDF Content", 12, new PdfPoint(){}, builder.AddStandard14Font(UglyToad.PdfPig.Fonts.Standard14Fonts.Standard14Font.Courier));

            builder.Build();

            return ms.ToArray();
        }

        public static string SaveMinimalPdfTo(string path)
        {
            var bytes = CreateMinimalPdf();
            //Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
            return path;
        }
    }
}