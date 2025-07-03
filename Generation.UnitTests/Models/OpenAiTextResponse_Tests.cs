// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using FluentAssertions;
using Genova.Generation.Models;

namespace Genova.Generation.UnitTests.Models;

public class OpenAiTextResponse_Tests
{
    [Fact]
    public void Default_constructor_should_set_expected_defaults()
    {
        OpenAiTextResponse response = new ();

        response.Model.Should().BeEmpty();
        response.Success.Should().BeTrue();
        response.Error.Should().BeEmpty();
        response.Content.Should().BeEmpty();
    }

    [Fact]
    public void Can_set_and_get_Model_property()
    {
        OpenAiTextResponse response = new ()
        {
            Model = "gpt-4"
        };

        response.Model.Should().Be("gpt-4");
    }

    [Fact]
    public void Can_set_and_get_Success_property()
    {
        OpenAiTextResponse response = new ()
        {
            Success = false
        };

        response.Success.Should().BeFalse();
    }

    [Fact]
    public void Can_set_and_get_Error_property()
    {
        OpenAiTextResponse response = new ()
        {
            Error = "Your request was rejected as a result of our safety system."
        };

        response.Error.Should().Be("Your request was rejected as a result of our safety system.");
    }

    [Fact]
    public void Can_set_and_get_Content_property()
    {
        OpenAiTextResponse response = new ()
        {
            Content = "Hello, world!"
        };

        response.Content.Should().Be("Hello, world!");
    }
}
