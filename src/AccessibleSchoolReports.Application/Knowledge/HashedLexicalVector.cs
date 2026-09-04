using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AccessibleSchoolReports.Application.Knowledge;

/// <summary>
/// Deterministic hashed bag-of-words vector. Used for local retrieval so Ask
/// does not require embedding every catalog chunk through an external API.
/// </summary>
public static partial class HashedLexicalVector
{
    public const int DefaultDimensions = 256;

    public static float[] Embed(string? text, int dimensions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions);
        var vector = new float[dimensions];
        var tokens = Tokenize(text);
        if (tokens.Count == 0)
        {
            return vector;
        }

        for (var index = 0; index < tokens.Count; index++)
        {
            Add(vector, tokens[index], 1f);
            if (index + 1 < tokens.Count)
            {
                Add(vector, tokens[index] + " " + tokens[index + 1], 1.25f);
            }
        }

        var norm = Math.Sqrt(vector.Sum(value => value * value));
        if (norm == 0)
        {
            return vector;
        }

        for (var index = 0; index < vector.Length; index++)
        {
            vector[index] = (float)(vector[index] / norm);
        }

        return vector;
    }

    private static void Add(float[] vector, string token, float weight)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var bucket = (int)(BitConverter.ToUInt32(bytes, 0) % (uint)vector.Length);
        vector[bucket] += weight;
    }

    private static List<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return TokenPattern().Matches(text.ToLowerInvariant())
            .Select(match => match.Value)
            .Where(token => token.Length >= 2)
            .ToList();
    }

    [GeneratedRegex(@"[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*", RegexOptions.CultureInvariant)]
    private static partial Regex TokenPattern();
}
