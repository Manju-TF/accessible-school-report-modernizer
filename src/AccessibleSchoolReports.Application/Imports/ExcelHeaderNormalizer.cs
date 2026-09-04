using System.Text;

namespace AccessibleSchoolReports.Application.Imports;

public static class ExcelHeaderNormalizer
{
    public static string Normalize(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(header.Length);
        foreach (var ch in header.Trim())
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }

        return builder.ToString();
    }
}
