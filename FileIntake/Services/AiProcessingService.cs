using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FileIntake.Interfaces;
using FileIntake.Models.DTO;
using Microsoft.IdentityModel.Tokens;

namespace FileIntake.Services;

public class AiProcessingService : IAiProcessingService
{
    private const string BaseUrl = "http://localhost:8000";
    private const string Summarize_Endpoint = "/summarize";

    private readonly HttpClient _httpClient;

    public AiProcessingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<AiProcessingResult> AiProcessAsync(string text, string query)
    {
        if(text == null || text.IsNullOrEmpty())
        {
            return new AiProcessingResult
            {
                success = false,
                ErrorMessage = "Invalid or empty prompt, please enter another"
            };
        }

        if(query == null || query.IsNullOrEmpty())
        {
            return new AiProcessingResult
            {
                success = false,
                ErrorMessage = "Invalid or empty query, please select a query"
            };
        }

        var finalizedPrompt = $"{query}\n\n{text}";

        var requestDto = new AiRequestDto
        {
            Prompt = finalizedPrompt
        };

        try
        {
            var jsonContent = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(Summarize_Endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                return new AiProcessingResult
                {
                    success = false,
                    ErrorMessage = "Ai service returned a failed status of: "+ response.StatusCode
                };
            }

            var returnedJson = await response.Content.ReadAsStringAsync();
            var aiResponse = JsonSerializer.Deserialize<AiResponseDto>(returnedJson);

            return new AiProcessingResult
            {
                success = true,
                aiResponse = aiResponse?.Result
            };
        }
        catch( Exception ex)
        {
            return new AiProcessingResult
            {
                success = false,
                ErrorMessage = "Connecting to ai service failed " + ex,
            };
        }
    }
}