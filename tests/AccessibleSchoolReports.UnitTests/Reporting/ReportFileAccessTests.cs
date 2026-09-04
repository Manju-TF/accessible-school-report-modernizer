using AccessibleSchoolReports.Application.Reporting;

namespace AccessibleSchoolReports.UnitTests.Reporting;

public sealed class ReportFileAccessTests
{
    [Fact]
    public void TryResolveDownloadPath_AcceptsPdfUnderOutputRoot()
    {
        var root = CreateTempRoot();
        try
        {
            var path = Path.Combine(root, "2025", "10701", "summary-report.pdf");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, "%PDF");

            Assert.True(ReportFileAccess.TryResolveDownloadPath(path, root, out var resolved));
            Assert.Equal(Path.GetFullPath(path), resolved);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TryResolveDownloadPath_RejectsPathOutsideOutputRoot()
    {
        var root = CreateTempRoot();
        var outside = Path.Combine(Path.GetTempPath(), "asr-outside-" + Guid.NewGuid().ToString("N"), "secret.pdf");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outside)!);
            File.WriteAllText(outside, "%PDF");

            Assert.False(ReportFileAccess.TryResolveDownloadPath(outside, root, out _));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
            Directory.Delete(Path.GetDirectoryName(outside)!, recursive: true);
        }
    }

    [Fact]
    public void TryResolveDownloadPath_RejectsNonPdfAndMissingFile()
    {
        var root = CreateTempRoot();
        try
        {
            var textPath = Path.Combine(root, "notes.txt");
            File.WriteAllText(textPath, "nope");
            Assert.False(ReportFileAccess.TryResolveDownloadPath(textPath, root, out _));
            Assert.False(ReportFileAccess.TryResolveDownloadPath(Path.Combine(root, "missing.pdf"), root, out _));
            Assert.False(ReportFileAccess.TryResolveDownloadPath(null, root, out _));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "asr-download-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}
