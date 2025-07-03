// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Genova.Generation.Models;
using Genova.Generation.Utilities;
using Xunit;

namespace Genova.Generation.UnitTests.Utilities;

public class OpenAiBaseHelper_Tests
{
    [Fact]
    public void GetRateLimits_should_return_defaults_when_httpResponse_is_null()
    {
        // Act
        OpenAIRateLimits limits = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetRateLimits(null);

        // Assert
        limits.LimitRequests.Should().Be(0);
        limits.LimitTokens.Should().Be(0);
        limits.RemainingRequests.Should().Be(0);
        limits.RemainingTokens.Should().Be(0);
        limits.ResetRequests.Should().Be(default);
        limits.ResetTokens.Should().Be(default);
    }

    [Fact]
    public void GetRateLimits_should_parse_headers_from_response_headers()
    {
        // Arrange
        HttpResponseMessage response = new ();
        response.Headers.Add("X-RateLimit-Limit-Requests", "10");
        response.Headers.Add("X-RateLimit-Limit-Tokens", "100");
        response.Headers.Add("X-RateLimit-Remaining-Requests", "5");
        response.Headers.Add("X-RateLimit-Remaining-Tokens", "50");
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        response.Headers.Add("X-RateLimit-Reset-Requests", now.ToString());
        response.Headers.Add("X-RateLimit-Reset-Tokens", (now + 60).ToString());

        // Act
        OpenAIRateLimits limits = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetRateLimits(response);

        // Assert
        limits.LimitRequests.Should().Be(10);
        limits.LimitTokens.Should().Be(100);
        limits.RemainingRequests.Should().Be(5);
        limits.RemainingTokens.Should().Be(50);
        limits.ResetRequests.Should().Be(DateTimeOffset.FromUnixTimeSeconds(now).UtcDateTime);
        limits.ResetTokens.Should().Be(DateTimeOffset.FromUnixTimeSeconds(now + 60).UtcDateTime);
    }

    [Fact]
    public void GetRateLimits_should_parse_headers_from_content_headers()
    {
        // Arrange
        HttpResponseMessage response = new()
        {
            Content = new StringContent("test")
        };
        response.Content.Headers.Add("X-RateLimit-Limit-Requests", "42");

        // Act
        OpenAIRateLimits limits = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetRateLimits(response);

        // Assert
        limits.LimitRequests.Should().Be(42);
    }

    [Fact]
    public void GetRateLimits_should_ignore_invalid_header_values()
    {
        // Arrange
        HttpResponseMessage response = new ();
        response.Headers.Add("X-RateLimit-Limit-Requests", "notanumber");
        response.Headers.Add("X-RateLimit-Reset-Requests", "notanumber");

        // Act
        OpenAIRateLimits limits = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetRateLimits(response);

        // Assert
        limits.LimitRequests.Should().Be(0);
        limits.ResetRequests.Should().Be(default);
    }

    [Fact]
    public void GetRateLimits_should_return_defaults_when_headers_missing()
    {
        // Arrange
        HttpResponseMessage response = new ();

        // Act
        OpenAIRateLimits limits = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetRateLimits(response);

        // Assert
        limits.LimitRequests.Should().Be(0);
        limits.LimitTokens.Should().Be(0);
        limits.RemainingRequests.Should().Be(0);
        limits.RemainingTokens.Should().Be(0);
        limits.ResetRequests.Should().Be(default);
        limits.ResetTokens.Should().Be(default);
    }

    [Fact]
    public void GetHeader_should_return_null_when_headers_is_null()
    {
        string? result = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetHeader(null, "X-Test");
        result.Should().BeNull();
    }

    [Fact]
    public void GetHeader_should_return_null_when_key_not_found()
    {
        HttpRequestHeaders headers = new HttpRequestMessage().Headers;
        string? result = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetHeader(headers, "X-Not-Found");
        result.Should().BeNull();
    }

    [Fact]
    public void GetHeader_should_return_first_value_when_key_exists()
    {
        HttpRequestHeaders headers = new HttpRequestMessage().Headers;
        headers.Add("X-Test", ["value1", "value2"]);
        string? result = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetHeader(headers, "X-Test");
        result.Should().Be("value1");
    }

    [Fact]
    public void GetHeader_should_return_null_when_key_exists_but_no_values()
    {
        // Simulate a header with an empty value
        HttpRequestHeaders headers = new HttpRequestMessage().Headers;
        headers.Add("X-Empty", []);
        string? result = OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>.GetHeader(headers, "X-Empty");
        result.Should().BeNull();
    }
}
