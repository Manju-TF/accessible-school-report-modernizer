namespace AccessibleSchoolReports.Web.Ui;

public static class AssistantSuggestions
{
    public sealed record Group(string Title, string Hint, IReadOnlyList<string> Questions);

    public static readonly Group General = new(
        "General",
        "How the project and assistant work.",
        [
            "Which SAS program generates the report?",
            "What does the Knowledge Assistant search?",
            "How do I ask about one generated report?",
            "What year chrome does the generated report use?",
            "What does a printed period mean on the report?",
        ]);

    public static readonly Group BusinessRules = new(
        "Business rules",
        "Characterized SAS rules from project documentation.",
        [
            "How is salary suppression handled?",
            "Where is CF-S-00 documented?",
            "How is median salary calculated?",
            "Which rules are ambiguous?",
            "Which rules appear to lack tests?",
        ]);

    public static readonly Group ThisReport = new(
        "This report",
        "Printed values from the open generated PDF only.",
        [
            "What does this report say about employment?",
            "What gender counts are printed in this report?",
            "What salary statistics are printed in this report?",
            "What does this report say about race?",
            "What employer types are printed in this report?",
        ]);

    public static readonly Group AllReports = new(
        "All generated reports",
        "Printed values from generated PDFs you are allowed to view.",
        [
            "What do generated reports say about employment?",
            "What gender counts appear in generated reports I can view?",
            "What salary statistics appear in generated reports I can view?",
            "What race counts appear in generated reports I can view?",
            "What employer types appear in generated reports I can view?",
        ]);

    public static IReadOnlyList<Group> ForScope(bool reportScoped) =>
        reportScoped
            ? [ThisReport]
            : [General, BusinessRules, AllReports];
}
