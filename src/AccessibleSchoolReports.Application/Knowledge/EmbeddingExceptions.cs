namespace AccessibleSchoolReports.Application.Knowledge;

public sealed class EmbeddingConfigurationException : InvalidOperationException
{
    public EmbeddingConfigurationException(string message)
        : base(message)
    {
    }
}

public sealed class EmbeddingTimeoutException : TimeoutException
{
    public EmbeddingTimeoutException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}

public sealed class EmbeddingProviderException : InvalidOperationException
{
    public EmbeddingProviderException(string message, int? statusCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int? StatusCode { get; }
}

public sealed class EmbeddingDimensionException : InvalidOperationException
{
    public EmbeddingDimensionException(string message)
        : base(message)
    {
    }
}
