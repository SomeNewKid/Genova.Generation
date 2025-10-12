// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Generation.Gateways;
using Genova.Generation.Models;

namespace Genova.Generation.Terminal;

/// <summary>
/// Facade for processing user input with the OpenAI moderation API.
/// Handles request creation, response processing, and console output for moderation results.
/// </summary>
public class ModerationApiFacade : IApiFacade
{
    private readonly IOpenAiApiGateway _gateway;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModerationApiFacade"/> class.
    /// </summary>
    /// <param name="gateway">The OpenAI API gateway to use for moderation requests.</param>
    public ModerationApiFacade(IOpenAiApiGateway gateway)
    {
        _gateway = gateway;
    }

    /// <inheritdoc/>
    public async Task ProcessAsync(string input)
    {
        OpenAiModerationRequest request = new()
        {
            Model = "text-moderation-latest",
            Prompt = input
        };
        OpenAiModerationResponse response = await _gateway.GetModerationResponseAsync(request);

        if (!response.Success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {response.Error} (Status: {response.StatusCode})");
            Console.ResetColor();
            return;
        }

        if (response.Flagged)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Flagged");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Not Flagged");
        }
        Console.ResetColor();

        foreach (OpenAiModerationCategory? category in response.Categories.OrderBy(c => c.Name))
        {
            if (category.Flag)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else
            {
                Console.ResetColor();
            }
            Console.WriteLine($"  {category.Name}: {category.Flag} ({category.Score:F4})");
        }
        Console.ResetColor();
    }
}
