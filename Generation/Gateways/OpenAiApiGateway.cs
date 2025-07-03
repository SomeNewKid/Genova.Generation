// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Genova.Common.Attributes;
using Genova.Generation;
using Genova.Generation.Models;
using Genova.Generation.Utilities;

namespace Genova.Generation.Gateways;

/// <summary>
/// Provides functionality to interact with the OpenAI API.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by modules and websites.")]
[ExcludeFromCodeCoverage(Justification = "This class is a gateway to an external API and does not require unit tests.")]
public sealed class OpenAiApiGateway : IOpenAiApiGateway
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Conflicting naming rules.")]
    private static readonly string ChatEndpoint = "https://api.openai.com/v1/chat/completions";
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Conflicting naming rules.")]
    private static readonly string ImageEndpoint = "https://api.openai.com/v1/images/generations";
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAiApiGateway"/> class.
    /// </summary>
    /// <param name="options">The application configuration.</param>
    /// <param name="httpClientFactory">The HTTP client factory used to create HTTP clients.</param>
    public OpenAiApiGateway(GenerationOptions options, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.OpenAiApiKey}");
    }

    /// <inheritdoc/>
    public async Task<OpenAiTextResponse> GetTextResponseAsync(OpenAiTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        OpenAiTextHelper openAiTextHelper = new();
        string requestBody = openAiTextHelper.Serialize(request);
        StringContent content = new(requestBody, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(ChatEndpoint, content);
        return await openAiTextHelper.Deserialize(response);
    }

    /// <inheritdoc/>
    public async Task<OpenAiImageResponse> GetImageResponseAsync(OpenAiImageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        OpenAiImageHelper openAiImageHelper = new();
        string requestBody = openAiImageHelper.Serialize(request);
        StringContent content = new(requestBody, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(ImageEndpoint, content);
        return await openAiImageHelper.Deserialize(response);
    }
}
