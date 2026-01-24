using System.Net;
using System.Text.Json;
using FileIntake.Models.Configuration;
using FileIntake.Models.Enums;
using FileIntake.Services;
using Microsoft.Extensions.Options;

namespace FileIntake.Tests;

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

        var options = Options.Create(new AiServiceOptions { BaseUrl = "http://localhost:8000"});     
        var service = new AiProcessingService(httpClient, options);

        // Act
        var result = await service.AiProcessAsync("my pdf text", AiQueryType.Summarize);

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
        
        var options = Options.Create(new AiServiceOptions { BaseUrl = "http://localhost:8000"}); 
        var service = new AiProcessingService(httpClient, options);

        // Act
        var result = await service.AiProcessAsync("", AiQueryType.Summarize);

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
        
        var options = Options.Create(new AiServiceOptions { BaseUrl = "http://localhost:8000"}); 
        var service = new AiProcessingService(httpClient, options);

        // Act
        var result = await service.AiProcessAsync(null!, AiQueryType.Summarize);

        // Assert
        Assert.False(result.success, result.ErrorMessage);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal("Invalid or empty prompt, please enter another", result.ErrorMessage);
    }

    [Fact]
    public async Task AiProcessingService_AiProcessAsync_InvalidQueryType_ReturnsFailure_DoesNotCallHTTP()
    {
        // Arrange
        var httpClient = HttpClientMockFactory.CreateThrowing(
            new Exception("HTTP should not be called for invalid input")
        );
        
        var options = Options.Create(new AiServiceOptions { BaseUrl = "http://localhost:8000"}); 
        var service = new AiProcessingService(httpClient, options);

        // Act
        var result = await service.AiProcessAsync("my pdf text", (AiQueryType)999);

        // Assert
        Assert.False(result.success, result.ErrorMessage);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal("Invalid or empty query, please select a query", result.ErrorMessage);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task AiProcessingService_AiProcessAsync_ResponseFailureCode_ReturnsFailure(HttpStatusCode inputStatusCode)
    {
        // Arrange
        var responseJson = "{\"summary\":\"hello from ai\"}";

        var httpClient = HttpClientMockFactory.Create(
            statusCode: inputStatusCode,
            responseBody: responseJson);
        
        var options = Options.Create(new AiServiceOptions { BaseUrl = "http://localhost:8000"}); 
        var service = new AiProcessingService(httpClient, options);

        // Act
        var result = await service.AiProcessAsync("my pdf text", AiQueryType.Summarize);

        // Assert
        Assert.False(result.success, result.ErrorMessage);
        Assert.Equal($"Ai service returned a failed status of: {inputStatusCode}", result.ErrorMessage);
    }

    [Fact]
    public async Task AiProcessingService_AiProcessAsync_AiResponseSummary_EmptyResponseJson()
    {
        // Arrange
        var responseJson = "{}";

        var httpClient = HttpClientMockFactory.Create(
            statusCode: HttpStatusCode.OK,
            responseBody: responseJson);
        
        var options = Options.Create(new AiServiceOptions { BaseUrl = "http://localhost:8000"});
        var service = new AiProcessingService(httpClient, options);

        // Act
        var result = await service.AiProcessAsync("my pdf text", AiQueryType.Summarize);

        // Assert
        Assert.False(result.success, result.ErrorMessage);
        Assert.Equal("AI response was empty or invalid.", result.ErrorMessage);
    }

    [Fact]
    public async Task AiProcessingService_AiProcessAsync_AiResponseSummary_MalformedJson()
    {
        // Arrange
        var responseJson = "{not valid json";

        var httpClient = HttpClientMockFactory.Create(
            statusCode: HttpStatusCode.OK,
            responseBody: responseJson);
        var options = Options.Create(new AiServiceOptions { BaseUrl = "http://localhost:8000"});
        var service = new AiProcessingService(httpClient, options);

        // Act
        var result = await service.AiProcessAsync("my pdf text", AiQueryType.Summarize);

        // Assert
        Assert.False(result.success, result.ErrorMessage);
        Assert.Contains("Connecting to ai service failed ", result.ErrorMessage);
    }

    [Fact]
    public async Task AiProcessingService_AiProcessAsync_AiResponseSummary_IsNullOrWhiteSpaceCondition()
    {
        // Arrange
        var responseJson = "{\"summary\": \"\"}";

        var httpClient = HttpClientMockFactory.Create(
            statusCode: HttpStatusCode.OK,
            responseBody: responseJson);

        var options = Options.Create(new AiServiceOptions { BaseUrl = "http://localhost:8000"});   
        var service = new AiProcessingService(httpClient, options);

        // Act
        var result = await service.AiProcessAsync("my pdf text", AiQueryType.Summarize);

        // Assert
        Assert.False(result.success, result.ErrorMessage);
        Assert.Equal("AI response was empty or invalid.", result.ErrorMessage);
    }

    [Fact]
    public async Task AiProcessingService_AiProcessAsync_AiResponseSummary_JsonExplicitNull()
    {
        // Arrange
        var responseJson = "{\"summary\": null }";

        var httpClient = HttpClientMockFactory.Create(
            statusCode: HttpStatusCode.OK,
            responseBody: responseJson);
        
        var options = Options.Create(new AiServiceOptions { BaseUrl = "http://localhost:8000"});   
        var service = new AiProcessingService(httpClient, options);

        // Act
        var result = await service.AiProcessAsync("my pdf text", AiQueryType.Summarize);

        // Assert
        Assert.False(result.success, result.ErrorMessage);
        Assert.Equal("AI response was empty or invalid.", result.ErrorMessage);
    }

    [Fact]
    public async Task AiProcessingService_AiProcessAsync_TaskCanceled_ReturnsFailure()
    {
        // Arrange
        var httpClient = HttpClientMockFactory.CreateThrowing(new TaskCanceledException("Request timed out"));
        
        var options = Options.Create(new AiServiceOptions { BaseUrl = "http://localhost:8000"}); 
        var service = new AiProcessingService(httpClient, options);

        // Act
        var result = await service.AiProcessAsync("my pdf text", AiQueryType.Summarize);

        // Assert
        Assert.False(result.success, result.ErrorMessage);
        Assert.Contains("Connecting to ai service failed ", result.ErrorMessage);
    }

    [Fact]
    public async Task AiProcessingService_AiProcessAsync_PostAsyncException_Failure()
    {
        // Arrange
        var httpClient = HttpClientMockFactory.CreateThrowing(new HttpRequestException("Blow Up"));
        
        var options = Options.Create(new AiServiceOptions { BaseUrl = "http://localhost:8000"}); 
        var service = new AiProcessingService(httpClient, options);

        // Act
        var result = await service.AiProcessAsync("my pdf text", AiQueryType.Summarize);

        // Assert
        Assert.False(result.success, result.ErrorMessage);
        Assert.Contains("Connecting to ai service failed System.Net.Http.HttpRequestException: Blow Up", result.ErrorMessage);
        Assert.False(result.IsResponseUpdated);
    }
}