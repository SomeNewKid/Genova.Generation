// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.Generation.Models;

/// <summary>
/// Represents the common properties for OpenAI API requests.
/// </summary>
public interface IOpenAiRequest
{
    /// <summary>
    /// Gets or sets the model to be used for the GPT request.
    /// </summary>
    string? Model { get; set; }

    /// <summary>
    /// Gets or sets the prompt provided for the GPT request.
    /// </summary>
    string Prompt { get; set; }
}
