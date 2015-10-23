// #define SHOW_DERIVATION_2

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;

namespace OSPC.Reporter.Html
{
    public class HtmlReporter : IReporter
    {
        public static readonly string[] Colors = new[]
        {
            "#AA0000", "#00AA00", "#0000AA", "#00AAAA", "#AA00AA", "#AAAA00",
            "#880000", "#008800", "#000088", "#008888", "#880088", "#888800",
        };
        public static readonly RectangleF GraphRect = new RectangleF(0.0f, 0.0f, 512.0f, 256.0f);

        private string _outPath;

        public HtmlReporter(string outPath = null)
        {
            this._outPath = outPath.IfNullOrWhiteSpace(Path.Combine(".", "report"));
        }

        public void Create(List<CompareResult> results)
        {
            if (!Directory.Exists(_outPath))
            {
                Directory.CreateDirectory(_outPath);
            }

            int progressCounter = 0;
            using (var html = new StreamWriter(Path.Combine(_outPath, "index.html")))
            {
                WriteHeader(html, "OSPC");
                WriteSummaryTitle(html);

                foreach (var result in results)
                {
                    var diffName = string.Format("{0}_{1}.html", Path.GetFileNameWithoutExtension(result.A.FilePath), Path.GetFileNameWithoutExtension(result.B.FilePath)).Replace(" ", "_");

                    WriteDetail(result, diffName);
                    WriteSummaryResultLine(html, result, diffName);
                    if (++progressCounter % 100 == 0) Console.Write(".");
                }

                WriteSummaryFooter(html);
                WriteFooter(html);
                html.Flush();
            }
            Console.WriteLine();

            CreateTokenGraph(results);
            CreateTokenDetailGraph(results);
            CreatePercentGraph(results);
            CreateTokenMatchGraph(results);

            WriteStylesheet();
        }

        #region Graphs
        private void CreateTokenGraph(List<CompareResult> results)
        {
            GraphPane g = new GraphPane(GraphRect, "Distribution of common token", "-", "# of token");
            SetupGraph(g);

            var lst = results.Select(i => (double)i.TokenCount).OrderBy(i => i).ToArray();
            var derv_2 = lst.CalcDerv2();

            var c = g.AddCurve("Common token",
                Enumerable.Range(1, results.Count).Select(i => (double)i).ToArray(),
                lst,
                Color.Red);
            c.Symbol.IsVisible = false;

#if SHOW_DERIVATION_2
            c = g.AddCurve("Derivation 2",
                Enumerable.Range(1, derv_2.Length).Select(i => (double)i).ToArray(),
                derv_2.ToArray(),
                Color.Green);
            c.IsY2Axis = true;
            c.Symbol.IsVisible = false;
#endif

            AddLine(g, lst.Average(), Color.Blue, "Avg");
            AddLine(g, lst[derv_2.MaxIndex()], Color.Green, "POI");

            g.AxisChange();
            using (var img = g.GetImage())
            {
                img.Save(Path.Combine(_outPath, "TokenGraph.png"), ImageFormat.Png);
            }
        }

        private void CreateTokenDetailGraph(List<CompareResult> results)
        {

            GraphPane g = new GraphPane(GraphRect, "Distribution of common token - top 10%", "-", "# of Token");
            SetupGraph(g);

            int count = (int)((double)results.Count * 0.1);

            var lst = results.Select(i => (double)i.TokenCount).OrderByDescending(i => i).Take(count).OrderBy(i => i).ToArray();
            var derv_2 = lst.CalcDerv2();

            var c = g.AddCurve("Common token",
                Enumerable.Range(1, count).Select(i => (double)i).ToArray(),
                lst,
                Color.Red);
            c.Symbol.IsVisible = false;

#if SHOW_DERIVATION_2
            c = g.AddCurve("Derivation 2",
                Enumerable.Range(1, derv_2.Length).Select(i => (double)i).ToArray(),
                derv_2.ToArray(),
                Color.Green);
            c.IsY2Axis = true;
            c.Symbol.IsVisible = false;
#endif

            AddLine(g, lst.Average(), Color.Blue, "Avg");
            AddLine(g, lst[derv_2.MaxIndex()], Color.Green, "POI");

            g.AxisChange();
            using (var img = g.GetImage())
            {
                img.Save(Path.Combine(_outPath, "TokenDetailGraph.png"), ImageFormat.Png);
            }
        }

        private void CreatePercentGraph(List<CompareResult> results)
        {
            GraphPane g = new GraphPane(GraphRect, "Distribution of % similarity", "-", "% similarity");
            SetupGraph(g);

            var lst = results.SelectMany(i => new[] { 100.0 * i.SimilarityA, 100.0 * i.SimilarityB }).OrderBy(i => i).ToArray();
            var derv_2 = lst.CalcDerv2();

            var c = g.AddCurve("Similarity",
                Enumerable.Range(1, results.Count * 2).Select(i => (double)i).ToArray(),
                lst,
                Color.Red);
            c.Symbol.IsVisible = false;

#if SHOW_DERIVATION_2
            c = g.AddCurve("Derivation 2",
                Enumerable.Range(1, derv_2.Length).Select(i => (double)i).ToArray(),
                derv_2.ToArray(),
                Color.Green);
            c.IsY2Axis = true;
            c.Symbol.IsVisible = false;
#endif

            AddLine(g, lst.Average(), Color.Blue, "Avg");
            AddLine(g, lst[derv_2.MaxIndex()], Color.Green, "POI");

            g.AxisChange();
            using (var img = g.GetImage(512, 256, 72.0f))
            {
                img.Save(Path.Combine(_outPath, "PercentGraph.png"), ImageFormat.Png);
            }
        }
        private void CreateTokenMatchGraph(List<CompareResult> results)
        {
            GraphPane g = new GraphPane(GraphRect, "Distribution of token / match", "-", "Token / match");
            SetupGraph(g);

            var lst = results.Select(i => (double)i.TokenCount / (double)i.MatchCount).OrderBy(i => i).ToArray();
            var derv_2 = lst.CalcDerv2();

            var c = g.AddCurve("Token / match",
                Enumerable.Range(1, results.Count).Select(i => (double)i).ToArray(),
                lst,
                Color.Red);
            c.Symbol.IsVisible = false;

#if SHOW_DERIVATION_2
            c = g.AddCurve("Derivation 2",
                Enumerable.Range(1, derv_2.Length).Select(i => (double)i).ToArray(),
                derv_2.ToArray(),
                Color.Green);
            c.IsY2Axis = true;
            c.Symbol.IsVisible = false;
#endif

            AddLine(g, lst.Average(), Color.Blue, "Avg");
            AddLine(g, lst[derv_2.MaxIndex()], Color.Green, "POI");

            g.AxisChange();
            using (var img = g.GetImage())
            {
                img.Save(Path.Combine(_outPath, "TokenMatchGraph.png"), ImageFormat.Png);
            }
        }

        private static void AddLine(GraphPane g, double val, Color color, string label = null)
        {
            var line = new LineObj(0, val, 1, val);
            line.IsClippedToChartRect = true;
            line.Location.CoordinateFrame = CoordType.XChartFractionYScale;
            line.Line.Color = color;
            line.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            g.GraphObjList.Add(line);

            if (!string.IsNullOrWhiteSpace(label))
            {
                var text = new TextObj(label, 0.01, val);
                text.IsClippedToChartRect = true;
                text.Location.CoordinateFrame = CoordType.XChartFractionYScale;
                text.Location.AlignH = AlignH.Left;
                text.Location.AlignV = AlignV.Bottom;
                text.FontSpec.FontColor = color;
                text.FontSpec.Fill.IsVisible = false;
                text.FontSpec.Size = 18.0f;
                text.FontSpec.Border.IsVisible = false;
                g.GraphObjList.Add(text);
            }
        }

        private static void SetupGraph(ZedGraph.GraphPane g)
        {
            var color = Color.FromArgb(0xCC, 0xCC, 0xCC);

#if SHOW_DERIVATION_2
            g.Legend.IsVisible = true;
#else
            g.Legend.IsVisible = false;
#endif
            g.XAxis.IsVisible = false;
            g.Legend.Border.IsVisible = false;

            g.Border.Color = color;
            g.Chart.Border.Color = color;

            g.XAxis.MajorGrid.IsVisible = true;
            g.YAxis.MajorGrid.IsVisible = true;
            g.YAxis.MinorGrid.IsVisible = true;

            g.Title.FontSpec.Size = 18.0f;
            g.Legend.FontSpec.Size = 18.0f;
            g.XAxis.Scale.FontSpec.Size = 18.0f;
            g.YAxis.Scale.FontSpec.Size = 18.0f;
            g.XAxis.Title.FontSpec.Size = 18.0f;
            g.YAxis.Title.FontSpec.Size = 18.0f;
        }
        #endregion

        #region Details
        private void WriteDetail(CompareResult result, string diffName)
        {
            using (var html = new StreamWriter(Path.Combine(_outPath, diffName)))
            {
                WriteHeader(html, diffName);

                html.WriteLine("<h1>Details</h2>");
                html.WriteLine("<p id=\"detail-summary\">Matches: {0}<br/>Common token: {1}<br/>Token / match: {2:n2}</p>",
                    result.MatchCount,
                    result.TokenCount,
                    (double)result.TokenCount / (double)result.MatchCount);
                html.WriteLine("<p id=\"detail-back\"><a href=\"index.html\">Back to summary</a></p>");

                if (result.SimilarityA >= result.SimilarityB)
                {
                    WriteDetailA(result, html);
                    WriteDetailB(result, html);
                }
                else
                {
                    WriteDetailB(result, html);
                    WriteDetailA(result, html);
                }

                WriteFooter(html);
                html.Flush();
            }
        }

        private void WriteDetailA(CompareResult result, StreamWriter html)
        {
            html.WriteLine("<div class=\"detail-col\">");
            html.WriteLine("<h2>{0}</h2>", Path.GetFileName(result.A.FilePath));
            html.WriteLine("<div class=\"detail-submission-summary\">Similarity: {0:n2} %<br/>Token: {1}</div>",
                result.SimilarityA * 100.0,
                result.A.Tokens.Length);

            html.WriteLine("<div class=\"detail-code\">");
            using (var rd = new StreamReader(result.A.FilePath))
            {
                ColorDiff(html, result, rd, m => m.TokensA);
            }
            html.WriteLine("</div></div>");
        }

        private void WriteDetailB(CompareResult result, StreamWriter html)
        {
            html.WriteLine("<div class=\"detail-col\">");
            html.WriteLine("<h2>{0}</h2>", Path.GetFileName(result.B.FilePath));
            html.WriteLine("<div class=\"detail-submission-summary\">Similarity: {0:n2} %<br/>Token: {1}</div>",
                result.SimilarityB * 100.0,
                result.B.Tokens.Length);

            html.WriteLine("<div class=\"detail-code\">");
            using (var rd = new StreamReader(result.B.FilePath))
            {
                ColorDiff(html, result, rd, m => m.TokensB);
            }
            html.WriteLine("</div></div>");
        }

        private void ColorDiff(StreamWriter diffHtml, CompareResult result, StreamReader rd, Func<Match, LinkedList<Token>> tokenExtractor)
        {
            var content = rd.ReadToEnd();
            int idx = 0;
            var currentMatch = result.Matches.OrderBy(m => tokenExtractor(m).First.Value.Start).ToList().GetEnumerator();
            currentMatch.MoveNext();
            while (idx < content.Length)
            {
                int start = currentMatch.Current != null ? tokenExtractor(currentMatch.Current).First.Value.Start : content.Length;
                if (start > idx)
                {
                    // not in match
                    diffHtml.Write(System.Web.HttpUtility.HtmlEncode(content.Substring(idx, start - idx)));
                    idx = start;
                }
                else
                {
                    int end = tokenExtractor(currentMatch.Current).Last.Value.End;
                    // in match
                    diffHtml.Write("<span style=\"font-weight: bold;color: {0}\">", Colors[currentMatch.Current.Index % Colors.Length]);

                    diffHtml.Write(System.Web.HttpUtility.HtmlEncode(content.Substring(idx, end - idx + 1)));
                    diffHtml.Write("</span>");
                    currentMatch.MoveNext();
                    idx = end + 1;
                }
            }
        }
        #endregion

        #region Summay
        private static void WriteSummaryTitle(StreamWriter html)
        {
            html.WriteLine("<h1>Open Software Plagiarism Checker</h1>");
            html.WriteLine("<p id=\"summary-info\">Created on: {0}</p>", DateTime.Now);
            html.WriteLine("<table id=\"summary-table\">");
            html.WriteLine(@"<tr id=""summary-table-header"">
    <th>A</th>
    <th>% A</th>
    <th>B</th>
    <th>% B</th>
    <th>Matches</th>
    <th>Tokens</th>
    <th>Tokens/Match</th>
    <th>Diff</th>
</tr>");
        }
        private void WriteSummaryResultLine(StreamWriter html, CompareResult result, string diffName)
        {
            var format = @"<tr class=""summary-table-row"">
    <td><a href=""{6}"">{0}</a></td>
    <td class=""right"">{1:n2}</td>
    <td><a href=""{7}"">{2}</a></td>
    <td class=""right"">{3:n2}</td>
    <td class=""right"">{4}</td>
    <td class=""right"">{5}</td>
    <td class=""right"">{8:n2}</td>
    <td><a href=""{9}"">Diff</a></td>
</tr>";
            if (result.SimilarityA >= result.SimilarityB)
            {
                html.WriteLine(format,
                        result.A.FilePath.MaxLength(17, "...", true),
                        100.0 * result.SimilarityA,
                        result.B.FilePath.MaxLength(17, "...", true),
                        100.0 * result.SimilarityB,
                        result.MatchCount,
                        result.TokenCount,
                        result.A.FilePath,
                        result.B.FilePath,
                        (double)result.TokenCount / (double)result.MatchCount,
                        diffName);
            }
            else
            {
                // This is a little lie, as it does not match the table headers.
                // But to the user the lables A and B are not important
                html.WriteLine(format,
                        result.B.FilePath.MaxLength(17, "...", true),
                        100.0 * result.SimilarityB,
                        result.A.FilePath.MaxLength(17, "...", true),
                        100.0 * result.SimilarityA,
                        result.MatchCount,
                        result.TokenCount,
                        result.B.FilePath,
                        result.A.FilePath,
                        (double)result.TokenCount / (double)result.MatchCount,
                        diffName);
            }
        }
        private static void WriteSummaryFooter(StreamWriter html)
        {
            html.WriteLine("</table>");
            html.WriteLine("<div id=\"summary-graph-pane\">");
            html.WriteLine("<img src=\"TokenGraph.png\" />");
            html.WriteLine("<br/>");
            html.WriteLine("<img src=\"TokenDetailGraph.png\" />");
            html.WriteLine("<br/>");
            html.WriteLine("<img src=\"TokenMatchGraph.png\" />");
            html.WriteLine("<br/>");
            html.WriteLine("<img src=\"PercentGraph.png\" />");
            html.WriteLine("</div>");
        }
        #endregion

        #region Commmon
        private static void WriteHeader(StreamWriter html, string title)
        {
            html.WriteLine(@"<html>
<head>
    <meta charset=""UTF-8"">
    <title>{0}</title>
    <link href=""style.css"" rel=""stylesheet"" />
</head>
<body>", title);
        }

        private void WriteStylesheet()
        {
            using (var sw = new StreamWriter(Path.Combine(_outPath, "style.css")))
            {
                sw.Write(Html.Resources.style);
            }
        }

        private static void WriteFooter(StreamWriter html)
        {
            html.WriteLine("</body></html>");
        }
        #endregion
    }
}
