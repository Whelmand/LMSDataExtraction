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
    private const string ModulesCacheKeyPrefix = "canvas:modules:";
    private const string AssignmentsCacheKeyPrefix = "canvas:assignments:";
    private const string CurrentUserCacheKeyPrefix = "canvas:currentuser:";
    private const string SubmissionsCacheKeyPrefix = "canvas:submissions:";
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

    public async Task<IEnumerable<CanvasModuleDto>> GetModulesAsync(string token, int courseCanvasId)
    {
        string cacheKey = ModulesCacheKeyPrefix + token + ":" + courseCanvasId;

        if (_cache.TryGetValue(cacheKey, out IEnumerable<CanvasModuleDto>? cachedModules))
        {
            if (cachedModules != null)
            {
                return cachedModules;
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _httpClient.GetAsync(_baseUrl + "/courses/" + courseCanvasId + "/modules");
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        IEnumerable<CanvasModuleDto>? modules = JsonSerializer.Deserialize<IEnumerable<CanvasModuleDto>>(json, options);

        if (modules == null)
        {
            return new List<CanvasModuleDto>();
        }

        _cache.Set(cacheKey, modules, CacheDuration);

        return modules;
    }

    public async Task<IEnumerable<CanvasAssignmentDto>> GetAssignmentsAsync(string token, int courseCanvasId)
    {
        string cacheKey = AssignmentsCacheKeyPrefix + token + ":" + courseCanvasId;

        if (_cache.TryGetValue(cacheKey, out IEnumerable<CanvasAssignmentDto>? cachedAssignments))
        {
            if (cachedAssignments != null)
            {
                return cachedAssignments;
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _httpClient.GetAsync(_baseUrl + "/courses/" + courseCanvasId + "/assignments");
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        IEnumerable<CanvasAssignmentDto>? assignments = JsonSerializer.Deserialize<IEnumerable<CanvasAssignmentDto>>(json, options);

        if (assignments == null)
        {
            return new List<CanvasAssignmentDto>();
        }

        _cache.Set(cacheKey, assignments, CacheDuration);

        return assignments;
    }

    public async Task<CanvasUserDto> GetCurrentUserAsync(string token)
    {
        string cacheKey = CurrentUserCacheKeyPrefix + token;

        if (_cache.TryGetValue(cacheKey, out CanvasUserDto? cachedUser))
        {
            if (cachedUser != null)
            {
                return cachedUser;
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _httpClient.GetAsync(_baseUrl + "/users/self?include[]=enrollments");
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        CanvasUserDto? user = JsonSerializer.Deserialize<CanvasUserDto>(json, options);

        if (user == null)
        {
            throw new Exception("Canvas gebruiker kon niet worden opgehaald.");
        }

        _cache.Set(cacheKey, user, CacheDuration);

        return user;
    }

    public async Task<IEnumerable<CanvasSubmissionDto>> GetSubmissionsAsync(string token, int courseCanvasId)
    {
        string cacheKey = SubmissionsCacheKeyPrefix + token + ":" + courseCanvasId;

        if (_cache.TryGetValue(cacheKey, out IEnumerable<CanvasSubmissionDto>? cachedSubmissions))
        {
            if (cachedSubmissions != null)
            {
                return cachedSubmissions;
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _httpClient.GetAsync(
            _baseUrl + "/courses/" + courseCanvasId + "/students/submissions?student_ids[]=self"
        );
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        IEnumerable<CanvasSubmissionDto>? submissions = JsonSerializer.Deserialize<IEnumerable<CanvasSubmissionDto>>(json, options);

        if (submissions == null)
        {
            return new List<CanvasSubmissionDto>();
        }

        _cache.Set(cacheKey, submissions, CacheDuration);

        return submissions;
    }
}
