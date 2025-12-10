using System.Threading.Tasks;
using FileIntake.Models;
using Microsoft.AspNetCore.Http;

namespace FileIntake.Interfaces;

public interface IFileProcessingService
{
    Task<FileProcessingResult> ProcessFile(IFormFile file, int id);
}