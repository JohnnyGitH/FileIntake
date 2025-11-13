using System.Collections.Generic;
using System.Threading.Tasks;
using FileIntake.Models;

namespace FileIntake.Interfaces;

public interface IFileIntakeService
{
    Task<List<FileRecord>> GetRecentFilesAsync(int count, string sortOrder);
    Task<FileRecord> GetFileByIdAsync(int id);
    Task AddFileAsync(FileRecord file);
}