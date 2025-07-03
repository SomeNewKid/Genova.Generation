// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Genova.Generation.Models;
using Genova.Generation.Utilities;
using Moq;

namespace Genova.Generation.UnitTests.Utilities;

public class OpenAiImageHelper_Tests
{
    [Fact]
    public void Serialize_should_include_model_and_prompt_and_size_and_number()
    {
        // Arrange
        OpenAiImageRequest request = new()
        {
            Model = "image-beta-v3",
            Prompt = "A bright, sunny day in the park",
            Size = "1024x1024",
            Number = 2
        };

        OpenAiImageHelper openAiImageHelper = new();

        // Act
        string json = openAiImageHelper.Serialize(request);

        // Assert
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        root.GetProperty("model").GetString().Should().Be("image-beta-v3");
        root.GetProperty("prompt").GetString().Should().Be("A bright, sunny day in the park");
        root.GetProperty("size").GetString().Should().Be("1024x1024");
        root.GetProperty("n").GetInt32().Should().Be(2);
    }

    [Fact]
    public void Serialize_should_use_empty_string_for_null_model()
    {
        // Arrange
        OpenAiImageRequest request = new()
        {
            Model = null,
            Prompt = "A face portrait",
            Size = "512x512",
            Number = 1
        };

        OpenAiImageHelper openAiImageHelper = new();

        // Act
        string json = openAiImageHelper.Serialize(request);

        // Assert
        using JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("model").GetString().Should().BeEmpty();
    }

    [Fact]
    public async Task Deserialize_HttpResponseMessage_should_return_error_when_response_is_null()
    {
        // Arrange
        OpenAiImageHelper openAiImageHelper = new();

        // Act
        OpenAiImageResponse result = await openAiImageHelper.Deserialize((HttpResponseMessage)null!);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Response is null.");
    }

    [Fact]
    public async Task Deserialize_HttpResponseMessage_should_return_error_when_status_is_not_success_and_reason_phrase_is_null()
    {
        // Arrange
        HttpResponseMessage response = new(HttpStatusCode.NotFound)
        {
            ReasonPhrase = null,
            Content = new StringContent("Resource not found")
        };

        OpenAiImageHelper openAiImageHelper = new();

        // Act
        OpenAiImageResponse result = await openAiImageHelper.Deserialize(response);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("HTTP 404");
        result.Error.Should().Contain("Resource not found");
    }

    [Fact]
    public async Task Deserialize_HttpResponseMessage_should_return_error_when_status_is_not_success()
    {
        // Arrange
        HttpResponseMessage response = new (HttpStatusCode.BadRequest)
        {
            ReasonPhrase = "Bad Request",
            Content = new StringContent("error body")
        };

        OpenAiImageHelper openAiImageHelper = new();

        // Act
        OpenAiImageResponse result = await openAiImageHelper.Deserialize(response);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("HTTP 400: Bad Request");
        result.Error.Should().Contain("error body");
    }

    [Fact]
    public async Task Deserialize_HttpResponseMessage_should_return_error_on_invalid_json()
    {
        // Arrange
        HttpResponseMessage response = new (HttpStatusCode.OK)
        {
            Content = new StringContent("not a json")
        };

        OpenAiImageHelper openAiImageHelper = new();

        // Act
        OpenAiImageResponse result = await openAiImageHelper.Deserialize(response);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not a json");
    }

    [Fact]
    public void Deserialize_string_should_return_error_when_body_is_null_or_empty()
    {
        // Arrange
        OpenAiImageHelper openAiImageHelper = new();

        // Act
        OpenAiImageResponse result1 = openAiImageHelper.Deserialize((string)null!);
        OpenAiImageResponse result2 = openAiImageHelper.Deserialize("");
        OpenAiImageResponse result3 = openAiImageHelper.Deserialize("   ");

        // Assert
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
        // Arrange
        OpenAiImageHelper openAiImageHelper = new();

        // Act
        OpenAiImageResponse result = openAiImageHelper.Deserialize("not a json");

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not a json");
    }

    [Fact]
    public void Deserialize_string_should_return_error_when_json_is_null()
    {
        // Arrange
        OpenAiImageHelper openAiImageHelper = new();

        // Act
        // Simulate a JSON string that deserializes to null (not truly possible with System.Text.Json, but we test it)
        OpenAiImageResponse result = openAiImageHelper.Deserialize((JsonDocument)null!);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("The response JSON is null.");
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_error_when_root_is_null()
    {
        // Arrange
        OpenAiImageHelper openAiImageHelper = new();

        // Act
        using JsonDocument doc = JsonDocument.Parse("null");
        OpenAiImageResponse result = openAiImageHelper.Deserialize(doc);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("The RootElement is null.");
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_error_from_error_message()
    {
        // Arrange
        string json = """
            {
              "error": {
                "message": "Your request was rejected as a result of our safety system.",
                "type": "invalid_request_error"
              }
            }
            """;

        OpenAiImageHelper openAiImageHelper = new();

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = openAiImageHelper.Deserialize(doc);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Your request was rejected as a result of our safety system.");
        result.Url.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_success_when_url_is_present()
    {
        // Arrange
        string json = """
            {
              "created": 1748531603,
              "data": [
                {
                  "revised_prompt": "Create a greyscale image ...",
                  "url": "https://some-image-url"
                }
              ]
            }
            """;

        OpenAiImageHelper openAiImageHelper = new();

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = openAiImageHelper.Deserialize(doc);

        // Assert
        result.Success.Should().BeTrue();
        result.Error.Should().BeEmpty();
        result.Url.Should().Be("https://some-image-url");
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_success_with_empty_url_when_missing_data_array()
    {
        // Arrange
        string json = """
            {
              "created": 1748531603
            }
            """;

        OpenAiImageHelper openAiImageHelper = new();

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = openAiImageHelper.Deserialize(doc);

        // Assert
        result.Success.Should().BeTrue();
        result.Url.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_success_with_empty_url_when_data_array_is_empty()
    {
        // Arrange
        string json = """
            {
              "data": []
            }
            """;

        OpenAiImageHelper openAiImageHelper = new();

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = openAiImageHelper.Deserialize(doc);

        // Assert
        result.Success.Should().BeTrue();
        result.Url.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_success_with_empty_url_when_url_property_is_missing()
    {
        // Arrange
        string json = """
            {
              "data": [
                {
                  "revised_prompt": "Some prompt"
                }
              ]
            }        
            """;

        OpenAiImageHelper openAiImageHelper = new();

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = openAiImageHelper.Deserialize(doc);

        // Assert
        result.Success.Should().BeTrue();
        result.Url.Should().BeEmpty();
    }

    [Fact]
    public async Task Deserialize_HttpResponseMessage_should_handle_JsonException()
    {
        // Arrange
        HttpContent throwingContent = new ThrowingContent(
            () => throw new JsonException("Mock JSON exception")
        );

        HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = throwingContent
        };

        OpenAiImageHelper openAiImageHelper = new();

        // Act
        OpenAiImageResponse result = await openAiImageHelper.Deserialize(response);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Mock JSON exception");
    }

    // Similar approach can be used for a general Exception:
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

        OpenAiImageHelper openAiImageHelper = new();

        // Act
        OpenAiImageResponse result = await openAiImageHelper.Deserialize(response);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("A different exception");
    }

    // Custom content that throws when ReadAsStringAsync is called
    internal class ThrowingContent : HttpContent
    {
        private readonly Action _throwAction;

        public ThrowingContent(Action throwAction)
        {
            _throwAction = throwAction;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
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
    public void Deserialize_JsonElement_should_return_error_when_root_is_undefined()
    {
        // Arrange
        JsonElement undefinedElement = default; // default(JsonElement) is Undefined

        OpenAiImageHelper openAiImageHelper = new();

        // Act
        OpenAiImageResponse result = openAiImageHelper.Deserialize(undefinedElement);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("The RootElement is undefined.");
    }

    [Fact]
    public void Deserialize_JsonElement_should_return_error_when_root_is_null()
    {
        // Arrange
        using JsonDocument doc = JsonDocument.Parse("null");
        JsonElement nullElement = doc.RootElement;

        OpenAiImageHelper openAiImageHelper = new();

        // Act
        OpenAiImageResponse result = openAiImageHelper.Deserialize(nullElement);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("The RootElement is null.");
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_default_error_when_error_message_is_missing()
    {
        // Arrange
        string json = """            
            {
              "error": {
                "type": "invalid_request_error"
              }
            }
            
            """;

        OpenAiImageHelper openAiImageHelper = new();

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = openAiImageHelper.Deserialize(doc);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Unknown error returned by GPT.");
        result.Url.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_JsonDocument_should_return_success_with_empty_url_when_data_is_null()
    {
        // Arrange
        string json = """
            {
              "data": null
            }
            """;

        OpenAiImageHelper openAiImageHelper = new();

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = openAiImageHelper.Deserialize(doc);

        // Assert
        result.Success.Should().BeTrue();
        result.Url.Should().BeEmpty();
    }

    [Fact]
    public void ExtractUrl_should_return_null_when_data_property_is_missing()
    {
        // Arrange
        string json = """
            {
              "not_data": []
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? url = OpenAiImageHelper.ExtractUrl(doc.RootElement);

        // Assert
        url.Should().BeNull();
    }

    [Fact]
    public void ExtractUrl_should_return_null_when_data_is_not_an_array()
    {
        // Arrange
        string json = """
            {
              "data": { "url": "https://example.com" }
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? url = OpenAiImageHelper.ExtractUrl(doc.RootElement);

        // Assert
        url.Should().BeNull();
    }

    [Fact]
    public void ExtractUrl_should_return_null_when_data_is_null()
    {
        // Arrange
        string json = """
            {
              "data": null
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? url = OpenAiImageHelper.ExtractUrl(doc.RootElement);

        // Assert
        url.Should().BeNull();
    }

    [Fact]
    public void ExtractUrl_should_return_null_when_data_array_is_empty()
    {
        // Arrange
        string json = """
            {
              "data": []
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? url = OpenAiImageHelper.ExtractUrl(doc.RootElement);

        // Assert
        url.Should().BeNull();
    }

    [Fact]
    public void ExtractUrl_should_return_null_when_first_data_element_has_no_url_property()
    {
        // Arrange
        string json = """
            {
              "data": [
                { "not_url": "value" }
              ]
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? url = OpenAiImageHelper.ExtractUrl(doc.RootElement);

        // Assert
        url.Should().BeNull();
    }

    [Fact]
    public void ExtractUrl_should_return_url_when_first_data_element_has_url_property()
    {
        // Arrange
        string json = """
            {
              "data": [
                { "url": "https://image-url" }
              ]
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        string? url = OpenAiImageHelper.ExtractUrl(doc.RootElement);

        // Assert
        url.Should().Be("https://image-url");
    }

    [Fact]
    public void DeserializeFromInternalPayload_should_deserialize_valid_b64_and_output_format()
    {
        // Arrange
        byte[] expectedBytes = Encoding.UTF8.GetBytes("hello world");
        string b64 = Convert.ToBase64String(expectedBytes);
        string json = $@"
        {{
            ""data"": [{{ ""b64_json"": ""{b64}"" }}],
            ""output_format"": ""png""
        }}";
        JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromInternalPayload(doc.RootElement);

        // Assert
        result.Success.Should().BeTrue();
        result.Bytes.Should().BeEquivalentTo(expectedBytes);
        result.Format.Should().Be("png");
        result.Url.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromInternalPayload_should_deserialize_valid_b64_and_missing_output_format()
    {
        // Arrange
        byte[] expectedBytes = Encoding.UTF8.GetBytes("test");
        string b64 = Convert.ToBase64String(expectedBytes);
        string json = $@"{{ ""data"": [{{ ""b64_json"": ""{b64}"" }}] }}";
        JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromInternalPayload(doc.RootElement);

        // Assert
        result.Success.Should().BeTrue();
        result.Bytes.Should().BeEquivalentTo(expectedBytes);
        result.Format.Should().BeEmpty();
        result.Url.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromInternalPayload_should_handle_invalid_base64()
    {
        // Arrange
        string json = @"{ ""data"": [ { ""b64_json"": ""not_base64!"" } ], ""output_format"": ""jpg"" }";
        JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromInternalPayload(doc.RootElement);

        // Assert
        result.Success.Should().BeTrue();
        result.Bytes.Should().BeEmpty();
        result.Format.Should().Be("jpg");
        result.Url.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromInternalPayload_should_handle_missing_b64_json()
    {
        // Arrange
        string json = @"{ ""data"": [ { ""other"": ""value"" } ], ""output_format"": ""gif"" }";
        JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromInternalPayload(doc.RootElement);

        // Assert
        result.Success.Should().BeTrue();
        result.Bytes.Should().BeEmpty();
        result.Format.Should().Be("gif");
        result.Url.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromInternalPayload_should_handle_empty_data_array()
    {
        // Arrange
        string json = @"{ ""data"": [], ""output_format"": ""bmp"" }";
        JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromInternalPayload(doc.RootElement);

        // Assert
        result.Success.Should().BeTrue();
        result.Bytes.Should().BeEmpty();
        result.Format.Should().Be("bmp");
        result.Url.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromInternalPayload_should_handle_missing_data_property()
    {
        // Arrange
        string json = @"{ ""output_format"": ""tiff"" }";
        JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromInternalPayload(doc.RootElement);

        // Assert
        result.Success.Should().BeTrue();
        result.Bytes.Should().BeEmpty();
        result.Format.Should().Be("tiff");
        result.Url.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromInternalPayload_should_trim_dot_and_whitespace_from_output_format()
    {
        // Arrange
        byte[] expectedBytes = Encoding.UTF8.GetBytes("abc");
        string b64 = Convert.ToBase64String(expectedBytes);
        string json = $@"
        {{
            ""data"": [{{ ""b64_json"": ""{b64}"" }}],
            ""output_format"": "" .JpEg  ""
        }}";
        JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromInternalPayload(doc.RootElement);

        // Assert
        result.Success.Should().BeTrue();
        result.Bytes.Should().BeEquivalentTo(expectedBytes);
        result.Format.Should().Be("jpeg");
        result.Url.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromExternalPayload_should_deserialize_valid_url_with_extension()
    {
        string json = @"{ ""data"": [ { ""url"": ""https://example.com/image.png"" } ] }";
        JsonDocument doc = JsonDocument.Parse(json);

        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromExternalPayload(doc.RootElement);

        result.Success.Should().BeTrue();
        result.Url.Should().Be("https://example.com/image.png");
        result.Format.Should().Be("png");
        result.Bytes.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromExternalPayload_should_deserialize_valid_url_with_no_extension()
    {
        string json = @"{ ""data"": [ { ""url"": ""https://example.com/image"" } ] }";
        JsonDocument doc = JsonDocument.Parse(json);

        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromExternalPayload(doc.RootElement);

        result.Success.Should().BeTrue();
        result.Url.Should().Be("https://example.com/image");
        result.Format.Should().BeEmpty();
        result.Bytes.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromExternalPayload_should_deserialize_url_with_uppercase_extension_and_leading_dot()
    {
        string json = @"{ ""data"": [ { ""url"": ""https://example.com/image.JPEG"" } ] }";
        JsonDocument doc = JsonDocument.Parse(json);

        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromExternalPayload(doc.RootElement);

        result.Success.Should().BeTrue();
        result.Url.Should().Be("https://example.com/image.JPEG");
        result.Format.Should().Be("jpeg");
        result.Bytes.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromExternalPayload_should_handle_empty_data_array()
    {
        string json = @"{ ""data"": [] }";
        JsonDocument doc = JsonDocument.Parse(json);

        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromExternalPayload(doc.RootElement);

        result.Success.Should().BeTrue();
        result.Url.Should().BeEmpty();
        result.Format.Should().BeEmpty();
        result.Bytes.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromExternalPayload_should_handle_missing_data_property()
    {
        string json = @"{ ""other"": 123 }";
        JsonDocument doc = JsonDocument.Parse(json);

        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromExternalPayload(doc.RootElement);

        result.Success.Should().BeTrue();
        result.Url.Should().BeEmpty();
        result.Format.Should().BeEmpty();
        result.Bytes.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromExternalPayload_should_handle_missing_url_property_in_data()
    {
        string json = @"{ ""data"": [ { ""noturl"": ""value"" } ] }";
        JsonDocument doc = JsonDocument.Parse(json);

        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromExternalPayload(doc.RootElement);

        result.Success.Should().BeTrue();
        result.Url.Should().BeEmpty();
        result.Format.Should().BeEmpty();
        result.Bytes.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromExternalPayload_should_handle_url_property_not_a_string()
    {
        string json = @"{ ""data"": [ { ""url"": 12345 } ] }";
        JsonDocument doc = JsonDocument.Parse(json);

        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromExternalPayload(doc.RootElement);

        result.Success.Should().BeTrue();
        result.Url.Should().BeEmpty();
        result.Format.Should().BeEmpty();
        result.Bytes.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void DeserializeFromExternalPayload_should_handle_url_property_null_or_empty()
    {
        string json = @"{ ""data"": [ { ""url"": """" } ] }";
        JsonDocument doc = JsonDocument.Parse(json);

        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromExternalPayload(doc.RootElement);

        result.Success.Should().BeTrue();
        result.Url.Should().BeEmpty();
        result.Format.Should().BeEmpty();
        result.Bytes.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public async Task Deserialize_should_include_known_reason_phrase_in_error()
    {
        // Arrange
        string reasonPhrase = "Bad Request";
        string responseBody = "{\"error\":\"Invalid input\"}";
        HttpResponseMessage response = new (HttpStatusCode.BadRequest)
        {
            ReasonPhrase = reasonPhrase,
            Content = new StringContent(responseBody)
        };

        OpenAiImageHelper helper = new OpenAiImageHelper();

        // Act
        OpenAiImageResponse result = await helper.Deserialize(response);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain(reasonPhrase);
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DeserializeFromInternalPayload_should_return_empty_format_when_output_format_is_empty_or_whitespace(string outputFormat)
    {
        // Arrange
        byte[] expectedBytes = Encoding.UTF8.GetBytes("data");
        string b64 = Convert.ToBase64String(expectedBytes);
        string json = $@"
            {{
                ""data"": [{{ ""b64_json"": ""{b64}"" }}],
                ""output_format"": ""{outputFormat}""
            }}";
        using JsonDocument doc = JsonDocument.Parse(json);

        // Act
        OpenAiImageResponse result = OpenAiImageHelper.DeserializeFromInternalPayload(doc.RootElement);

        // Assert
        result.Success.Should().BeTrue();
        result.Bytes.Should().BeEquivalentTo(expectedBytes);
        result.Format.Should().BeEmpty();
        result.Url.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_should_call_DeserializeFromInternalPayload_when_internal_payload()
    {
        // Arrange
        byte[] expectedBytes = Encoding.UTF8.GetBytes("test-image");
        string b64 = Convert.ToBase64String(expectedBytes);
        string json = $@"
            {{
                ""data"": [{{ ""b64_json"": ""{b64}"" }}],
                ""output_format"": ""png""
            }}";
        using JsonDocument doc = JsonDocument.Parse(json);
        OpenAiImageHelper helper = new ();

        // Act
        OpenAiImageResponse result = helper.Deserialize(doc);

        // Assert
        result.Success.Should().BeTrue();
        result.Bytes.Should().BeEquivalentTo(expectedBytes);
        result.Format.Should().Be("png");
        result.Url.Should().BeEmpty();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void GetStringValue_should_return_value_when_not_null()
    {
        string result = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetStringValue("hello", "default");
        result.Should().Be("hello");
    }

    [Fact]
    public void GetStringValue_should_return_default_when_value_is_null()
    {
        string result = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetStringValue(null, "default");
        result.Should().Be("default");
    }

    [Fact]
    public void GetStringValue_should_return_empty_string_when_both_null_and_no_default_specified()
    {
        string result = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetStringValue(null);
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetStringValue_should_return_value_even_if_default_is_provided()
    {
        string result = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetStringValue("value", "default");
        result.Should().Be("value");
    }
}
