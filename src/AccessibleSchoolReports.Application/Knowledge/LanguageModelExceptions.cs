namespace AccessibleSchoolReports.Application.Knowledge;

public sealed class LanguageModelConfigurationException : InvalidOperationException
{
    public LanguageModelConfigurationException(string message)
        : base(message)
    {
    }
}

public sealed class LanguageModelTimeoutException : TimeoutException
{
    public LanguageModelTimeoutException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}

public sealed class LanguageModelProviderException : InvalidOperationException
{
    public LanguageModelProviderException(string message, int? statusCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int? StatusCode { get; }
}
