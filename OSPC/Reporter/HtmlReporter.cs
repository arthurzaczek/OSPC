using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC.Reporter
{
    public class HtmlReporter : IReporter
    {
        public static readonly string[] Colors = new[]
        {
            "#AA0000", "#00AA00", "#0000AA", "#00AAAA", "#AA00AA", "#AAAA00",
            "#880000", "#008800", "#000088", "#008888", "#880088", "#888800",
        };
        public void Create(List<CompareResult> results)
        {
            using (var html = new StreamWriter("report.html"))
            {
                WriteHeader(html);

                foreach (var result in results)
                {
                    var diffName = string.Format("{0}_{1}.html", Path.GetFileNameWithoutExtension(result.A.FilePath), Path.GetFileNameWithoutExtension(result.B.FilePath)).Replace(" ", "_");
                    WriteResultLine(html, result, diffName);

                    WriteDiff(result, diffName);
                }
                html.WriteLine("</table>");
                html.WriteLine("</body></html>");
                html.Flush();
            }
        }

        private static void WriteDiff(CompareResult result, string diffName)
        {
            using (var diffHtml = new StreamWriter(diffName))
            {
                diffHtml.WriteLine("<html><head><meta charset=\"UTF-8\"><title>{0}</title></head><body>", diffName);

                diffHtml.WriteLine("<div style=\"float:left;width:50%;font-family: monospace;white-space: pre;\">");
                diffHtml.WriteLine("<h2>{0}</h2>", Path.GetFileName(result.A.FilePath));
                using (var rd = new StreamReader(result.A.FilePath))
                {
                    ColorDiff(diffHtml, result, rd, m => m.TokensA);
                }
                diffHtml.WriteLine("</div>");

                diffHtml.WriteLine("<div style=\"float:left;width:50%;font-family: monospace;white-space: pre;\">");
                diffHtml.WriteLine("<h2>{0}</h2>", Path.GetFileName(result.B.FilePath));
                using (var rd = new StreamReader(result.B.FilePath))
                {
                    ColorDiff(diffHtml, result, rd, m => m.TokensB);
                }
                diffHtml.WriteLine("</div>");

                diffHtml.WriteLine("</body></html>");
                diffHtml.Flush();
            }
        }

        private static void ColorDiff(StreamWriter diffHtml, CompareResult result, StreamReader rd, Func<Match, LinkedList<Token>> tokenExtractor)
        {
            var content = rd.ReadToEnd();
            int idx = 0;
            int colorIdx = 0;
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
                    diffHtml.Write("<span style=\"font-weight: bold;color: {0}\">", Colors[colorIdx]);
                    colorIdx = ++colorIdx % Colors.Length;

                    diffHtml.Write(System.Web.HttpUtility.HtmlEncode(content.Substring(idx, end - idx + 1)));
                    diffHtml.Write("</span>");
                    currentMatch.MoveNext();
                    idx = end + 1;
                }
            }
        }

        private static void WriteResultLine(StreamWriter html, CompareResult result, string diffName)
        {
            html.WriteLine(@"<tr>
    <td><a href=""{6}"">{0}</a></td>
    <td>{1:n2}</td>
    <td><a href=""{7}"">{2}</a></td>
    <td>{3:n2}</td>
    <td>{4}</td>
    <td>{5}</td>
    <td><a href=""{8}"">Diff</a></td>
</tr>",
                                        result.A.FilePath.MaxLength(17, "...", true),
                                        100.0 * result.MatchA,
                                        result.B.FilePath.MaxLength(17, "...", true),
                                        100.0 * result.MatchB,
                                        result.MatchCount,
                                        result.TokenCount,
                                        result.A.FilePath,
                                        result.B.FilePath,
                                        diffName);
        }

        private static void WriteHeader(StreamWriter html)
        {
            html.WriteLine("<html><head><meta charset=\"UTF-8\"><title>OSPC</title></head><body>");
            html.WriteLine("<table>");
            html.WriteLine(@"<tr>
    <th>A</th>
    <th>% A</th>
    <th>B</th>
    <th>% B</th>
    <th>Matches</th>
    <th>Tokens</th>
    <th>Diff</th>
</tr>");
        }
    }
}
