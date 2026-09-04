namespace AccessibleSchoolReports.Application.Knowledge;

public static class EmbeddingSimilarity
{
    public static float Cosine(ReadOnlySpan<float> left, ReadOnlySpan<float> right)
    {
        if (left.Length == 0 || left.Length != right.Length)
        {
            return 0f;
        }

        double dot = 0;
        double leftNorm = 0;
        double rightNorm = 0;
        for (var index = 0; index < left.Length; index++)
        {
            dot += left[index] * right[index];
            leftNorm += left[index] * left[index];
            rightNorm += right[index] * right[index];
        }

        if (leftNorm == 0 || rightNorm == 0)
        {
            return 0f;
        }

        return (float)(dot / (Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm)));
    }
}
