using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileIntake.Data;
using FileIntake.Interfaces;
using FileIntake.Models;
using Microsoft.EntityFrameworkCore;

namespace FileIntake.Services
{
    public class FileIntakeService : IFileIntakeService
    {
        private readonly ApplicationDbContext _context;
        public FileIntakeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FileRecord>> GetRecentFilesAsync(int count, string sortOrder = null)
        {
            var query = _context.Files
                .Include(f => f.UserProfile)
                .OrderByDescending(f => f.UploadedAt)
                .Take(count)
                .AsQueryable();

            query = ApplySorting(query, sortOrder);

            return await query.ToListAsync();
        }

        public async Task<FileRecord> GetFileByIdAsync(int id)
        {
            return await _context.Files
                .Include(f => f.UserProfile)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddFileAsync(FileRecord file)
        {
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Applies sorting to the file query based on the sortOrder parameter.
        /// </summary>
        /// <param name="query">Queried files</param>
        /// <param name="sortOrder">Request sort order</param>
        /// <returns>Sorted list of FileRecords</returns>
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
                    return query.OrderBy(f => f.UserProfile.FirstName);
                case "uploader_desc":
                    return query.OrderByDescending(f => f.UserProfile.FirstName);
                default:
                    return query.OrderBy(f => f.FileName);
            }
        }
    }
}