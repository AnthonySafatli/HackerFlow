using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

public sealed class DocumentTextExtractor : IDocumentTextExtractor
{
    private static readonly XNamespace TextNs = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";

    public async Task<string> ExtractTextAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        return ext switch
        {
            ".pdf" => ExtractPdf(stream),
            ".odt" or ".ods" or ".odp" => await ExtractOdfAsync(stream, ct),
            _ => throw new NotSupportedException($"Unsupported file type: {ext}")
        };
    }

    private static string ExtractPdf(Stream stream)
    {
        using var doc = PdfDocument.Open(stream);
        return string.Join("\n\n", doc.GetPages().Select(p => ContentOrderTextExtractor.GetText(p)));
    }

    private static async Task<string> ExtractOdfAsync(Stream stream, CancellationToken ct)
    {
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var entry = zip.GetEntry("content.xml")
            ?? throw new InvalidDataException("Not a valid OpenDocument file (content.xml missing).");

        await using var xml = entry.Open();
        var doc = await XDocument.LoadAsync(xml, LoadOptions.None, ct);

        var paragraphs = doc.Descendants()
            .Where(e => e.Name == TextNs + "p" || e.Name == TextNs + "h")
            .Select(e =>
            {
                var sb = new StringBuilder();
                AppendText(e, sb);
                return sb.ToString();
            });

        return string.Join("\n", paragraphs);
    }

    // Handles ODF whitespace elements that XElement.Value would drop
    private static void AppendText(XElement element, StringBuilder sb)
    {
        foreach (var node in element.Nodes())
        {
            switch (node)
            {
                case XText t:
                    sb.Append(t.Value);
                    break;
                case XElement e when e.Name == TextNs + "s":
                    sb.Append(' ', (int?)e.Attribute(TextNs + "c") ?? 1);
                    break;
                case XElement e when e.Name == TextNs + "tab":
                    sb.Append('\t');
                    break;
                case XElement e when e.Name == TextNs + "line-break":
                    sb.Append('\n');
                    break;
                case XElement e:
                    AppendText(e, sb);
                    break;
            }
        }
    }
}

public interface IDocumentTextExtractor
{
    Task<string> ExtractTextAsync(Stream stream, string fileName, CancellationToken ct = default);
}