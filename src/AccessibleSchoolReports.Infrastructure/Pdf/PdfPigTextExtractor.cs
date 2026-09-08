using System.Text;
using AccessibleSchoolReports.Application.Knowledge;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

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
                var text = ExtractPageText(page);
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

    internal static string ExtractPageText(Page page)
    {
        var letters = page.Letters
            .Where(letter => letter.Value.Length > 0 && letter.Value.Any(character => character != '\0'))
            .ToList();
        var words = letters.Count == 0
            ? []
            : new NearestNeighbourWordExtractor()
                .GetWords(letters)
                .Where(word => !string.IsNullOrWhiteSpace(word.Text) && word.Text.Any(character => character != '\0'))
                .ToList();
        if (words.Count == 0)
        {
            words = page.GetWords()
                .Where(word => !string.IsNullOrWhiteSpace(word.Text) && word.Text.Any(character => character != '\0'))
                .ToList();
        }

        if (words.Count == 0)
        {
            return letters.Count == 0
                ? string.Empty
                : string.Join(' ', letters.Select(letter => letter.Value));
        }

        var heights = words
            .Select(word => word.BoundingBox.Height)
            .Where(height => height > 0)
            .OrderBy(height => height)
            .ToList();
        var medianHeight = heights.Count == 0 ? 8 : heights[heights.Count / 2];
        var yTolerance = Math.Max(2.0, medianHeight * 0.6);
        var lines = GroupByLine(words, word => word.BoundingBox, yTolerance);
        return string.Join(
            '\n',
            lines.Select(line =>
                string.Join(' ', line.OrderBy(item => item.BoundingBox.Left).Select(item => item.Text))));
    }

    private static List<List<T>> GroupByLine<T>(
        IReadOnlyList<T> items,
        Func<T, PdfRectangle> box,
        double yTolerance)
    {
        var lines = new List<List<T>>();
        foreach (var item in items
            .OrderByDescending(value => MidY(box(value)))
            .ThenBy(value => box(value).Left))
        {
            var y = MidY(box(item));
            var current = lines.Count == 0 ? null : lines[^1];
            if (current is null)
            {
                lines.Add([item]);
                continue;
            }

            var lineY = MidY(box(current[0]));
            if (Math.Abs(lineY - y) <= yTolerance)
            {
                current.Add(item);
            }
            else
            {
                lines.Add([item]);
            }
        }

        return lines;
    }

    private static double MidY(PdfRectangle box) => (box.Bottom + box.Top) / 2;

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
