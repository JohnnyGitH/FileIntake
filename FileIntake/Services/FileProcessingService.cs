using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileIntake.Exceptions;
using FileIntake.Interfaces;
using FileIntake.Models;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace FileIntake;

public class FileProcessingService : IFileProcessingService
{
    private readonly IFileIntakeService _fileIntakeService;
    
    public FileProcessingService(IFileIntakeService fileIntakeService)
    {
        _fileIntakeService = fileIntakeService;
    }

    /// <summary>
    ///  Primary processing method, Takes users uploaded file, creates FileRecord
    ///  object, adds it to the database
    /// </summary>
    /// <param name="file">Uploaded <see cref="IFormFile"/> from the user</param>
    /// <param name="id">UserProfile id to add to the created FileRecord</param>
    /// <returns></returns>
    public async Task<FileProcessingResult> ProcessFile(IFormFile file, int id)
    {
        if(file == null || file.Length == 0)
        {
            return new FileProcessingResult
                {
                    success = false,
                    ErrorMessage = "No file selected for upload."
                };
        }

        var fileBytes = await GetByteArrayFromIFormFile(file);
        string extractedText = "";

        Console.WriteLine($"First 10 bytes: {BitConverter.ToString(fileBytes.Take(10).ToArray())}");

        try
        {
            using (PdfDocument document = PdfDocument.Open(fileBytes))
            {
                foreach (Page page in document.GetPages())
                {
                    extractedText += page.Text + "\n\n";
                    Console.WriteLine($"Document info: {extractedText}");
                }
            }
        }
        catch (FileProcessingException ex)
        {
            return new FileProcessingResult
            {
                success = false,
                ErrorMessage = $"Error uploading file: {ex.Message}",
                SavedToDatabase = false
            };
        }

        var fileRecord = new FileRecord{
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedAt = DateTime.UtcNow,
            UserProfileId = id,
            FileText = extractedText,
        };

        try
        {
            Console.WriteLine($"Starting file upload: {file.FileName}, Size: {file.Length} bytes");
            await _fileIntakeService.AddFileAsync(fileRecord);

            return new FileProcessingResult
                {
                    success = true,
                    FileRecord = fileRecord,
                    SavedToDatabase = true
                };
        } 
        catch (Exception ex)
        {
            return new FileProcessingResult
            {
                success = false,
                FileRecord = fileRecord,
                ErrorMessage = $"Error uploading file: {ex.Message}",
                SavedToDatabase = false
            };
        }
    }

    /// <summary>
    /// Converts an <see cref="IFormFile"/> into a byte array
    /// </summary>
    /// <param name="file"><see cref="IFormFile"/> to be converted to the byte array</param>
    /// <returns>A byte array from the input <see cref="IFormFile"/></returns>
    private async Task<byte[]> GetByteArrayFromIFormFile(IFormFile file)
    {
        if(file == null || file.Length == 0)
        {
            return Array.Empty<byte>();
        }

        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);

            return memoryStream.ToArray();
        }
    }
}