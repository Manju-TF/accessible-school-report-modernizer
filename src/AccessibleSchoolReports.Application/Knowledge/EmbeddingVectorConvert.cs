namespace AccessibleSchoolReports.Application.Knowledge;

public static class EmbeddingVectorConvert
{
    public static byte[] ToBytes(float[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var bytes = new byte[values.Length * sizeof(float)];
        Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    public static float[] ToFloats(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length % sizeof(float) != 0)
        {
            throw new ArgumentException("Embedding bytes are not a multiple of 4.", nameof(bytes));
        }

        var values = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, values, 0, bytes.Length);
        return values;
    }
}
