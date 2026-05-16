using System.Net.Http.Headers;
using System.Text.Json;
using LMSDataExtraction.Application.Dtos;
using LMSDataExtraction.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LMSDataExtraction.Infrastructure.Canvas;

public class CanvasService : ICanvasService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public CanvasService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["Canvas:BaseUrl"]!;
    }

    public async Task<IEnumerable<CanvasCourseDto>> GetCoursesAsync(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync(_baseUrl + "/courses");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var courses = JsonSerializer.Deserialize<IEnumerable<CanvasCourseDto>>(json, options);

        if (courses == null)
        {
            return new List<CanvasCourseDto>();
        }

        return courses;
    }
}
