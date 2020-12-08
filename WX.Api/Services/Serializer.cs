using System.Text.Json;
using System.Text.Json.Serialization;
using WX.Api.Abstractions;

namespace WX.Api.Services
{
    public class Serializer : ISerializer
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions =
            SetJsonSerializerOptions(new JsonSerializerOptions());

        public static Serializer Default => new Serializer();

        public static JsonSerializerOptions SetJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
        {
            jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            return jsonSerializerOptions;
        }

        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
        }

        public string Serialize(object inpput)
        {
            return JsonSerializer.Serialize(inpput, JsonSerializerOptions);
        }
    }
}