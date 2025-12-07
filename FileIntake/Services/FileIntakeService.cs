using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileIntake.Data;
using FileIntake.Interfaces;
using FileIntake.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileIntake.Services
{
    public class FileIntakeService : IFileIntakeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FileIntakeService> _logger;
        public FileIntakeService(ApplicationDbContext context, ILogger<FileIntakeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<FileRecord>> GetRecentFilesAsync(int count, string sortOrder)
        {
            var query = _context.Files
                .Include(f => f.UserProfile)
                .AsQueryable();

            query = ApplySorting(query, sortOrder);
            _logger.LogInformation("Fetching {Count} recent files sorted by {SortOrder}", count, sortOrder);

            return await query.Take(count).ToListAsync();
        }

        public async Task<FileRecord?> GetFileByIdAsync(int id)
        {
            _logger.LogInformation("Fetching file with ID {FileId}", id);
            return await _context.Files
                .Include(f => f.UserProfile)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddFileAsync(FileRecord file)
        {
            _logger.LogInformation("Adding new file: {FileName}", file.FileName);
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Applies server-side sorting based on the provided sortOrder key.
        /// </summary>
        /// <param name="query">The file metadata query.</param>
        /// <param name="sortOrder">The requested sort order.</param>
        /// <returns>A sorted IQueryable of FileRecord.</returns>
        private IQueryable<FileRecord> ApplySorting(IQueryable<FileRecord> query, string sortOrder)
        {
            switch (sortOrder)
            {
                case "name_desc":
                    return query.OrderByDescending(f => f.FileName);
                case "Date":
                    return query.OrderBy(f => f.UploadedAt);
                case "date_desc":
                    return query.OrderByDescending(f => f.UploadedAt);
                case "Uploader":
                    return query.OrderBy(f => f.UserProfile!.FirstName);
                case "uploader_desc":
                    return query.OrderByDescending(f => f.UserProfile!.FirstName);
                default:
                    return query.OrderBy(f => f.FileName);
            }
        }
    }
}