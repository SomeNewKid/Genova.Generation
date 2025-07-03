// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using FluentAssertions;
using Genova.Generation.Models;

namespace Genova.Generation.UnitTests.Models;

public class OpenAiImageResponse_Tests
{
    [Fact]
    public void Default_constructor_should_set_expected_defaults()
    {
        OpenAiImageResponse response = new ();

        response.Model.Should().BeEmpty();
        response.Success.Should().BeTrue();
        response.Error.Should().BeEmpty();
        response.Url.Should().BeEmpty();
    }

    [Fact]
    public void Can_set_and_get_Model_property()
    {
        OpenAiImageResponse response = new ()
        {
            Model = "dall-e"
        };

        response.Model.Should().Be("dall-e");
    }

    [Fact]
    public void Can_set_and_get_Success_property()
    {
        OpenAiImageResponse response = new ()
        {
            Success = false
        };

        response.Success.Should().BeFalse();
    }

    [Fact]
    public void Can_set_and_get_Error_property()
    {
        OpenAiImageResponse response = new ()
        {
            Error = "Your request was rejected as a result of our safety system."
        };

        response.Error.Should().Be("Your request was rejected as a result of our safety system.");
    }

    [Fact]
    public void Can_set_and_get_Url_property()
    {
        OpenAiImageResponse response = new ()
        {
            Url = "https://example.com/image.png"
        };

        response.Url.Should().Be("https://example.com/image.png");
    }
}
