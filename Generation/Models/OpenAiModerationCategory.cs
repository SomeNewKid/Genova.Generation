// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Generation.Models;

/// <summary>
/// Represents a single moderation category entry (e.g., "violence/graphic").
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by modules and websites.")]
public sealed class OpenAiModerationCategory
{
    /// <summary>
    /// Gets or sets the category name, e.g. "violence/graphic".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the category was flagged (boolean in the Moderation API response).
    /// </summary>
    public bool Flag { get; set; }

    /// <summary>
    /// Gets or sets the numeric score for the category (0..1). If the score was absent, this will be 0.
    /// </summary>
    public double Score { get; set; }
}
