using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.ComponentModel;

namespace ReaderLib
{
  public partial class WebEntry
  {

    public bool UpdateFromRss(XElement item, Action<Action> dispatch, Action<Exception> error)
    {
      XElement title = item.Element("title");
      XElement description = item.Element("description");
      XElement encoded = item.Element(XName.Get("encoded", "http://purl.org/rss/1.0/modules/content/"));
      XElement pubDate = item.Element("pubDate");
      string ItemURI = GetRssItemUri(item);
      Title = (title != null ? (string)title : null);
      if (encoded != null) {
        Description = (string)encoded;
      }
      else if (description != null) {
        Description = (string)description;
      }
      else {
        Description = null;
      }
      URI = ItemURI;
      try {
        if (pubDate != null) {
          Date = Tools.RFC822Date((string)pubDate);
        }
      }
      catch (Exception e) {
        error(new SubscriptionParsingException(string.Format("invalid date: {0}", (string)pubDate),
                                               e));
      }
      // TODO author, category, comments, enclosure, source 
      return false;
    }

    static private string GetRssItemUri(XElement item)
    {
      // If the <guid> advertizes itself as a link, use that
      XElement guid = item.Element("guid");
      if (guid != null) {
        XAttribute isPermaLink = guid.Attribute("isPermaLink");
        if (isPermaLink != null && (string)isPermaLink == "true") {
          return (string)guid;
        }
      }
      // Otherwise if <link> is present use that
      XElement link = item.Element("link");
      if (link != null) {
        return (string)link;
      }
      // Otherwise see if <guid> looks like a URI
      // (this is out of spec but even the sample RSS needs it!)
      if (guid != null) {
        Uri URI;
        if (Uri.TryCreate((string)guid, UriKind.Absolute, out URI)) {
          return URI.ToString();
        }
      }
      return null;
    }

  }
}
