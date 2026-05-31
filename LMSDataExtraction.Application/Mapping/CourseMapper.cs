using LMSDataExtraction.Application.Dtos;

namespace LMSDataExtraction.Application.Mapping;

public static class CourseMapper
{
    public static CourseResponseDto ToResponse(CanvasCourseDto source)
    {
        CourseResponseDto response = new CourseResponseDto();
        response.CanvasId = source.Id;
        response.Name = source.Name;
        response.CourseCode = source.CourseCode;
        response.StartAt = source.StartAt;
        response.EndAt = source.EndAt;
        return response;
    }

    public static IEnumerable<CourseResponseDto> ToResponseList(IEnumerable<CanvasCourseDto> source)
    {
        List<CourseResponseDto> result = new List<CourseResponseDto>();

        foreach (CanvasCourseDto item in source)
        {
            result.Add(ToResponse(item));
        }

        return result;
    }
}
