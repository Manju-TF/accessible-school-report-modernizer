export interface SuggestionGroup {
  title: string;
  hint: string;
  questions: string[];
}

const general: SuggestionGroup = {
  title: "General",
  hint: "How the project and assistant work.",
  questions: [
    "What does the Knowledge Assistant search?",
    "How do I ask about one generated report?",
    "What year chrome does the generated report use?",
    "What does a printed period mean on the report?",
    "Which SAS program generates the report?",
  ],
};

const thisReport: SuggestionGroup = {
  title: "This report",
  hint: "Printed values from the open generated PDF only.",
  questions: [
    "What is Total Reported in this report?",
    "List every printed count in this report.",
    "What does this report say about employment?",
    "What gender counts are printed in this report?",
    "What does this report say about race?",
    "What employer types are printed in this report?",
    "What salary statistics are printed in this report?",
  ],
};

const allReports: SuggestionGroup = {
  title: "All generated reports",
  hint: "Printed values and totals from generated PDFs you are allowed to view.",
  questions: [
    "What is the sum of Total Reported across generated reports I can view?",
    "What is the sum of Total Reported for last year in generated reports I can view?",
    "What is the difference in Total Reported between Class of 2025 and last year in generated reports I can view?",
    "Compare Total Reported across generated reports I can view.",
    "What Total Reported values appear in generated reports I can view?",
    "What do generated reports say about employment?",
    "List printed employment counts in generated reports I can view?",
    "What gender counts appear in generated reports I can view?",
    "What race counts appear in generated reports I can view?",
    "What employer types appear in generated reports I can view?",
  ],
};

const compareReports: SuggestionGroup = {
  title: "Compare reports",
  hint: "Sums, comparisons, and differences of printed values you are allowed to view.",
  questions: [
    "Compare Total Reported across generated reports I can view.",
    "What is the difference in Total Reported between Class of 2025 and last year in generated reports I can view?",
    "What is the sum of Total Reported for last year in generated reports I can view?",
    "Compare employment figures school by school in generated reports I can view.",
    "Compare year-over-year printed values for the same school in generated reports I can view.",
    "Compare Class of 2025 reports for schools I can view.",
  ],
};

export function suggestionGroups(reportScoped: boolean): SuggestionGroup[] {
  return reportScoped ? [thisReport] : [allReports, compareReports, general];
}
