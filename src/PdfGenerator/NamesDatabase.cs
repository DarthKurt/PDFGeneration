using System.Text.Json.Serialization;

namespace PdfGenerator;

internal sealed record NamesDatabase(
    [property: JsonPropertyName("male")] string[]? Male,
    [property: JsonPropertyName("female")] string[]? Female,
    [property: JsonPropertyName("last")] string[]? Last);