using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using LMSDataExtraction.Application.Json;

namespace LMSDataExtraction.Application.Dtos.FeedPulse;

// Smiley scale from the FeedPulse UI: three steps.
[JsonConverter(typeof(EnumMemberJsonConverter<FeedPulseRating>))]
public enum FeedPulseRating
{
    [EnumMember(Value = "sad")]
    Sad = 1,

    [EnumMember(Value = "neutral")]
    Neutral = 2,

    [EnumMember(Value = "happy")]
    Happy = 3,
}
