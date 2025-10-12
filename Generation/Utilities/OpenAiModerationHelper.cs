// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using Genova.Generation.Models;

namespace Genova.Generation.Utilities;

/// <summary>
/// Helper responsible for serializing moderation requests and deserializing moderation responses.
/// </summary>
internal sealed class OpenAiModerationHelper : OpenAiBaseHelper<OpenAiModerationRequest, OpenAiModerationResponse>
{
    /// <inheritdoc/>
    protected override Dictionary<string, object?> BuildPayload(OpenAiModerationRequest request)
    {
        return new Dictionary<string, object?>
        {
            ["model"] = request.Model, // allow null; serializer will omit if null
            ["input"] = request.Prompt ?? string.Empty,
        };
    }

    /// <inheritdoc/>
    protected override OpenAiModerationResponse CreateErrorResponse(string error, int statusCode = -1)
    {
        OpenAiModerationResponse response = CreateCommonErrorResponse(error, statusCode);
        return response;
    }

    /// <inheritdoc/>
    protected override OpenAiModerationResponse DeserializeFromRoot(JsonElement rootElement)
    {
        // model
        string model = rootElement.TryGetProperty("model", out JsonElement modelEl)
            ? (modelEl.GetString() ?? string.Empty)
            : string.Empty;

        bool flagged = false;
        var categories = new List<OpenAiModerationCategory>();

        // results[0]
        if (rootElement.TryGetProperty("results", out JsonElement resultsEl) &&
            resultsEl.ValueKind == JsonValueKind.Array &&
            resultsEl.GetArrayLength() > 0)
        {
            JsonElement first = resultsEl[0];

            // flagged
            if (first.TryGetProperty("flagged", out JsonElement flaggedEl) &&
                (flaggedEl.ValueKind is JsonValueKind.True or JsonValueKind.False))
            {
                flagged = flaggedEl.GetBoolean();
            }

            // categories booleans
            Dictionary<string, bool> catFlags = [];
            if (first.TryGetProperty("categories", out JsonElement catsEl) &&
                catsEl.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in catsEl.EnumerateObject())
                {
                    bool val = (prop.Value.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        ? prop.Value.GetBoolean()
                        : false;
                    catFlags[prop.Name] = val;
                }
            }

            // category scores (as double)
            Dictionary<string, double> catScores = [];
            if (first.TryGetProperty("category_scores", out JsonElement scoresEl) &&
                scoresEl.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty prop in scoresEl.EnumerateObject())
                {
                    double score = prop.Value.ValueKind == JsonValueKind.Number &&
                                   prop.Value.TryGetDouble(out double d)
                                    ? d : 0d;
                    catScores[prop.Name] = score;
                }
            }

            // Flatten -> List<OpenAiModerationCategory>
            foreach (KeyValuePair<string, bool> kvp in catFlags)
            {
                catScores.TryGetValue(kvp.Key, out double s);
                categories.Add(new OpenAiModerationCategory
                {
                    Name = kvp.Key,
                    Flag = kvp.Value,
                    Score = s,
                });
            }

            // handle scores without a corresponding boolean (defensive)
            foreach (var kvp in catScores)
            {
                if (!catFlags.ContainsKey(kvp.Key))
                {
                    categories.Add(new OpenAiModerationCategory
                    {
                        Name = kvp.Key,
                        Flag = false,
                        Score = kvp.Value,
                    });
                }
            }
        }

        return new OpenAiModerationResponse
        {
            Success = true,
            Model = model,
            Flagged = flagged,
            Categories = categories,
        };
    }
}
