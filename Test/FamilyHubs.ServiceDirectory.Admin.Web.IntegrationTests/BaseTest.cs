using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FamilyHubs.ServiceDirectory.Admin.Web.IntegrationTests;

public abstract class BaseTest : IDisposable, IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly Random Random = new();

    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    protected BaseTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        {"StubAuthentication:UseStubAuthentication", "true"}
                    });
                });

                builder.ConfigureTestServices(Configure);
            }
        ).CreateClient(
            new WebApplicationFactoryClientOptions {
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true
            }
        );
    }

    protected abstract void Configure(IServiceCollection services);

    protected async Task Login(StubUser user)
    {
        await _client.GetAsync($"account/stub/roleSelected?user={user.Email}&redirect=%2f");
    }

    protected async Task<IHtmlDocument> Navigate(string uri, Action<HttpResponseMessage>? responseValidation = null)
    {
        var response = await _client.GetAsync(uri);
        responseValidation?.Invoke(response);
        return await GetDocumentAsync(response);
    }

    private static async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var document = await BrowsingContext.New(Configuration.Default.WithCss()).OpenAsync(ResponseFactory, CancellationToken.None);
        return (IHtmlDocument) document;

        void ResponseFactory(VirtualResponse htmlResponse)
        {
            htmlResponse
                .Address(response.RequestMessage.RequestUri)
                .Status(response.StatusCode);

            MapHeaders(response.Headers);
            MapHeaders(response.Content.Headers);

            htmlResponse.Content(content);

            void MapHeaders(HttpHeaders headers)
            {
                foreach (var header in headers)
                {
                    foreach (var value in header.Value)
                    {
                        htmlResponse.Header(header.Key, value);
                    }
                }
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        // Cleanup
    }

    public void Dispose()
    {
        Dispose(true);

        _client.Dispose();
        _factory.Dispose();

        GC.SuppressFinalize(this);
    }

    protected class StubUser
    {
        public string Email { get; }

        private StubUser(string email)
        {
            Email = email;
        }

        public static readonly StubUser DfeAdmin = new("dfeAdmin.user@stub.com");
    }
}
