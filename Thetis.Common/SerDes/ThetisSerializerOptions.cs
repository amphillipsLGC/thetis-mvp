using System.Text.Json;

namespace Thetis.Common.SerDes;

public static class ThetisSerializerOptions
{
    public static JsonSerializerOptions PreserveReferenceHandler { get; } = new JsonSerializerOptions
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
    };
}