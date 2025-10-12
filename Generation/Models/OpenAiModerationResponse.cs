// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Generation.Models;

/// <summary>
/// Represents the response to a moderation request to the OpenAI Moderation API.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by modules and websites.")]
public sealed class OpenAiModerationResponse : IOpenAiResponse
{
    /// <inheritdoc/>
    public string Model { get; set; } = string.Empty;

    /// <inheritdoc/>
    public bool Success { get; set; }

    /// <inheritdoc/>
    public string Error { get; set; } = string.Empty;

    /// <inheritdoc/>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// Gets or sets a value indicating whether the input was flagged by the moderation model
    /// (derived from results[0].flagged).
    /// </summary>
    public bool Flagged { get; set; }

    /// <summary>
    /// Gets or sets the list of categories from <c>results[0].categories</c> combined with
    /// their corresponding scores from <c>results[0].category_scores</c>.
    /// </summary>
    public List<OpenAiModerationCategory> Categories { get; set; } = [];

    /// <inheritdoc/>
    public OpenAIRateLimits RateLimits { get; set; } = new();
}
