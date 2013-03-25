using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace ReaderLib
{
  /// <summary>
  /// Interface to takeouts from Google Reader
  /// </summary>
  public class GoogleSubscriptions
  {
    /// <summary>
    /// Construct a subscription parser
    /// </summary>
    /// <param name="stream"></param>
    public GoogleSubscriptions(Stream stream)
    {
      xml = XDocument.Load(stream);
    }

    /// <summary>
    /// Get a list of subscriptions
    /// </summary>
    /// <remarks>
    /// <para>Unrecognized subscription types are simply ignored
    /// (for now).</para>
    /// </remarks>
    /// <returns></returns>
    public List<Subscription> GetSubscriptions()
    {
      List<Subscription> subscriptions = new List<Subscription>();
      foreach (XElement outline in xml.Descendants("outline")) {
        switch((string)outline.Attribute("type")) {
          case "rss":
            XAttribute uri = outline.Attribute("xmlUrl");
            XAttribute publicUri = outline.Attribute("htmlUrl");
            XAttribute title = outline.Attribute("title");
            if(uri == null) {
              continue;
            }
            RssSubscription r = new RssSubscription() {
              URI = (string)uri,
            };
            if(publicUri != null) {
              r.PublicURI = (string)publicUri;
            }
            if(title != null) {
              r.Title = (string)title;
            }
            subscriptions.Add(r);
            break;
          default:
            break;
        }
      }
      return subscriptions;
    }

    private XDocument xml;
  }
}
