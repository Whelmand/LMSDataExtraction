using LMSDataExtraction.Domain.Entities;
using Xunit;

namespace LMSDataExtraction.Tests.Entities;

public class ModuleTests
{
    [Fact]
    public void NewModule_ShouldHaveEmptyDefaults()
    {
        Module module = new Module();

        Assert.Equal(0, module.Id);
        Assert.Equal(0, module.CanvasId);
        Assert.Equal(0, module.CourseId);
        Assert.Equal(string.Empty, module.Name);
        Assert.Equal(0, module.Position);
    }

    [Fact]
    public void AssignedValues_ShouldBeReadBackCorrectly()
    {
        Module module = new Module();
        module.CanvasId = 42;
        module.CourseId = 7;
        module.Name = "Inleiding";
        module.Position = 3;

        Assert.Equal(42, module.CanvasId);
        Assert.Equal(7, module.CourseId);
        Assert.Equal("Inleiding", module.Name);
        Assert.Equal(3, module.Position);
    }
}
