// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using FluentAssertions;

namespace Genova.Generation.UnitTests;

public class GenerationModule_Tests
{
    [Fact]
    public void Module_name()
    {
        // Act & Assert
        Assert.Equal("Generation", GenerationModule.Name);
    }
}
