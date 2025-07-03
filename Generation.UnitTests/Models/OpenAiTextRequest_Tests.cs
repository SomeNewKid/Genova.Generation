// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using FluentAssertions;
using Genova.Generation.Models;

namespace Genova.Generation.UnitTests.Models;

public class OpenAiTextRequest_Tests
{
    [Fact]
    public void Default_constructor_should_set_Context_and_Prompt_to_empty_string()
    {
        OpenAiTextRequest request = new ();

        request.Context.Should().BeEmpty();
        request.Prompt.Should().BeEmpty();
        request.Model.Should().BeNull();
    }

    [Fact]
    public void Can_set_and_get_Model_property()
    {
        OpenAiTextRequest request = new ()
        {
            Model = "gpt-4"
        };

        request.Model.Should().Be("gpt-4");
    }

    [Fact]
    public void Can_set_and_get_Context_property()
    {
        OpenAiTextRequest request = new ()
        {
            Context = "system context"
        };

        request.Context.Should().Be("system context");
    }

    [Fact]
    public void Can_set_and_get_Prompt_property()
    {
        OpenAiTextRequest request = new ()
        {
            Prompt = "user prompt"
        };

        request.Prompt.Should().Be("user prompt");
    }
}
