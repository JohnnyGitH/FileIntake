using System;
using System.Net.Http;
using System.Threading.Tasks;
using FileIntake.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FileIntake.Services;

public class AiProcessingService : IAiProcessingService
{
    private const string BaseURL = "http://localhost:8000";
    private const string SUMMARIZE = "/summarize";
    private readonly HttpClient _httpClient;

    public AiProcessingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<AiProcessingResult> AiProcessAsync(string prompt, string query)
    {
        if(prompt == null || prompt.IsNullOrEmpty())
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

        // Create the full promp
        var finalizedPrompt = query + prompt;

        // Checked parameters, now its time to reach out to the service.
        try
        {
            // Logic to connect to the Ai microservice
            _httpClient.BaseAddress = new Uri(BaseURL+SUMMARIZE);


            // I need to JSON serialize the finalized prompt, newtonsoft?
            // Should this step be in its own try catch for serialization?

            // I need to add that serialized content to the request

            // See what I get back?
        }
        catch( Exception ex)
        {
            return new AiProcessingResult
            {
                success = false,
                ErrorMessage = "Connecting to ai service failed " + ex,
            };
        }

        return new AiProcessingResult
        {
            success = true,
            aiResponse = "placeholder for variable to be returned from service"
        };
    }
}