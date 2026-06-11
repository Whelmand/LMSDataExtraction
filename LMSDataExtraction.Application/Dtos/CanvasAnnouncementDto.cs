using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Dtos;

public class CanvasAnnouncementDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("author_name")]
    public string? AuthorName { get; set; }

    [JsonPropertyName("posted_at")]
    public DateTime? PostedAt { get; set; }

    [JsonPropertyName("delayed_post_at")]
    public DateTime? DelayedPostAt { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonPropertyName("context_code")]
    public string? ContextCode { get; set; }
}
