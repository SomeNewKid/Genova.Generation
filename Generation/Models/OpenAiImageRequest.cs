// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Generation.Models;

/// <summary>
/// Represents an image generation request to the OpenAI API.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by modules and websites.")]
public sealed class OpenAiImageRequest : IOpenAiRequest
{
    /// <inheritdoc/>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the number of images to generate.
    /// </summary>
    public int Number { get; set; } = 1;

    /// <summary>
    /// Gets or sets the size of the images for the GPT to generate.
    /// </summary>
    public string Size { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string Prompt { get; set; } = string.Empty;
}
