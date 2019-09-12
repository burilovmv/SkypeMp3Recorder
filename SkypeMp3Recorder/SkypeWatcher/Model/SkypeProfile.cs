using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using log4net;

namespace SkypeMp3Recorder.SkypeWatcher.Model
{
    public class SkypeProfile
    {
        internal static readonly ILog log = LogManager.GetLogger("SkypeProfile");
        public const string configFileName = "config.xml";

        public string ProfilePath { get; set; }
        public SkypeUser User { get; set; }
        public DateTime LastUsed { get; set; }

        public string DefaultMicrophone { get; set; }
        public string DefaultSpeakers { get; set; }

        private string ReadAllText(string path, Encoding encoding) {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var textReader = new StreamReader(fileStream, encoding))
            {
                return textReader.ReadToEnd();
            }
        }

        private string UnescapeUtf8Chars(string text) {
            var bytes = new byte[2];
            var sb = new StringBuilder(text);
            for (Match match = new Regex(@"(&#(\d+);)(&#(\d+);)", RegexOptions.IgnoreCase).Match(text); match.Success; match = match.NextMatch())
            {
                try
                {
                    if (match.Groups.Count == 5)
                    {
                        bytes[0] = byte.Parse(match.Groups[2].Value);
                        bytes[1] = byte.Parse(match.Groups[4].Value);
                    }
                    try
                    {
                        sb.Replace(match.Groups[0].Value, Encoding.UTF8.GetString(bytes));
                    }
                    catch
                    {
                    }
                }
                catch (Exception ex)
                {
                    sb.Replace(match.Groups[0].Value, "");
                }
            }

            return sb.ToString();
        }

        public void LoadSettings() {
            try {
                string xml = ReadAllText(Path.Combine(ProfilePath, configFileName), Encoding.UTF8);
                xml = UnescapeUtf8Chars(xml);

                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var nodes = doc.GetElementsByTagName("InputName");
                if (nodes.Count > 0) {
                    DefaultMicrophone = HttpUtility.HtmlDecode(nodes[0].InnerXml);
                }
                nodes = doc.GetElementsByTagName("OutputName");
                if (nodes.Count > 0) {
                    DefaultSpeakers = nodes[0].InnerXml;
                }

            }
            catch (Exception ex) {
                log.Error("Error parsing user profile", ex);
            }
        }
    }
}
