using LMSDataExtraction.Domain.Entities;
using Xunit;

namespace LMSDataExtraction.Tests.Entities;

public class AssignmentTests
{
    [Fact]
    public void NewAssignment_ShouldHaveNullableFieldsNull()
    {
        Assignment assignment = new Assignment();

        Assert.Equal(0, assignment.Id);
        Assert.Equal(0, assignment.CanvasId);
        Assert.Equal(0, assignment.CourseId);
        Assert.Equal(string.Empty, assignment.Name);
        Assert.Null(assignment.DueDate);
        Assert.Null(assignment.MaxScore);
    }

    [Fact]
    public void AssignedValues_ShouldBeReadBackCorrectly()
    {
        DateTime due = new DateTime(2026, 6, 1, 12, 0, 0);

        Assignment assignment = new Assignment();
        assignment.Name = "Final report";
        assignment.DueDate = due;
        assignment.MaxScore = 100m;

        Assert.Equal("Final report", assignment.Name);
        Assert.Equal(due, assignment.DueDate);
        Assert.Equal(100m, assignment.MaxScore);
    }
}
