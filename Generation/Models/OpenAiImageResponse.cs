// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Generation.Models;

/// <summary>
/// Represents the response to an image generation request to the OpenAI API.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by modules and websites.")]
public sealed class OpenAiImageResponse : IOpenAiResponse
{
    /// <inheritdoc/>
    public string Model { get; set; } = string.Empty;

    /// <inheritdoc/>
    public bool Success { get; set; } = true;

    /// <inheritdoc/>
    public string Error { get; set; } = string.Empty;

    /// <inheritdoc/>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// Gets or sets the URL of the generated image.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the byte array of the generated image.
    /// </summary>
    public byte[] Bytes { get; set; } = [];

    /// <summary>
    /// Gets or sets the file format or extension of the generated image (e.g., "png").
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <inheritdoc/>
    public OpenAIRateLimits RateLimits { get; set; } = new();
}
