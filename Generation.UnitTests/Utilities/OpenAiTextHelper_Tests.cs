// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Genova.Generation.Models;
using Genova.Generation.Utilities;

namespace Genova.Generation.UnitTests.Utilities;

public class OpenAiTextHelper_Tests
{
    [Fact]
    public void Serialize_should_include_model_and_messages_with_context_and_prompt()
    {
        OpenAiTextRequest request = new ()
        {
            Model = "gpt-4",
            Context = "system context",
            Prompt = "user prompt"
        };

        OpenAiTextHelper openAiTextHelper = new ();

        string json = openAiTextHelper.Serialize(request);

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        root.GetProperty("model").GetString().Should().Be("gpt-4");

        JsonElement messages = root.GetProperty("messages");
        messages.GetArrayLength().Should().Be(2);

        messages[0].GetProperty("role").GetString().Should().Be("system");
        messages[0].GetProperty("content").GetString().Should().Be("system context");

        messages[1].GetProperty("role").GetString().Should().Be("user");
        messages[1].GetProperty("content").GetString().Should().Be("user prompt");
    }

    [Fact]
    public void Serialize_should_use_empty_string_for_null_model()
    {
        OpenAiTextRequest request = new ()
        {
            Model = null,
            Context = "context",
            Prompt = "prompt"
        };

        OpenAiTextHelper openAiTextHelper = new();

        string json = openAiTextHelper.Serialize(request);

        using JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("model").GetString().Should().BeEmpty();
    }

    [Fact]
    public void Serialize_should_allow_empty_context_and_prompt()
    {
        OpenAiTextRequest request = new()
        {
            Model = "gpt-4"
            // Context and Prompt left as default (empty)
        };

        OpenAiTextHelper openAiTextHelper = new();

        string json = openAiTextHelper.Serialize(request);

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement messages = doc.RootElement.GetProperty("messages");
        messages[0].GetProperty("content").GetString().Should().BeEmpty();
        messages[1].GetProperty("content").GetString().Should().BeEmpty();
    }

    [Fact]
    public async Task Deserialize_HttpResponseMessage_should_return_error_when_response_is_null()
    {
        OpenAiTextHelper openAiTextHelper = new();
        OpenAiTextResponse result = await openAiTextHelper.Deserialize((HttpResponseMessage)null!);
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Response is null.");
    }

    [Fact]
    public async Task Deserialize_HttpResponseMessage_should_return_error_when_status_is_not_success()
    {
        HttpResponseMessage response = new (HttpStatusCode.BadRequest)
        {
            ReasonPhrase = "Bad Request",
            Content = new StringContent("error body")
        };

        OpenAiTextHelper openAiTextHelper = new();

        OpenAiTextResponse result = await openAiTextHelper.Deserialize(response);
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("HTTP 400: Bad Request");
        result.Error.Should().Contain("error body");
    }

    [Fact]
    public async Task Deserialize_HttpResponseMessage_should_return_error_on_invalid_json()
    {
        HttpResponseMessage response = new (HttpStatusCode.OK)
        {
            Content = new StringContent("not a json")
        };

        OpenAiTextHelper openAiTextHelper = new();

        OpenAiTextResponse result = await openAiTextHelper.Deserialize(response);
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not a json");
    }

    [Fact]
    public void Deserialize_string_should_return_error_when_body_is_null_or_empty()
    {
        OpenAiTextHelper openAiTextHelper = new();

        OpenAiTextResponse result1 = openAiTextHelper.Deserialize((string)null!);
        OpenAiTextResponse result2 = openAiTextHelper.Deserialize("");
        OpenAiTextResponse result3 = openAiTextHelper.Deserialize("   ");

        result1.Success.Should().BeFalse();
        result1.Error.Should().Be("Response body is null or empty.");
        result2.Success.Should().BeFalse();
        result2.Error.Should().Be("Response body is null or empty.");
        result3.Success.Should().BeFalse();
        result3.Error.Should().Be("Response body is null or empty.");
    }

    [Fact]
    public void Deserialize_string_should_return_error_on_invalid_json()
    {
        OpenAiTextHelper openAiTextHelper = new();
        OpenAiTextResponse result = openAiTextHelper.Deserialize("not a json");
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not a json");
    }

    [Fact]
    public void Deserialize_string_should_return_error_when_json_is_null()
    {
        OpenAiTextHelper openAiTextHelper = new();
        // Simulate a JSON string that deserializes to null (not possible with System.Text.Json, but for coverage)
        // We'll use the overload with JsonDocument for this case.
        OpenAiTextResponse result = openAiTextHelper.Deserialize((JsonDocument)null!);
        result.Success.Should().BeFalse();
        result.Error.Should().Be("The response JSON is null.");
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_error_when_root_is_undefined()
    {
        OpenAiTextHelper openAiTextHelper = new();
        using JsonDocument doc = JsonDocument.Parse("null");
        OpenAiTextResponse result = openAiTextHelper.Deserialize(doc);
        // The root element of "null" is ValueKind.Null, not Undefined, so we need to simulate Undefined.
        // This is not possible with System.Text.Json, so we can only test the actual code path for Null.
        // We'll check that it does not succeed.
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_success_with_model_and_content()
    {
        string json = """
        {
            "model": "gpt-4",
            "choices": [
                {
                    "message": {
                        "content": "Hello, world!"
                    }
                }
            ]
        }
        """;

        OpenAiTextHelper openAiTextHelper = new();
        using JsonDocument doc = JsonDocument.Parse(json);
        OpenAiTextResponse result = openAiTextHelper.Deserialize(doc);

        result.Success.Should().BeTrue();
        result.Model.Should().Be("gpt-4");
        result.Content.Should().Be("Hello, world!");
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_success_with_empty_content_and_model_if_missing()
    {
        string json = """
        {
            "choices": [
                {
                    "message": {
                        "content": null
                    }
                }
            ]
        }
        """;

        OpenAiTextHelper openAiTextHelper = new();
        using JsonDocument doc = JsonDocument.Parse(json);
        OpenAiTextResponse result = openAiTextHelper.Deserialize(doc);

        result.Success.Should().BeTrue();
        result.Model.Should().BeEmpty();
        result.Content.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_success_with_empty_content_if_choices_missing()
    {
        string json = """
        {
            "model": "gpt-4"
        }
        """;

        OpenAiTextHelper openAiTextHelper = new();
        using JsonDocument doc = JsonDocument.Parse(json);
        OpenAiTextResponse result = openAiTextHelper.Deserialize(doc);

        result.Success.Should().BeTrue();
        result.Model.Should().Be("gpt-4");
        result.Content.Should().BeEmpty();
    }

    [Fact]
    public void ExtractContent_should_return_null_when_choices_not_present()
    {
        // Arrange
        string json = "{}";
        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? result = OpenAiTextHelper.ExtractContent(doc.RootElement);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractContent_should_return_null_when_choices_is_not_an_array()
    {
        // Arrange
        string json = """
            {
                "choices": {
                    "message": { "content": "Not an array" }
                }
            }
            """;
        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? result = OpenAiTextHelper.ExtractContent(doc.RootElement);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractContent_should_return_null_when_choices_array_is_empty()
    {
        // Arrange
        string json = """
            {
                "choices": []
            }
            """;
        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? result = OpenAiTextHelper.ExtractContent(doc.RootElement);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractContent_should_return_null_when_message_property_is_missing()
    {
        // Arrange
        string json = """
            {
                "choices": [
                    {
                        "noMessage": { "content": "Should not be found" }
                    }
                ]
            }
            """;
        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? result = OpenAiTextHelper.ExtractContent(doc.RootElement);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractContent_should_return_null_when_content_property_is_missing()
    {
        // Arrange
        string json = """
            {
                "choices": [
                    {
                        "message": { "notContent": "Missing content" }
                    }
                ]
            }
            """;
        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? result = OpenAiTextHelper.ExtractContent(doc.RootElement);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractContent_should_return_null_when_content_is_null()
    {
        // Arrange
        string json = """
            {
                "choices": [
                    {
                        "message": {
                            "content": null
                        }
                    }
                ]
            }
            """;
        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? result = OpenAiTextHelper.ExtractContent(doc.RootElement);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractContent_should_return_content_when_valid_structure_is_provided()
    {
        // Arrange
        string json = """
            {
                "choices": [
                    {
                        "message": {
                            "content": "Expected content"
                        }
                    }
                ]
            }
            """;
        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? result = OpenAiTextHelper.ExtractContent(doc.RootElement);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("Expected content");
    }

    [Fact]
    public async Task Deserialize_HttpResponseMessage_should_handle_JsonException()
    {
        // Arrange
        HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("not a json")
        };

        OpenAiTextHelper openAiTextHelper = new();

        // Act
        OpenAiTextResponse result = await openAiTextHelper.Deserialize(response);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not a json");
    }

    [Fact]
    public async Task Deserialize_HttpResponseMessage_should_handle_GeneralException()
    {
        // Arrange
        HttpContent throwingContent = new ThrowingContent(
            () => throw new InvalidOperationException("A different exception")
        );

        HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = throwingContent
        };

        OpenAiTextHelper openAiTextHelper = new();

        // Act
        OpenAiTextResponse result = await openAiTextHelper.Deserialize(response);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("InvalidOperationException: A different exception");
    }

    // Custom content that throws when ReadAsStringAsync is called
    internal class ThrowingContent : HttpContent
    {
        private readonly Action _throwAction;

        public ThrowingContent(Action throwAction)
        {
            _throwAction = throwAction;
        }

        protected override Task SerializeToStreamAsync(System.IO.Stream stream, TransportContext? context)
        {
            _throwAction();
            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return true;
        }
    }

    [Fact]
    public void Deserialize_should_return_error_when_root_is_undefined()
    {
        // Arrange
        JsonElement undefinedElement = default; // default(JsonElement) is Undefined

        OpenAiTextHelper openAiTextHelper = new();

        // Act
        OpenAiTextResponse result = openAiTextHelper.Deserialize(undefinedElement);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("The RootElement is undefined.");
    }

    [Fact]
    public void Deserialize_should_return_error_when_root_is_null()
    {
        // Arrange
        using JsonDocument doc = JsonDocument.Parse("null");
        JsonElement nullElement = doc.RootElement;

        OpenAiTextHelper openAiTextHelper = new();

        // Act
        OpenAiTextResponse result = openAiTextHelper.Deserialize(nullElement);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("The RootElement is null.");
    }

    [Fact]
    public void Deserialize_string_should_call_Deserialize_with_valid_json()
    {
        // Arrange
        string json = """
            {
                "model": "gpt-4",
                "choices": [
                    {
                        "message": {
                            "content": "Test content"
                        }
                    }
                ]
            }
            """;

        OpenAiTextHelper openAiTextHelper = new();

        // Act
        OpenAiTextResponse result = openAiTextHelper.Deserialize(json);

        // Assert
        result.Success.Should().BeTrue();
        result.Model.Should().Be("gpt-4");
        result.Content.Should().Be("Test content");
    }
}
