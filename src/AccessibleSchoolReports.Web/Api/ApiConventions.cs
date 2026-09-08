using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccessibleSchoolReports.Web.Api;

internal static class ApiConventions
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static bool IsFourDigitYear(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Trim().Length == 4
        && value.Trim().All(char.IsAsciiDigit);
}
