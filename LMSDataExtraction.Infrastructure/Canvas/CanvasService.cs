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
    private const string OutcomeGroupsCacheKeyPrefix = "canvas:outcomegroups:";
    private const string OutcomesCacheKeyPrefix = "canvas:outcomes:";
    private const string PeerReviewsCacheKeyPrefix = "canvas:peerreviews:";
    private const string AnnouncementsCacheKeyPrefix = "canvas:announcements:";
    private const string GradesCacheKeyPrefix = "canvas:grades:";
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

    public async Task<IEnumerable<CanvasOutcomeGroupDto>> GetOutcomeGroupsAsync(string token, int courseCanvasId)
    {
        string cacheKey = OutcomeGroupsCacheKeyPrefix + token + ":" + courseCanvasId;

        if (_cache.TryGetValue(cacheKey, out IEnumerable<CanvasOutcomeGroupDto>? cachedGroups))
        {
            if (cachedGroups != null)
            {
                return cachedGroups;
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _httpClient.GetAsync(_baseUrl + "/courses/" + courseCanvasId + "/outcome_groups");
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        IEnumerable<CanvasOutcomeGroupDto>? groups = JsonSerializer.Deserialize<IEnumerable<CanvasOutcomeGroupDto>>(json, options);

        if (groups == null)
        {
            return new List<CanvasOutcomeGroupDto>();
        }

        _cache.Set(cacheKey, groups, CacheDuration);

        return groups;
    }

    public async Task<IEnumerable<CanvasOutcomeDto>> GetOutcomesAsync(string token, int courseCanvasId)
    {
        string cacheKey = OutcomesCacheKeyPrefix + token + ":" + courseCanvasId;

        if (_cache.TryGetValue(cacheKey, out IEnumerable<CanvasOutcomeDto>? cachedOutcomes))
        {
            if (cachedOutcomes != null)
            {
                return cachedOutcomes;
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // outcome_group_links returns all outcomes linked under the course's outcome groups (flattened).
        HttpResponseMessage response = await _httpClient.GetAsync(_baseUrl + "/courses/" + courseCanvasId + "/outcome_group_links?outcome_style=full");
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        using JsonDocument document = JsonDocument.Parse(json);

        List<CanvasOutcomeDto> outcomes = new List<CanvasOutcomeDto>();

        foreach (JsonElement element in document.RootElement.EnumerateArray())
        {
            if (element.TryGetProperty("outcome", out JsonElement outcomeElement) == true)
            {
                CanvasOutcomeDto? outcome = JsonSerializer.Deserialize<CanvasOutcomeDto>(outcomeElement.GetRawText(), options);

                if (outcome != null)
                {
                    outcomes.Add(outcome);
                }
            }
        }

        _cache.Set(cacheKey, outcomes, CacheDuration);

        return outcomes;
    }

    public async Task<IEnumerable<CanvasPeerReviewDto>> GetPeerReviewsAsync(string token, int courseCanvasId, int assignmentCanvasId)
    {
        string cacheKey = PeerReviewsCacheKeyPrefix + token + ":" + courseCanvasId + ":" + assignmentCanvasId;

        if (_cache.TryGetValue(cacheKey, out IEnumerable<CanvasPeerReviewDto>? cachedReviews))
        {
            if (cachedReviews != null)
            {
                return cachedReviews;
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _httpClient.GetAsync(
            _baseUrl + "/courses/" + courseCanvasId + "/assignments/" + assignmentCanvasId + "/peer_reviews"
        );
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        IEnumerable<CanvasPeerReviewDto>? reviews = JsonSerializer.Deserialize<IEnumerable<CanvasPeerReviewDto>>(json, options);

        if (reviews == null)
        {
            return new List<CanvasPeerReviewDto>();
        }

        _cache.Set(cacheKey, reviews, CacheDuration);

        return reviews;
    }

    public async Task<IEnumerable<CanvasAnnouncementDto>> GetAnnouncementsAsync(string token, int courseCanvasId)
    {
        string cacheKey = AnnouncementsCacheKeyPrefix + token + ":" + courseCanvasId;

        if (_cache.TryGetValue(cacheKey, out IEnumerable<CanvasAnnouncementDto>? cachedAnnouncements))
        {
            if (cachedAnnouncements != null)
            {
                return cachedAnnouncements;
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _httpClient.GetAsync(
            _baseUrl + "/announcements?context_codes[]=course_" + courseCanvasId
        );
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        IEnumerable<CanvasAnnouncementDto>? announcements = JsonSerializer.Deserialize<IEnumerable<CanvasAnnouncementDto>>(json, options);

        if (announcements == null)
        {
            return new List<CanvasAnnouncementDto>();
        }

        _cache.Set(cacheKey, announcements, CacheDuration);

        return announcements;
    }

    public async Task<IEnumerable<CanvasGradeDto>> GetGradesAsync(string token, int courseCanvasId)
    {
        string cacheKey = GradesCacheKeyPrefix + token + ":" + courseCanvasId;

        if (_cache.TryGetValue(cacheKey, out IEnumerable<CanvasGradeDto>? cachedGrades))
        {
            if (cachedGrades != null)
            {
                return cachedGrades;
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Submissions endpoint returns full grade data (score, grade, graded_at, late/missing/excused) for the current user.
        HttpResponseMessage response = await _httpClient.GetAsync(
            _baseUrl + "/courses/" + courseCanvasId + "/students/submissions?student_ids[]=self&include[]=assignment"
        );
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        IEnumerable<CanvasGradeDto>? grades = JsonSerializer.Deserialize<IEnumerable<CanvasGradeDto>>(json, options);

        if (grades == null)
        {
            return new List<CanvasGradeDto>();
        }

        _cache.Set(cacheKey, grades, CacheDuration);

        return grades;
    }
}
