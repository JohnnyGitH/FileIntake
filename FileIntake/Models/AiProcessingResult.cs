namespace FileIntake;

public class AiProcessingResult
{
    public bool success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? aiResponse { get; set; }

    public bool IsResponseUpdated => aiResponse != null;
}