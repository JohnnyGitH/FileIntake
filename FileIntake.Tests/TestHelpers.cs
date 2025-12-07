using Microsoft.AspNetCore.Http;
using Moq;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

/// <summary>
/// Provides helper utilities for creating mock PDF files and test PDF content
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a Mock <see cref="IFormFile"/> simulating a PDF upload. This helper is 
    /// used to test controllers that simulate file uploads.
    /// </summary>
    /// <param name="fileName">Name of the Mocked PDF file to be simulated</param>
    /// <param name="isEmpty">
    /// Parameter to emulate an empty PDF file if true, otherwise
    /// if it's false, it will create a proper PDF.
    /// </param>
    /// <returns>A Mocked <see cref="IFormFile"/> that has been set up to emulate user file upload in tests</returns>
    public static IFormFile CreateMockPDFFile( string fileName, bool isEmpty = false)
    {
        // isEmpty Param, if true returns empty byte array, else it returns a minimal PDF
        byte [] pdfBytes = isEmpty ? Array.Empty<byte>() : PdfTestHelper.CreateMinimalPdf();

        var ms = new MemoryStream(pdfBytes);
        var mockFile = new Mock<IFormFile>();

        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(pdfBytes.Length);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        // Let controller code to read the stream
        mockFile.Setup(f => f.OpenReadStream()).Returns(ms);

        // CopyToAsync(), used by ASP.NET Core when saving files
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream target, CancellationToken token) =>
            {
                ms.Position = 0;
                return ms.CopyToAsync(target, token);
            });

        return mockFile.Object;
    }

    /// <summary>
    /// Provides helpers for creating minimal, valid PDF files for testing purposes
    /// </summary>
    public static class PdfTestHelper
    {
        /// <summary>
        /// Builds minimal, but valid PDF files in memory
        /// Ensuring that PDF parsing library is behaving as expected in tests.
        /// </summary>
        /// <returns>Byte array with a valid single page PDF document</returns>
        public static byte[] CreateMinimalPdf()
        {
            using var ms = new MemoryStream();
            var builder = new PdfDocumentBuilder(ms);

            var page = builder.AddPage(595, 842); // A4 standard size

            // Writes a simple PDF text using size 14 font (very customizable)
            page.AddText("Test PDF Content", 12, new PdfPoint(){}, builder.AddStandard14Font(UglyToad.PdfPig.Fonts.Standard14Fonts.Standard14Font.Courier));

            builder.Build();

            return ms.ToArray();
        }

        /// <summary>
        ///  Generates a minimal PDF on disk at the declared path.
        ///  Used for tests needing physical files rather and an in-memory stream
        /// </summary>
        /// <param name="path">Relative or absolute path where the PDF will be written</param>
        /// <returns>Returns the path as a string</returns>
        public static string SaveMinimalPdfTo(string path)
        {
            var bytes = CreateMinimalPdf();
            File.WriteAllBytes(path, bytes);

            return path;
        }
    }
}