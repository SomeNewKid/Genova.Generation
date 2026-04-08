// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Genova.Generation.Gateways;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Genova.Generation.Terminal;

/// <summary>
/// Entry point for the Genova console application. 
/// Allows testing of different OpenAI API endpoints by switching the <c>ApiMode</c>.
/// </summary>
internal class Program
{
    /// <summary>
    /// Specifies which API mode to use for testing ("text" or "moderation").
    /// </summary>
    private const string ApiMode = "moderation";

    /// <summary>
    /// Main entry point for the console application.
    /// Configures dependency injection, logging, and runs the selected API facade in a loop.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddHttpClient();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning); // Set global minimum level to Warning
                logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning); // Filter HttpClient logs
            })
            .Build();

        GenerationOptions options = new() { OpenAiApiKey = GetOpenAiApiKey() };
        IHttpClientFactory httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
        OpenAiApiGateway apiGateway = new(options, httpClientFactory);

        IApiFacade api = ApiMode switch
        {
            "text" => new TextApiFacade(apiGateway),
            "moderation" => new ModerationApiFacade(apiGateway),
            _ => throw new InvalidOperationException("Unknown API mode.")
        };

        Console.WriteLine($"Enter input for {ApiMode} API (Ctrl+C to exit):");
        while (true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            await api.ProcessAsync(input);
        }
    }

    /// <summary>
    /// Retrieves the OpenAI API key from the environment variable <c>OPENAI_API_KEY</c>.
    /// Throws an exception if the key is not set.
    /// </summary>
    /// <returns>The OpenAI API key as a string.</returns>
    [ExcludeFromCodeCoverage(Justification = "Not possible to test the exception.")]
    private static string GetOpenAiApiKey()
    {
        string? apiKey = Environment.GetEnvironmentVariable(GenerationModule.OpenAiApiKeyEnvironmentVaraible);
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("API key is not set in the environment variables.");
        }

        return apiKey;
    }
}
