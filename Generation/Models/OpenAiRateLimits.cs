// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Generation.Models;

/// <summary>
/// Represents the rate limit metadata returned by the OpenAI API response headers.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by modules and websites.")]
public sealed class OpenAIRateLimits
{
    /// <summary>
    /// Gets or sets the maximum number of requests allowed in the current rate limit window.
    /// Corresponds to the 'X-RateLimit-Limit-Requests' HTTP header.
    /// </summary>
    public int LimitRequests { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of tokens allowed in the current rate limit window.
    /// Corresponds to the 'X-RateLimit-Limit-Tokens' HTTP header.
    /// </summary>
    public int LimitTokens { get; set; }

    /// <summary>
    /// Gets or sets the number of remaining requests that can be made in the current window.
    /// Corresponds to the 'X-RateLimit-Remaining-Requests' HTTP header.
    /// </summary>
    public int RemainingRequests { get; set; }

    /// <summary>
    /// Gets or sets the number of remaining tokens that can be used in the current window.
    /// Corresponds to the 'X-RateLimit-Remaining-Tokens' HTTP header.
    /// </summary>
    public int RemainingTokens { get; set; }

    /// <summary>
    /// Gets or sets the time at which the request rate limit will reset.
    /// Corresponds to the 'X-RateLimit-Reset-Requests' HTTP header (as a Unix timestamp).
    /// </summary>
    public DateTime ResetRequests { get; set; }

    /// <summary>
    /// Gets or sets the time at which the token rate limit will reset.
    /// Corresponds to the 'X-RateLimit-Reset-Tokens' HTTP header (as a Unix timestamp).
    /// </summary>
    public DateTime ResetTokens { get; set; }
}
