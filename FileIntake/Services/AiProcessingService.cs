using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FileIntake.Interfaces;
using FileIntake.Models.Configuration;
using FileIntake.Models.DTO;
using FileIntake.Models.Enums;
using FileIntake.Models.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FileIntake.Services;

public class AiProcessingService : IAiProcessingService
{
    private readonly HttpClient _httpClient;

    public AiProcessingService(HttpClient httpClient, IOptions<AiServiceOptions> options)
    {
        _httpClient = httpClient;
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new InvalidOperationException("AiProcessingService: BaseUrl is not configured");
        }
        _httpClient.BaseAddress = new Uri(baseUrl!);
    }

    public async Task<AiProcessingResult> AiProcessAsync(string text, AiQueryType query)
    {
        Console.WriteLine("Inside AiProcessingService AiProcessAsync method");
        if(text == null || text.IsNullOrEmpty())
        {
            return new AiProcessingResult
            {
                success = false,
                ErrorMessage = "Invalid or empty prompt, please enter another"
            };
        }

        if(!Enum.IsDefined(typeof(AiQueryType),query))
        {
            return new AiProcessingResult
            {
                success = false,
                ErrorMessage = "Invalid or empty query, please select a query"
            };
        }

        var finalizedPrompt = $"{query.GetPrompt()}\n\n{text}";

        var requestDto = new AiRequestDto
        {
            Text = finalizedPrompt
        };

        try
        {
            Console.WriteLine("Inside AiProcessingService AiProcessAsync method - TRY");
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonContent = JsonSerializer.Serialize(requestDto, options);
            var request = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(GetEndpointFromQuery(query), request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Inside AiProcessingService AiProcessAsync method - TRY - FAILED");
                return new AiProcessingResult
                {
                    success = false,
                    ErrorMessage = "Ai service returned a failed status of: "+ response.StatusCode
                };
            }

            var returnedJson = await response.Content.ReadAsStringAsync();
            var aiResponse = JsonSerializer.Deserialize<AiResponseDto>(returnedJson, options);
            Console.WriteLine("Response From Python Service: "+ aiResponse.Summary);

            if (string.IsNullOrWhiteSpace(aiResponse.Summary))
            {
                return new AiProcessingResult
                {
                    success = false,
                    ErrorMessage = "AI response was empty or invalid."
                };
            }

            return new AiProcessingResult
            {
                success = true,
                aiResponse = aiResponse.Summary
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

    private string GetEndpointFromQuery(AiQueryType queryType)
    {
        return queryType switch
        {
            AiQueryType.Summarize => "/summarize",
            AiQueryType.ELI5 => "/eli5",
            AiQueryType.PointForm => "/pointform",
            _ => throw new ArgumentOutOfRangeException( nameof(queryType), queryType, 
            "Unhandled AiQueryType. Value is invalid or needs to be mapped")
        };
    }
}