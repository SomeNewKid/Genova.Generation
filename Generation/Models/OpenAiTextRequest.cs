// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Generation.Models;

/// <summary>
/// Represents a text generation request to the OpenAI API.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by modules and websites.")]
public sealed class OpenAiTextRequest : IOpenAiRequest
{
    /// <inheritdoc/>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the context provided for the GPT text request.
    /// </summary>
    /// <remarks>Maps to the `system` role.</remarks>
    public string Context { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string Prompt { get; set; } = string.Empty;
}
