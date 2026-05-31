using LMSDataExtraction.Application.Dtos;
using Xunit;

namespace LMSDataExtraction.Tests.Dtos;

public class CanvasCourseDtoTests
{
    [Fact]
    public void NewDto_ShouldHaveEmptyDefaults()
    {
        CanvasCourseDto dto = new CanvasCourseDto();

        Assert.Equal(0, dto.Id);
        Assert.Equal(string.Empty, dto.Name);
        Assert.Equal(string.Empty, dto.CourseCode);
        Assert.Null(dto.StartAt);
        Assert.Null(dto.EndAt);
    }
}
