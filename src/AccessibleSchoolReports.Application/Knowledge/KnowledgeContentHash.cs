using System.Security.Cryptography;

namespace AccessibleSchoolReports.Application.Knowledge;

public static class KnowledgeContentHash
{
    public static string Sha256Hex(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
