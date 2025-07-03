// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using FluentAssertions;
using Genova.Generation.Models;

namespace Genova.Generation.UnitTests.Models;

public class OpenAiImageRequest_Tests
{
    [Fact]
    public void Default_constructor_should_set_Number_to_1_and_other_properties_to_empty_or_null()
    {
        OpenAiImageRequest request = new ();

        request.Model.Should().BeNull();
        request.Number.Should().Be(1);
        request.Size.Should().BeEmpty();
        request.Prompt.Should().BeEmpty();
    }

    [Fact]
    public void Can_set_and_get_Model_property()
    {
        OpenAiImageRequest request = new()
        {
            Model = "dall-e"
        };

        request.Model.Should().Be("dall-e");
    }

    [Fact]
    public void Can_set_and_get_Number_property()
    {
        OpenAiImageRequest request = new()
        {
            Number = 5
        };

        request.Number.Should().Be(5);
    }

    [Fact]
    public void Can_set_and_get_Size_property()
    {
        OpenAiImageRequest request = new()
        {
            Size = "512x512"
        };

        request.Size.Should().Be("512x512");
    }

    [Fact]
    public void Can_set_and_get_Prompt_property()
    {
        OpenAiImageRequest request = new()
        {
            Prompt = "A cat riding a bicycle"
        };

        request.Prompt.Should().Be("A cat riding a bicycle");
    }
}
