using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Web.Ui;

namespace AccessibleSchoolReports.UnitTests.Ui;

public sealed class KnowledgeSourceDisplayTests
{
    [Fact]
    public void DocumentName_UsesFileNameOnly()
    {
        Assert.Equal("cf200.sas", KnowledgeSourceDisplay.DocumentName(@"C:\work\legacy\sas\cf200.sas"));
        Assert.Equal("summary-report.pdf", KnowledgeSourceDisplay.DocumentName("output/2025/10701/summary-report.pdf"));
        Assert.Equal("Untitled document", KnowledgeSourceDisplay.DocumentName("   "));
    }

    [Fact]
    public void Location_HidesPhysicalPaths()
    {
        Assert.Equal("page 2", KnowledgeSourceDisplay.Location("page 2"));
        Assert.Equal("legacy/sas/cf200.sas:12", KnowledgeSourceDisplay.Location("legacy/sas/cf200.sas:12"));
        Assert.Equal("Document location", KnowledgeSourceDisplay.Location(@"C:\Users\me\output\a.pdf"));
        Assert.Equal("page 3", KnowledgeSourceDisplay.Location("/tmp/output/report.pdf page 3"));
        Assert.Equal("Document location", KnowledgeSourceDisplay.Location("../secret/notes.md"));
    }

    [Fact]
    public void RuleIdAndKind_AreSafeLabels()
    {
        Assert.Equal("CF-S-00", KnowledgeSourceDisplay.RuleId("CF-S-00"));
        Assert.Equal("Not recorded", KnowledgeSourceDisplay.RuleId(null));
        Assert.Equal("Generated report", KnowledgeSourceDisplay.DocumentKind(KnowledgeDocumentType.GeneratedReport));
        Assert.Equal("Legacy documentation", KnowledgeSourceDisplay.DocumentKind(KnowledgeDocumentType.Legacy));
    }
}
