// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.Generation.Models;

/// <summary>
/// Represents the common properties for OpenAI API responses.
/// </summary>
public interface IOpenAiResponse
{
    /// <summary>
    /// Gets or sets the name of the GPT model.
    /// </summary>
    string Model { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the GPT response was successful.
    /// </summary>
    bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    string Error { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the rate limits for the OpenAI API response.
    /// </summary>
    OpenAIRateLimits RateLimits { get; set; }
}
