// #define SHOW_DERIVATION_2
// #define SINGLE_THREADED

using OSPC.Tokenizer;
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

        public string OutPath { get; private set; }

        public HtmlReporter(string outPath = null)
        {
            this.OutPath = outPath.IfNullOrWhiteSpace(Path.Combine(".", "report"));
        }

        public void Create(OSPCResult r)
        {
            if (!Directory.Exists(OutPath))
            {
                Directory.CreateDirectory(OutPath);
            }

            CreateSummaryPage(r);
            CreateDetailPages(r);
            CreateFriendFinderPage(r);

            CreateTokenGraph(r);
            CreateTokenDetailGraph(r);
            CreatePercentGraph(r);
            CreateTokenMatchGraph(r);

            WriteStylesheet();
        }

        private void CreateFriendFinderPage(OSPCResult r)
        {
            using (var html = new StreamWriter(Path.Combine(OutPath, "friendfinder.html")))
            {
                WriteHeader(html, "OSPC - FriendFinder", new TupleList<string, string>() { { "index.html", "Results" } });
                WriteFriendFinderTitle(html);

                foreach (var f in r.Friends)
                {
                    WriteFriendFinderLine(html, f);
                }

                WriteFriendFinderFooter(html);
                WriteFooter(html);
                html.Flush();
            }
        }

        private void CreateDetailPages(OSPCResult r)
        {
            int progressCounter = 0;
            object _lock = new object();
#if SINGLE_THREADED
            foreach(var result in r.Results)
#else
            Parallel.ForEach(r.Results, result =>
#endif
            {
                WriteDetail(result, GetDetailFileName(result));
                lock (_lock)
                {
                    if (++progressCounter % 100 == 0) Console.Write(".");
                }
            }
#if !SINGLE_THREADED
            );
#endif
            Console.WriteLine();
        }

        private void CreateSummaryPage(OSPCResult r)
        {
            using (var html = new StreamWriter(Path.Combine(OutPath, "index.html")))
            {
                WriteHeader(html, "OSPC", new TupleList<string, string>() { { "friendfinder.html", "Friend Finder" } });
                WriteSummaryTitle(html);

                foreach (var result in r.Results)
                {
                    WriteSummaryResultLine(html, result, GetDetailFileName(result));
                }

                WriteSummaryFooter(html);
                WriteFooter(html);
                html.Flush();
            }
        }

        #region Graphs
        private void CreateTokenGraph(OSPCResult r)
        {
            GraphPane g = new GraphPane(GraphRect, "Distribution of common token", "-", "# of token");
            SetupGraph(g);

            var lst = r.Results.Select(i => (double)i.TokenCount).OrderBy(i => i).ToArray();

            var c = g.AddCurve("Common token",
                Enumerable.Range(1, lst.Length).Select(i => (double)i).ToArray(),
                lst,
                Color.Red);
            c.Symbol.IsVisible = false;

#if SHOW_DERIVATION_2
            var derv_2 = lst.CalcDerv2();
            c = g.AddCurve("Derivation 2",
                Enumerable.Range(1, derv_2.Length).Select(i => (double)i).ToArray(),
                derv_2.ToArray(),
                Color.Green);
            c.IsY2Axis = true;
            c.Symbol.IsVisible = false;
#endif

            AddLine(g, r.AVG_TokenCount, Color.Blue, "Avg");
            AddLine(g, r.POI_TokenCount, Color.Green, "POI");

            g.AxisChange();
            using (var img = g.GetImage())
            {
                img.Save(Path.Combine(OutPath, "TokenGraph.png"), ImageFormat.Png);
            }
        }

        private void CreateTokenDetailGraph(OSPCResult r)
        {

            GraphPane g = new GraphPane(GraphRect, "Distribution of common token - top 10%", "-", "# of Token");
            SetupGraph(g);

            int count = (int)((double)r.Results.Count * 0.1);

            var lst = r.Results.Select(i => (double)i.TokenCount).OrderByDescending(i => i).Take(count).OrderBy(i => i).ToArray();

            var c = g.AddCurve("Common token",
                Enumerable.Range(1, lst.Length).Select(i => (double)i).ToArray(),
                lst,
                Color.Red);
            c.Symbol.IsVisible = false;

#if SHOW_DERIVATION_2
            var derv_2 = lst.CalcDerv2();
            c = g.AddCurve("Derivation 2",
                Enumerable.Range(1, derv_2.Length).Select(i => (double)i).ToArray(),
                derv_2.ToArray(),
                Color.Green);
            c.IsY2Axis = true;
            c.Symbol.IsVisible = false;
#endif

            AddLine(g, r.AVG_TokenCount, Color.Blue, "Avg");
            AddLine(g, r.POI_TokenCount, Color.Green, "POI");

            g.AxisChange();
            using (var img = g.GetImage())
            {
                img.Save(Path.Combine(OutPath, "TokenDetailGraph.png"), ImageFormat.Png);
            }
        }

        private void CreatePercentGraph(OSPCResult r)
        {
            GraphPane g = new GraphPane(GraphRect, "Distribution of % similarity", "-", "% similarity");
            SetupGraph(g);

            var lst = r.Results.SelectMany(i => new[] { 100.0 * i.SimilarityA, 100.0 * i.SimilarityB }).OrderBy(i => i).ToArray();

            var c = g.AddCurve("Similarity",
                Enumerable.Range(1, lst.Length).Select(i => (double)i).ToArray(),
                lst,
                Color.Red);
            c.Symbol.IsVisible = false;

#if SHOW_DERIVATION_2
            var derv_2 = lst.CalcDerv2();
            c = g.AddCurve("Derivation 2",
                Enumerable.Range(1, derv_2.Length).Select(i => (double)i).ToArray(),
                derv_2.ToArray(),
                Color.Green);
            c.IsY2Axis = true;
            c.Symbol.IsVisible = false;
#endif

            AddLine(g, 100.0 * r.AVG_Similarity, Color.Blue, "Avg");
            AddLine(g, 100.0 * r.POI_Similarity, Color.Green, "POI");

            g.AxisChange();
            using (var img = g.GetImage(512, 256, 72.0f))
            {
                img.Save(Path.Combine(OutPath, "PercentGraph.png"), ImageFormat.Png);
            }
        }
        private void CreateTokenMatchGraph(OSPCResult r)
        {
            GraphPane g = new GraphPane(GraphRect, "Distribution of token / match", "-", "Token / match");
            SetupGraph(g);

            var lst = r.Results.Select(i => (double)i.TokenCount / (double)i.MatchCount).OrderBy(i => i).ToArray();

            var c = g.AddCurve("Token / match",
                Enumerable.Range(1, lst.Length).Select(i => (double)i).ToArray(),
                lst,
                Color.Red);
            c.Symbol.IsVisible = false;

#if SHOW_DERIVATION_2
            var derv_2 = lst.CalcDerv2();
            c = g.AddCurve("Derivation 2",
                Enumerable.Range(1, derv_2.Length).Select(i => (double)i).ToArray(),
                derv_2.ToArray(),
                Color.Green);
            c.IsY2Axis = true;
            c.Symbol.IsVisible = false;
#endif

            AddLine(g, r.AVG_TokenPerMatch, Color.Blue, "Avg");
            AddLine(g, r.POI_TokenPerMatch, Color.Green, "POI");

            g.AxisChange();
            using (var img = g.GetImage())
            {
                img.Save(Path.Combine(OutPath, "TokenMatchGraph.png"), ImageFormat.Png);
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
            using (var html = new StreamWriter(Path.Combine(OutPath, diffName)))
            {
                WriteHeader(html, diffName, new TupleList<string, string>() { { "index.html", "Results" }, { "friendfinder.html", "Friend Finder" } });

                html.WriteLine("<h1>Details</h2>");
                html.WriteLine("<p id=\"detail-summary\">Matches: {0}<br/>Common token: {1}<br/>Token / match: {2:n2}</p>",
                    result.MatchCount,
                    result.TokenCount,
                    (double)result.TokenCount / (double)result.MatchCount);

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

                    diffHtml.Write(System.Web.HttpUtility.HtmlEncode(content.Substring(idx, end - idx)));
                    diffHtml.Write("</span>");
                    currentMatch.MoveNext();
                    idx = end;
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

        #region FriendFinder
        private static void WriteFriendFinderTitle(StreamWriter html)
        {
            html.WriteLine("<h1>OSPC - FriendFinder</h1>");
            html.WriteLine("<table id=\"friend-table\">");
            html.WriteLine(@"<tr id=""friend-table-header"">
    <th>Submission</th>
    <th>#</th>
    <th>sum(Similarity)</th>
    <th>Friend</th>
    <th>Similarity</th>
    <th>Matches</th>
    <th>Diff</th>
</tr>");
        }
        private void WriteFriendFinderLine(StreamWriter html, FriendOf f)
        {
            html.WriteLine(@"<tr class=""friend-table-group-row"">
    <td class=""friend-group-col""><a href=""{1}"">{0}</a></td>
    <td class=""friend-group-col right"">{2}</td>
    <td class=""friend-group-col right"">{3:n2}</td>",
                f.Submission.FilePath.MaxLength(17, "...", true),
                f.Submission.FilePath,
                f.InMatches.Count,
                100.0 * f.SumSimilarity);
            var first = true;
            var lst = f.InMatches
                .OrderByDescending(i => f.Submission.FilePath == i.A.FilePath ? i.SimilarityB : i.SimilarityA);
            foreach (var match in lst)
            {
                if (!first)
                {
                    html.WriteLine(@"<tr class=""friend-table-detail-row"">
<td class=""friend-group-col empty first""></td>
<td class=""friend-group-col empty""></td>
<td class=""friend-group-col empty last""></td>");
                }
                first = false;

                var isA = f.Submission.FilePath == match.A.FilePath;
                var path = isA ? match.B.FilePath : match.A.FilePath;
                html.WriteLine(@"
    <td class=""friend-detail-col""><a href=""{1}"">{0}</a></td>
    <td class=""friend-detail-col right"">{2:n2}</td>
    <td class=""friend-detail-col right"">{3}</td>
    <td class=""friend-detail-col""><a href=""{4}"">Diff</a></td>
</tr>",
                path.MaxLength(17, "...", true),
                path,
                100.0 * (isA ? match.SimilarityB : match.SimilarityA),
                match.MatchCount,
                GetDetailFileName(match)
                );
            }
        }
        private static void WriteFriendFinderFooter(StreamWriter html)
        {
            html.WriteLine("</table>");
        }
        #endregion

        #region Commmon
        private static string GetDetailFileName(CompareResult result)
        {
            return string.Format("Match_{0:00000}.html", result.Index + 1);
        }

        private static void WriteHeader(StreamWriter html, string title, TupleList<string, string> menu)
        {
            if (html == null) throw new ArgumentNullException("html");
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentNullException("title");
            if (menu == null) throw new ArgumentNullException("menu");

            html.WriteLine(@"<html>
<head>
    <meta charset=""UTF-8"">
    <title>{0}</title>
    <link href=""style.css"" rel=""stylesheet"" />
</head>
<body>", title);

            html.WriteLine(@"<ul id=""menu"">");
            foreach (var item in menu)
            {
                html.WriteLine(@"<li class=""menu-item""><a class=""menu-link"" href=""{0}"">{1}</a></li>", item.Item1, item.Item2);
            }
            html.WriteLine(@"</ul>");
        }

        private void WriteStylesheet()
        {
            using (var sw = new StreamWriter(Path.Combine(OutPath, "style.css")))
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
