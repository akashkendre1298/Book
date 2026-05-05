using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookTracker.Tests;

public static class TestJsonOptions
{
    public static JsonSerializerOptions Options { get; }

    static TestJsonOptions()
    {
        Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        Options.Converters.Add(new JsonStringEnumConverter());
    }
}
