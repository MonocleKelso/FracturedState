using System.Collections;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace FracturedState.Game.Data
{
    [XmlRoot("rss")]
    public class RSSFeed
    {
        public static void Get(string url, MonoBehaviour runner, System.Action<RSSFeed> success, System.Action fail)
        {
            var request = UnityWebRequest.Get(url);
            runner.StartCoroutine(RequestWait(request, success, fail));
        }

        private static IEnumerator RequestWait(UnityWebRequest req, System.Action<RSSFeed> success, System.Action fail)
        {
            req.Send();
            while (!req.isDone)
            {
                yield return null;
            }
            if (!req.isNetworkError)
            {
                try
                {
                    var feed = DataUtil.DeserializeXmlString<RSSFeed>(req.downloadHandler.text);
                    if (feed != null)
                    {
                        success?.Invoke(feed);
                    }
                    else
                    {
                        fail?.Invoke();
                    }
                }
                catch (System.Exception)
                {
                    fail?.Invoke();
                }
            }
            else
            {
                fail?.Invoke();
            }
        }

        [XmlElement("channel")]
        public RssChannel Channel { get; set; }
    }

    public class RssChannel
    {
        [XmlElement("item")]
        public RssItem[] Items { get; set; }

        [XmlElement("link")]
        public string Link { get; set; }
    }

    public class RssItem
    {
        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("link")]
        public string Link { get; set; }

        [XmlElement("category")]
        public string[] Categories { get; set; }

        [XmlElement("pubDate")]
        public string PubDate { get; set; }

        [XmlElement("creator", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Author { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        public string PrettyDescription()
        {
            if (string.IsNullOrEmpty(Description)) return string.Empty;
            
            // remove HTML tags, replace unicodes with regular characters and then remove remaining unicode
            var descr = Regex.Replace(Description, @"<[^>]+>|&nbsp;", "").Trim().Replace("&#8217;", "'").Replace("&#8211;", "-").Replace("&#8230;", "...");
            return Regex.Replace(descr, @"&#\d{4};", "");
        }

        public string PrettyDetails()
        {
            string val = "";
            if (!string.IsNullOrEmpty(PubDate))
            {
                System.DateTime dt;
                if (System.DateTime.TryParse(PubDate, out dt))
                {
                    val = dt.ToString("MMMM dd, yyyy");
                }
            }
            if (!string.IsNullOrEmpty(Author))
            {
                if (!string.IsNullOrEmpty(val))
                {
                    val += ", ";
                }
                val += Author;
            }
            return val;
        }
    }
}