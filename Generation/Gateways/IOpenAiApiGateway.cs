// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Generation.Models;

namespace Genova.Generation.Gateways;

/// <summary>
/// Provides an interface for sending requests to the OpenAI API.
/// </summary>
public interface IOpenAiApiGateway
{
    /// <summary>
    /// Sends a text generation request to the OpenAI API and returns the response.
    /// </summary>
    /// <param name="request">The text generation request.</param>
    /// <returns>The response to the text generation request.</returns>
    Task<OpenAiTextResponse> GetTextResponseAsync(OpenAiTextRequest request);

    /// <summary>
    /// Sends an image generation request to the OpenAI API and returns the response.
    /// </summary>
    /// <param name="request">The image generation request.</param>
    /// <returns>The response to the image generation request.</returns>
    Task<OpenAiImageResponse> GetImageResponseAsync(OpenAiImageRequest request);

    /// <summary>
    /// Sends a moderation request to the OpenAI API and returns the response.
    /// </summary>
    /// <param name="request">The moderation request.</param>
    /// <returns>The response to the moderation request.</returns>
    Task<OpenAiModerationResponse> GetModerationResponseAsync(OpenAiModerationRequest request);
}
