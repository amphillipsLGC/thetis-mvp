using System.Text.Json;
using System.Text.Json.Serialization;

namespace Thetis.Common.Enums;

public enum RuleOperator
{
    Equals,
    NotEquals,
    Contains,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}

public static class RuleOperatorsProvider
{
    public static List<RuleOperator> GetRuleOperators()
    {
        return Enum.GetValues<RuleOperator>().ToList();
    }
}

public class RuleOperatorJsonConverter : JsonConverter<RuleOperator>
{
    public override RuleOperator Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if(Enum.TryParse<RuleOperator>(reader.GetString(), true, out var result))
        {
            return result;
        }
        
        throw new JsonException($"Unable to convert \"{reader.GetString()}\" to {nameof(RuleOperator)}.");
    }

    public override void Write(Utf8JsonWriter writer, RuleOperator value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}