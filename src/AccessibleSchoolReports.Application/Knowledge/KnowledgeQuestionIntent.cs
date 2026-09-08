namespace AccessibleSchoolReports.Application.Knowledge;

public static class KnowledgeQuestionIntent
{
    public static bool PrefersPrintedReportEvidence(string? question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return false;
        }

        var text = question.Trim();
        if (LooksLikeRuleQuestion(text))
        {
            return false;
        }

        return AsksAboutGeneratedReport(text)
            || AsksForPrintedNumbers(text)
            || AsksForPrintedArithmetic(text);
    }

    public static bool AsksForPrintedArithmetic(string? question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return false;
        }

        var text = question.Trim();
        if (LooksLikeRuleQuestion(text))
        {
            return false;
        }

        return ContainsPhrase(text, "sum")
            || ContainsPhrase(text, "add up")
            || ContainsPhrase(text, "added up")
            || ContainsPhrase(text, "total of all")
            || ContainsPhrase(text, "across all schools")
            || ContainsPhrase(text, "all schools")
            || ContainsPhrase(text, "difference")
            || ContainsPhrase(text, "differ")
            || ContainsPhrase(text, "delta")
            || ContainsPhrase(text, "minus")
            || ContainsPhrase(text, "last year")
            || ContainsPhrase(text, "last years")
            || ContainsPhrase(text, "previous year")
            || ContainsPhrase(text, "year over year")
            || ContainsPhrase(text, "year-over-year")
            || ContainsPhrase(text, "compare")
            || ContainsPhrase(text, "comparison")
            || ContainsPhrase(text, "versus")
            || ContainsPhrase(text, " vs ")
            || ContainsPhrase(text, " vs.");
    }

    internal static bool LooksLikeRuleQuestion(string question) =>
        ContainsPhrase(question, "cf-")
        || ContainsPhrase(question, "ss-")
        || ContainsPhrase(question, ".sas")
        || ContainsPhrase(question, "jobcat1")
        || ContainsPhrase(question, "salftperm")
        || ContainsPhrase(question, "n ge 5")
        || ContainsPhrase(question, "analvar")
        || ContainsPhrase(question, "characterized")
        || ContainsPhrase(question, "when are")
        || ContainsPhrase(question, "when is")
        || (ContainsPhrase(question, "how is")
            && (ContainsPhrase(question, "calculated")
                || ContainsPhrase(question, "handled")
                || ContainsPhrase(question, "counted")
                || ContainsPhrase(question, "recode")));

    internal static bool AsksAboutGeneratedReport(string question) =>
        ContainsPhrase(question, "generated report")
        || ContainsPhrase(question, "summary report")
        || ContainsPhrase(question, "this report")
        || ContainsPhrase(question, "printed")
        || ContainsPhrase(question, "class of")
        || ContainsPhrase(question, "compare")
        || ContainsPhrase(question, "comparison")
        || ContainsPhrase(question, "versus")
        || ContainsPhrase(question, " vs ")
        || ContainsPhrase(question, " vs.")
        || ContainsPhrase(question, "school-wise")
        || ContainsPhrase(question, "school by school")
        || ContainsPhrase(question, "year-wise")
        || ContainsPhrase(question, "year over year")
        || ContainsPhrase(question, "across schools")
        || ContainsPhrase(question, "across years")
        || ContainsPhrase(question, "i can view");

    internal static bool AsksForPrintedNumbers(string question) =>
        ContainsPhrase(question, "total reported")
        || ContainsPhrase(question, "how many")
        || ContainsPhrase(question, "headcount")
        || ContainsPhrase(question, "head count")
        || ContainsPhrase(question, "number reported")
        || ContainsPhrase(question, "counts")
        || ContainsPhrase(question, "count ")
        || question.EndsWith("count", StringComparison.OrdinalIgnoreCase)
        || ContainsPhrase(question, "percent")
        || ContainsPhrase(question, "percentage")
        || ContainsPhrase(question, "salary")
        || ContainsPhrase(question, "salaries")
        || ContainsPhrase(question, "median")
        || ContainsPhrase(question, "25th")
        || ContainsPhrase(question, "75th")
        || ContainsPhrase(question, "percentile")
        || ContainsPhrase(question, "employed")
        || ContainsPhrase(question, "employment")
        || ContainsPhrase(question, "unemployed")
        || ContainsPhrase(question, "enrolled")
        || ContainsPhrase(question, "gender")
        || ContainsPhrase(question, "women")
        || ContainsPhrase(question, "race")
        || ContainsPhrase(question, "people of color")
        || ContainsPhrase(question, "employer")
        || ContainsPhrase(question, "private practice")
        || ContainsPhrase(question, "clerk")
        || ContainsPhrase(question, "sector")
        || ContainsPhrase(question, "all printed")
        || ContainsPhrase(question, "every printed")
        || ContainsPhrase(question, "list the printed")
        || ContainsPhrase(question, "list every printed")
        || ContainsPhrase(question, "what numbers")
        || ContainsPhrase(question, "what figures");

    private static bool ContainsPhrase(string question, string phrase) =>
        question.Contains(phrase, StringComparison.OrdinalIgnoreCase);
}
