using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileIntake.Data;
using FileIntake.Models;
using Microsoft.EntityFrameworkCore;

namespace FileIntake.Services
{
    public class FileIntakeService
    {
        private readonly ApplicationDbContext _context;
        public FileIntakeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FileRecord>> GetRecentFilesAsync(int count = 5)
        {
            return await _context.Files
                .Include(f => f.UserProfile)
                .OrderByDescending(f => f.UploadedAt)
                .Take(count)
                .ToListAsync();
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
    }
}