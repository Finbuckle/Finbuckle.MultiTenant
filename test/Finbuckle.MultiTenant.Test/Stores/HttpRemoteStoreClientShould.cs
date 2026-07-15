// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Net;
using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class HttpRemoteStoreClientShould
{
    private sealed class TestHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responseFactory;

        public TestHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responseFactory)
            : this((request, cancellationToken) => Task.FromResult(responseFactory(request, cancellationToken)))
        {
        }

        public TestHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public Uri? RequestUri { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            CancellationToken = cancellationToken;
            return _responseFactory(request, cancellationToken);
        }
    }

    private static HttpRemoteStoreClient<TenantInfo> CreateClient(TestHandler handler,
        JsonSerializerOptions? serializerOptions = null)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));
        return new HttpRemoteStoreClient<TenantInfo>(factory.Object, serializerOptions);
    }

    [Fact]
    public void ThrowIfHttpClientFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new HttpRemoteStoreClient<TenantInfo>(null!));
    }

    [Fact]
    public async Task SubstituteIdentifierAndDeserializeTenant()
    {
        var handler = new TestHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":"initech-id","identifier":"initech"}""")
        });
        var client = CreateClient(handler);

        var tenant = await client.GetByIdentifierAsync("https://example.com/{__tenant__}", "initech");

        Assert.Equal("https://example.com/initech", handler.RequestUri?.ToString());
        Assert.Equal("initech-id", tenant?.Id);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task ReturnNullForUnsuccessfulIdentifierResponse(HttpStatusCode statusCode)
    {
        var client = CreateClient(new TestHandler((_, _) => new HttpResponseMessage(statusCode)));

        Assert.Null(await client.GetByIdentifierAsync("https://example.com/{__tenant__}", "missing"));
    }

    [Fact]
    public async Task UseCustomSerializerOptions()
    {
        var handler = new TestHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"Id":"initech-id","Identifier":"initech"}""")
        });
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = null
        };
        var client = CreateClient(handler, options);

        var tenant = await client.GetByIdentifierAsync("https://example.com/{__tenant__}", "initech");

        Assert.Equal("initech-id", tenant?.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    public async Task ThrowForMalformedTenantJson(string json)
    {
        var client = CreateClient(new TestHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        }));

        await Assert.ThrowsAsync<JsonException>(() =>
            client.GetByIdentifierAsync("https://example.com/{__tenant__}", "initech"));
    }

    [Fact]
    public async Task GetAllTenantsFromCollectionEndpoint()
    {
        var handler = new TestHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""[{"id":"initech-id","identifier":"initech"}]""")
        });
        var client = CreateClient(handler);

        var tenants = await client.GetAllAsync("https://example.com/{__tenant__}");

        Assert.Equal("https://example.com/", handler.RequestUri?.ToString());
        Assert.Equal("initech", Assert.Single(tenants).Identifier);
    }

    [Fact]
    public async Task ThrowNotImplementedForMissingCollectionEndpoint()
    {
        var client = CreateClient(new TestHandler((_, _) => new HttpResponseMessage(HttpStatusCode.NotFound)));

        await Assert.ThrowsAsync<NotImplementedException>(() =>
            client.GetAllAsync("https://example.com/{__tenant__}"));
    }

    [Fact]
    public async Task ReturnEmptyCollectionForOtherCollectionErrors()
    {
        var client = CreateClient(new TestHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        Assert.Empty(await client.GetAllAsync("https://example.com/{__tenant__}"));
    }

    [Fact]
    public async Task ForwardCancellationTokenThroughStore()
    {
        var requestStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new TestHandler(async (_, cancellationToken) =>
        {
            requestStarted.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var store = new HttpRemoteStore<TenantInfo>(CreateClient(handler), "https://example.com/{__tenant__}");
        using var cancellationTokenSource = new CancellationTokenSource();

        var request = store.GetByIdentifierAsync("initech", cancellationTokenSource.Token);
        await requestStarted.Task;
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request);
        Assert.True(handler.CancellationToken.IsCancellationRequested);
    }
}
