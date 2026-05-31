using System.Net.Http.Headers;
using System.Text.Json;
using LMSDataExtraction.Application.Dtos;
using LMSDataExtraction.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace LMSDataExtraction.Infrastructure.Canvas;

public class CanvasService : ICanvasService
{
    private const string CoursesCacheKeyPrefix = "canvas:courses:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly string _baseUrl;

    public CanvasService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
        _baseUrl = configuration["Canvas:BaseUrl"]!;

        _httpClient.Timeout = RequestTimeout;
    }

    public async Task<IEnumerable<CanvasCourseDto>> GetCoursesAsync(string token)
    {
        string cacheKey = CoursesCacheKeyPrefix + token;

        if (_cache.TryGetValue(cacheKey, out IEnumerable<CanvasCourseDto>? cachedCourses))
        {
            if (cachedCourses != null)
            {
                return cachedCourses;
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _httpClient.GetAsync(_baseUrl + "/courses");
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        IEnumerable<CanvasCourseDto>? courses = JsonSerializer.Deserialize<IEnumerable<CanvasCourseDto>>(json, options);

        if (courses == null)
        {
            return new List<CanvasCourseDto>();
        }

        _cache.Set(cacheKey, courses, CacheDuration);

        return courses;
    }
}
