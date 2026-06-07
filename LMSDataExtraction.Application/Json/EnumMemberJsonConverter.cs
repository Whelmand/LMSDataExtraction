using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LMSDataExtraction.Application.Json;

// Generic JsonConverter that serializes enum members to the string in their
// [EnumMember(Value = "...")] attribute, and vice versa. Needed because
// JsonStringEnumConverter does not support EnumMember attributes and we
// have wire values with spaces and '&' (e.g. "Manage&Control").
public class EnumMemberJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    private static readonly Dictionary<TEnum, string> EnumToString = BuildEnumToString();
    private static readonly Dictionary<string, TEnum> StringToEnum = BuildStringToEnum();

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a string for enum {typeof(TEnum).Name}, got {reader.TokenType}.");
        }

        string? value = reader.GetString();
        if (value is null)
        {
            throw new JsonException($"Null is not a valid value for enum {typeof(TEnum).Name}.");
        }

        if (StringToEnum.TryGetValue(value, out TEnum result))
        {
            return result;
        }

        throw new JsonException($"'{value}' is not a valid value for enum {typeof(TEnum).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (EnumToString.TryGetValue(value, out string? mapped))
        {
            writer.WriteStringValue(mapped);
            return;
        }

        writer.WriteStringValue(value.ToString());
    }

    private static Dictionary<TEnum, string> BuildEnumToString()
    {
        var result = new Dictionary<TEnum, string>();
        foreach (TEnum value in Enum.GetValues<TEnum>())
        {
            result[value] = ResolveWireValue(value);
        }

        return result;
    }

    private static Dictionary<string, TEnum> BuildStringToEnum()
    {
        var result = new Dictionary<string, TEnum>(StringComparer.Ordinal);
        foreach (TEnum value in Enum.GetValues<TEnum>())
        {
            result[ResolveWireValue(value)] = value;
        }

        return result;
    }

    private static string ResolveWireValue(TEnum value)
    {
        string name = value.ToString();
        FieldInfo? field = typeof(TEnum).GetField(name);
        EnumMemberAttribute? attribute = field?.GetCustomAttribute<EnumMemberAttribute>();
        return attribute?.Value ?? name;
    }
}
