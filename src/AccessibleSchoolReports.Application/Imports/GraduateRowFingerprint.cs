using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AccessibleSchoolReports.Application.Imports;

/// <summary>
/// Stable hashes for import de-duplication. Used to reject a re-uploaded or
/// re-saved workbook. Does not collapse look-alike graduates inside one file.
/// </summary>
public static class GraduateRowFingerprint
{
    public static string Compute(
        string schoolCode,
        int? classYear,
        string? sex3,
        string? minstat,
        string? jobcat1,
        string? jobFtPt,
        string? empgen,
        string? firm1,
        string? lfjob,
        string? jobreg,
        string? locationFlag,
        string? jobst,
        string? source,
        string? time1,
        string? status,
        string? duration,
        string? schoolFund,
        decimal? salFtPerm,
        string? emptype1)
    {
        var payload = string.Join(
            '\u001f',
            Text(schoolCode),
            classYear?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            Text(sex3),
            Text(minstat),
            Text(jobcat1),
            Text(jobFtPt),
            Text(empgen),
            Text(firm1),
            Text(lfjob),
            Text(jobreg),
            Text(locationFlag),
            Text(jobst),
            Text(source),
            Text(time1),
            Text(status),
            Text(duration),
            Text(schoolFund),
            Salary(salFtPerm),
            Text(emptype1));
        return Hash(payload);
    }

    public static string ComputeSet(IEnumerable<string> rowFingerprints)
    {
        ArgumentNullException.ThrowIfNull(rowFingerprints);
        var payload = string.Join('\n', rowFingerprints.OrderBy(hash => hash, StringComparer.Ordinal));
        return Hash(payload);
    }

    private static string Text(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string Salary(decimal? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return value.Value.ToString("0.##########", CultureInfo.InvariantCulture)
            .TrimEnd('0')
            .TrimEnd('.');
    }

    private static string Hash(string payload)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
