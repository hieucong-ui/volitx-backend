using Aspose.Words;
using Aspose.Words.Loading;
using Aspose.Words.Saving;
using Microsoft.Playwright;
using Voltix.Domain.ValueObjects;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace Voltix.Application.Pdf
{
    public class EContractPdf
    {
        private static readonly Regex Placeholder = new(@"\{\{\s*([A-Za-z0-9\._]+)\s*(?:(?::\s*([^}]+))|\|\s*date\.format\s*['""]([^'""]+)['""])?\s*\}\}", RegexOptions.Compiled);
        public static AnchorBox FindAnchorBox(byte[] pdfBytes, string anchorText)
        {
            using var ms = new MemoryStream(pdfBytes);
            using var doc = PdfDocument.Open(ms);
            int lastPgae = doc.NumberOfPages;
            var page = doc.GetPage(lastPgae);

            foreach (var word in page.GetWords())
            {
                if (word.Text.Contains(anchorText, StringComparison.Ordinal))
                {
                    var bbox = word.BoundingBox;
                    return new AnchorBox
                    {
                        Page = lastPgae,
                        Top = bbox.Top,
                        Bottom = bbox.Bottom,
                        Left = bbox.Left,
                        Right = bbox.Right
                    };
                }
            }

            throw new InvalidOperationException($"Cannot find anchor text '{anchorText}' in pdf.");
        }

        public static async Task<byte[]> RenderAsync(string html)
        {
            try
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });

                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    Locale = "vi-VN",
                    ViewportSize = new ViewportSize { Width = 1240, Height = 1754 },
                    DeviceScaleFactor = 1.0f
                });

                var page = await context.NewPageAsync();

                await page.SetContentAsync(html, new PageSetContentOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });

                await page.AddStyleTagAsync(new PageAddStyleTagOptions
                {
                    Content = @"
                @page {
                    size: A4;
                    margin: 15mm 12mm 12mm 12mm;
                }
                body {
                    font-family: 'Times New Roman', DejaVu Serif, serif;
                    font-size: 13pt;
                    line-height: 1.5;
                    color: #000;
                    -webkit-print-color-adjust: exact !important;
                    print-color-adjust: exact !important;
                    background: white;
                }
                h1, h2, h3 {
                    text-align: center;
                    font-weight: bold;
                }
                table {
                    width: 100%;
                    border-collapse: collapse;
                }
                td, th {
                    padding: 4px 6px;
                    vertical-align: top;
                }
                "
                });

                var pdf = await page.PdfAsync(new PagePdfOptions
                {
                    Format = "A4",
                    PrintBackground = true,
                    PreferCSSPageSize = true,
                    Margin = new Margin
                    {
                        Top = "15mm",
                        Bottom = "12mm",
                        Left = "12mm",
                        Right = "12mm"
                    },
                    DisplayHeaderFooter = true,
                    HeaderTemplate = @"<div style='font-size:10px; text-align:center; width:100%;'>
                                    <span class='title'></span>
                                </div>",
                    FooterTemplate = @"<div style='font-size:10px; text-align:center; width:100%; color:#666;'>
                                    <span class='pageNumber'></span> / <span class='totalPages'></span>
                               </div>"
                });

                return pdf;
            }
            catch (PlaywrightException ex) when (ex.Message.Contains("Executable doesn't exist"))
            {
                await Task.Run(() => Program.Main(new[] { "install", "chromium" }));
                return await RenderAsync(html);
            }
        }



        public static string ReplacePlaceholders(string html, Object values, bool htmlEncode = false)
        {
            return Placeholder.Replace(html, m =>
            {
                var path = m.Groups[1].Value.Trim();
                var format = m.Groups[2].Success ? m.Groups[2].Value.Trim()
                           : m.Groups[3].Success ? m.Groups[3].Value.Trim()
                           : null;

                var value = ResolvePath(values, path);
                if (value == null)
                {
                    return string.Empty;
                }

                string text;
                if (value is IFormattable fo && !string.IsNullOrWhiteSpace(format))
                {
                    text = fo.ToString(format, CultureInfo.InvariantCulture) ?? string.Empty;
                }
                else
                {
                    text = value?.ToString() ?? string.Empty;
                }

                return htmlEncode ? WebUtility.HtmlEncode(text) : text;
            });
        }

        private static object? ResolvePath(object root, string path)
        {
            var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            object? current = root;

            foreach (var segment in segments)
            {
                if (current == null) return null;
                if (current is IDictionary<string, object?> dict)
                {
                    if (!dict.TryGetValue(segment, out current))
                    {
                        var joined = string.Join(".", segments);
                        return dict.TryGetValue(joined, out var val) ? val : null;
                    }
                    continue;
                }
                var type = current?.GetType();

                var prop = type?.GetProperty(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null)
                {
                    current = prop.GetValue(current);
                    continue;
                }

                var field = type?.GetField(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field != null)
                {
                    current = field.GetValue(current);
                    continue;
                }

                return null;
            }

            return current;
        }

        public static Dictionary<string, AnchorBox> FindAnchors(byte[] pdfBytes, params string[] anchors)
        {
            var result = new Dictionary<string, AnchorBox>(StringComparer.Ordinal);
            using var ms = new MemoryStream(pdfBytes);
            using var doc = PdfDocument.Open(ms);

            var remaining = new HashSet<string>(anchors, StringComparer.Ordinal);

            for (int pageNumber = 1; pageNumber <= doc.NumberOfPages && remaining.Count > 0; pageNumber++)
            {
                var page = doc.GetPage(pageNumber);
                foreach (var word in page.GetWords())
                {
                    foreach (var token in remaining.ToArray())
                    {
                        if (word.Text.Contains(token, StringComparison.Ordinal))
                        {
                            var bound = word.BoundingBox;
                            result[token] = new AnchorBox
                            {
                                Page = pageNumber,
                                Top = bound.Top,
                                Bottom = bound.Bottom,
                                Left = bound.Left,
                                Right = bound.Right
                            };
                            remaining.Remove(token);
                            if (remaining.Count == 0) break;
                        }
                    }
                    if (remaining.Count == 0) break;
                }
            }

            if (remaining.Count > 0)
            {
                var missing = string.Join(", ", remaining);
                throw new InvalidOperationException($"Cannot find anchor text(s): {missing} in pdf.");
            }

            return result;
        }

        public static string InjectStyle(string html, string css)
        {
            const string marker = "</head>";
            var style = $"<style>\n{css}\n</style>\n";
            var idx = html.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            return idx >= 0 ? html.Insert(idx, style) : style + html;
        }

        public static (string pos, int pageSign) GetVnptEContractPositionSmart(
            byte[] pdfBytes, AnchorBox anchor,
            double width = 170, double height = 90,
            double offsetY = 36, double margin = 18,
            double xAdjust = 0)
        {
            using var ms = new MemoryStream(pdfBytes);
            using var doc = PdfDocument.Open(ms);

            var page = doc.GetPage(anchor.Page);
            int lastPage = doc.NumberOfPages;

            double pw = page.Width;
            double ph = page.Height;

            double candidateLlx = Math.Clamp(anchor.Left + xAdjust, margin, pw - margin - width);
            double candidateLly = anchor.Bottom - offsetY - height;

            bool enoughSpaceSamePage = candidateLly >= margin;

            if (enoughSpaceSamePage)
            {
                var llx = candidateLlx;
                var lly = candidateLly;
                var pos1 = $"{(int)llx},{(int)lly},{(int)(llx + width)},{(int)(lly + height)}";
                return (pos1, anchor.Page);
            }

            if (anchor.Page < lastPage)
            {
                var nextPage = doc.GetPage(anchor.Page + 1);
                double npw = nextPage.Width;
                double nph = nextPage.Height;

                double llx = Math.Clamp(anchor.Left + xAdjust, margin, npw - margin - width);
                double lly = Math.Max(nph - margin - height - 36, margin);

                var pos2 = $"{(int)llx},{(int)lly},{(int)(llx + width)},{(int)(lly + height)}";
                return (pos2, anchor.Page + 1);
            }

            {
                double llx = candidateLlx;
                double lly = margin;
                var pos3 = $"{(int)llx},{(int)lly},{(int)(llx + width)},{(int)(lly + height)}";
                return (pos3, anchor.Page);
            }
        }
    }
}