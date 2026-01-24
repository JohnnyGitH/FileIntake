using System.Threading.Tasks;
using FileIntake.Models.Enums;

namespace FileIntake.Interfaces;

public interface IAiProcessingService
{
    Task<AiProcessingResult> AiProcessAsync(string prompt, AiQueryType query);
}