# Genova.Generation

Provides generation and moderation helpers for integrating OpenAI-backed AI features into a Genova website.

## Installation

Add a reference to the package, or build the project:

```bash
dotnet build
```

## Usage

```csharp
using Genova.Generation;
using Genova.Generation.Gateways;
using Genova.Generation.Models;

var options = new GenerationOptions
{
    OpenAiApiKey = Environment.GetEnvironmentVariable(GenerationModule.OpenAiApiKeyEnvironmentVaraible) ?? string.Empty,
};

var gateway = new OpenAiApiGateway(options, httpClientFactory);

var response = await gateway.GetTextResponseAsync(new OpenAiTextRequest
{
    Model = "gpt-4o-mini",
    Context = "You are a helpful assistant.",
    Prompt = "Write a short summary.",
});
```

## Features

* Text generation requests
* Image generation requests
* Moderation requests
* Simple request and response models for OpenAI APIs

## Notes

* Part of the Genova multi-tenant ASP.NET Core platform.
* This is a class library module, not a standalone application.
* Requires an OpenAI API key, commonly supplied via the `OPENAI_API_KEY` environment variable.

## License

GNU General Public License v3.0
