using LMSDataExtraction.Domain.Entities;
using Xunit;

namespace LMSDataExtraction.Tests.Entities;

public class CourseTests
{
    [Fact]
    public void NewCourse_ShouldHaveEmptyDefaults()
    {
        Course course = new Course();

        Assert.Equal(0, course.Id);
        Assert.Equal(0, course.CanvasId);
        Assert.Equal(string.Empty, course.Name);
        Assert.Equal(string.Empty, course.Description);
        Assert.NotNull(course.Modules);
        Assert.Empty(course.Modules);
        Assert.NotNull(course.Assignments);
        Assert.Empty(course.Assignments);
        Assert.NotNull(course.Activities);
        Assert.Empty(course.Activities);
    }

    [Fact]
    public void AddModule_ShouldAppearInModulesCollection()
    {
        Course course = new Course();
        course.Name = "Software Engineering";

        Module module = new Module();
        module.Name = "Introduction";
        module.Position = 1;

        course.Modules.Add(module);

        Assert.Single(course.Modules);
    }
}
