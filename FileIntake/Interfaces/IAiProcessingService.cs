using System.Threading.Tasks;

namespace FileIntake.Interfaces;

public interface IAiProcessingService
{
    Task<AiProcessingResult> AiProcessAsync(string prompt, string query);
}