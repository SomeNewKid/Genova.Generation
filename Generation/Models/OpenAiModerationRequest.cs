// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Generation.Models;

/// <summary>
/// Represents a moderation request to the OpenAI Moderation API.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by modules and websites.")]
public sealed class OpenAiModerationRequest : IOpenAiRequest
{
    /// <inheritdoc/>
    public string? Model { get; set; }

    /// <inheritdoc/>
    public string Prompt { get; set; } = string.Empty; // Maps to the Moderation API's "input"
}
