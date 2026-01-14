using System.Net;
using System.Text;
using Moq;
using Moq.Protected;

// HttpClient is just a wrapper, HttpMessageHandler is what does the real work of sending requests
// This class is to fake the handler so I an intercept requests, inspect the request body/URL/Headers
// I can return whatever response I need and throw exceptions.
public static class HttpClientMockFactory
{
    public static HttpClient Create(HttpStatusCode statusCode, string responseBody, Action<HttpRequestMessage>? assertRequest = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
               assertRequest?.Invoke(request);

               return new HttpResponseMessage
               {
                   StatusCode = statusCode,
                   Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
               };
            });

        return new HttpClient(handlerMock.Object);
    }

    public static HttpClient CreateThrowing(Exception ex)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(ex);

        return new HttpClient(handlerMock.Object);
    }
}