// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;
using Genova.Common.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;

namespace Genova.Generation;

/// <summary>
/// Represents a module that integrates with the Engine and provides authorization-related services.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by websites.")]
public sealed class GenerationModule : IModule
{
    /// <summary>
    /// The environment variable name for the OpenAI API key.
    /// </summary>
    // <notes>
    // 1. Reequirements of an environment name for a Ubuntu-based container:
    //    A value must consist of lower case alphanumeric characters, '-',
    //    and must start and end with an alphanumeric character.
    //    The length must not be more than 253 characters.
    // </notes>
    public const string OpenAiApiKeyEnvironmentVaraible = "OPENAI_API_KEY";

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationModule"/> class.
    /// </summary>
    public GenerationModule()
    {
    }

    /// <summary>
    /// Gets the name of the module.
    /// </summary>
    public static string Name => "Generation";

    /// <inheritdoc/>
    public IStringLocalizer? Localizer
    {
        get
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public string[] ViewLocations
    {
        get
        {
            return [];
        }
    }

    /// <inheritdoc/>
    public void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
    }
}
