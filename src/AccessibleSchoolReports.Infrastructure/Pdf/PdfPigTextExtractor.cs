using System.Text;
using AccessibleSchoolReports.Application.Knowledge;
using UglyToad.PdfPig;

namespace AccessibleSchoolReports.Infrastructure.Pdf;

public sealed class PdfPigTextExtractor : IPdfTextExtractor
{
    public PdfTextExtractionResult Extract(Stream pdf)
    {
        ArgumentNullException.ThrowIfNull(pdf);

        Stream source = pdf;
        MemoryStream? copy = null;
        try
        {
            if (!pdf.CanSeek)
            {
                copy = new MemoryStream();
                pdf.CopyTo(copy);
                copy.Position = 0;
                source = copy;
            }

            if (!HasPdfHeader(source))
            {
                return PdfTextExtractionResult.Invalid("The file is not a PDF.");
            }

            using var document = PdfDocument.Open(source);
            var pages = new List<PdfExtractedPage>();
            foreach (var page in document.GetPages())
            {
                var text = string.Join(' ', page.GetWords().Select(word => word.Text));
                pages.Add(new PdfExtractedPage(page.Number, text));
            }

            if (pages.Count == 0 || pages.All(page => string.IsNullOrWhiteSpace(page.Text)))
            {
                return PdfTextExtractionResult.Failed("The PDF opened but no text could be extracted.");
            }

            return PdfTextExtractionResult.Ok(pages);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return PdfTextExtractionResult.Failed(exception.Message);
        }
        finally
        {
            copy?.Dispose();
        }
    }

    private static bool HasPdfHeader(Stream stream)
    {
        var header = new byte[4];
        var read = stream.Read(header, 0, header.Length);
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return read == 4 && Encoding.ASCII.GetString(header) == "%PDF";
    }
}
