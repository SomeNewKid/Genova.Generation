// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using FluentAssertions;
using Genova.Generation.Gateways;

namespace Genova.Generation.UnitTests.Gateways;

public class OpenAiApiGateway_Tests
{
    [Fact]
    public void Constructor_should_throw_when_options_is_null()
    {
        // Arrange
        GenerationOptions? options = null;
        IHttpClientFactory? httpClientFactory = new TestHttpClientFactory();

        // Act
        Action act = () => _ = new OpenAiApiGateway(options!, httpClientFactory);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Constructor_should_throw_when_httpClientFactory_is_null()
    {
        // Arrange
        GenerationOptions options = new ()
        {
            OpenAiApiKey = "some-api-key"
        };
        IHttpClientFactory? httpClientFactory = null;

        // Act
        Action act = () => _ = new OpenAiApiGateway(options, httpClientFactory!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClientFactory");
    }

    [Fact]
    public void Constructor_should_succeed_with_valid_parameters()
    {
        // Arrange
        GenerationOptions options = new ()
        {
            OpenAiApiKey = "some-api-key"
        };
        IHttpClientFactory httpClientFactory = new TestHttpClientFactory();

        // Act
        OpenAiApiGateway helper = new (options, httpClientFactory);

        // Assert
        helper.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTextResponseAsync_should_throw_when_request_is_null()
    {
        // Arrange
        GenerationOptions options = new ()
        {
            OpenAiApiKey = "some-api-key"
        };
        IHttpClientFactory httpClientFactory = new TestHttpClientFactory();
        OpenAiApiGateway helper = new (options, httpClientFactory);

        // Act
        Func<Task> act = async () => _ = await helper.GetTextResponseAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task GetImageResponseAsync_should_throw_when_request_is_null()
    {
        // Arrange
        GenerationOptions options = new ()
        {
            OpenAiApiKey = "some-api-key"
        };
        IHttpClientFactory httpClientFactory = new TestHttpClientFactory();
        OpenAiApiGateway helper = new (options, httpClientFactory);

        // Act
        Func<Task> act = async () => _ = await helper.GetImageResponseAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new ();
    }
}
