// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Generation;

/// <summary>
/// Represents configuration options for connecting to Generative AI endpoints.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for websites to pass options to the module.")]
public sealed class GenerationOptions
{
    /// <summary>
    /// Gets or sets the OpenAI API key used for authentication with the OpenAI service.
    /// </summary>
    public string OpenAiApiKey { get; set; } = string.Empty;
}
