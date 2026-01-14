using System.Net;
using System.Text.Json;
using FileIntake.Services;

namespace FileIntake.Test;

public class AiProcessingServiceTests
{
    [Fact]
    public async Task AiProcessingService_AiProcessAsync_PostRequestHappyPath()
    {
        // Arrange
        var responseJson = "{\"summary\":\"hello from ai\"}";

        var httpClient = HttpClientMockFactory.Create(
            statusCode: HttpStatusCode.OK,
            responseBody: responseJson,
            assertRequest: request =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Equal("http://localhost:8000/summarize", request.RequestUri!.ToString());
                
                var body = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                using var doc = JsonDocument.Parse(body);

                Assert.True(doc.RootElement.TryGetProperty("text",out var textProp));
                var sentText = textProp.GetString();

                Assert.NotNull(sentText);
                Assert.Contains("Summarize", sentText!);
                Assert.Contains("my pdf text", sentText!);
            });
            
        var service = new AiProcessingService(httpClient);

        // Act
        var result = await service.AiProcessAsync("my pdf text", "Summarize");

        // Assert
        Assert.True(result.success, result.ErrorMessage);
        Assert.Equal("hello from ai", result.aiResponse);
        Assert.True(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public async Task AiProcessingService_AiProcessAsync_EmptyText_ReturnsFailure_DoesNotCallHTTP()
    {
        // Arrange
        var httpClient = HttpClientMockFactory.CreateThrowing(
            new Exception("HTTP should not be called for invalid input")
        );
            
        var service = new AiProcessingService(httpClient);

        // Act
        var result = await service.AiProcessAsync("", "Summarize");

        // Assert
        Assert.False(result.success, result.ErrorMessage);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal("Invalid or empty prompt, please enter another", result.ErrorMessage);
    }

    [Fact]
    public async Task AiProcessingService_AiProcessAsync_NullText_ReturnsFailure_DoesNotCallHTTP()
    {
        // Arrange
        var httpClient = HttpClientMockFactory.CreateThrowing(
            new Exception("HTTP should not be called for invalid input")
        );
            
        var service = new AiProcessingService(httpClient);

        // Act
        var result = await service.AiProcessAsync(null!, "Summarize");

        // Assert
        Assert.False(result.success, result.ErrorMessage);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal("Invalid or empty prompt, please enter another", result.ErrorMessage);
    }
}