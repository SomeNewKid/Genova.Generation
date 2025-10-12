// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Generation.Gateways;
using Genova.Generation.Models;

namespace Genova.Generation.Terminal;

/// <summary>
/// Facade for processing user input with the OpenAI text completion API.
/// Handles request creation, response processing, and console output.
/// </summary>
public class TextApiFacade : IApiFacade
{
    private readonly IOpenAiApiGateway _gateway;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextApiFacade"/> class.
    /// </summary>
    /// <param name="gateway">The OpenAI API gateway to use for requests.</param>
    public TextApiFacade(IOpenAiApiGateway gateway)
    {
        _gateway = gateway;
    }

    /// <inheritdoc/>
    public async Task ProcessAsync(string input)
    {
        OpenAiTextRequest request = new()
        {
            Model = "gpt-3.5-turbo",
            Context = "You are a helpful assistant.",
            Prompt = input
        };
        OpenAiTextResponse response = await _gateway.GetTextResponseAsync(request);

        if (response.Success)
        {
            Console.WriteLine(response.Content);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {response.Error} (Status: {response.StatusCode})");
            Console.ResetColor();
        }
    }
}
