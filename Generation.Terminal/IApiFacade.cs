// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.Generation.Terminal;

/// <summary>
/// Defines a facade for processing user input with an OpenAI API endpoint.
/// Implementations should handle the request, response, and output logic.
/// </summary>
public interface IApiFacade
{
    /// <summary>
    /// Processes the specified input by sending it to the configured OpenAI API endpoint.
    /// The implementation is responsible for handling the response and any output (e.g., writing to the console).
    /// </summary>
    /// <param name="input">The user input to process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProcessAsync(string input);
}
