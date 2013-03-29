using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;

namespace ReaderLib
{
  public partial class WebSubscription
  {

    private void UpdateFromRss(XElement rss, Action<Action> dispatch, Action<Exception> error)
    {
      XElement channel = GetMandatoryElement(rss, "channel", dispatch, error);
      // Mandatory <channel> subelements
      string title = (string)GetMandatoryElement(channel, "title", dispatch, error);
      string description = (string)GetMandatoryElement(channel, "description", dispatch, error);
      string publicURI = (string)GetMandatoryElement(channel, "link", dispatch, error);
      // Optional <channel> subelements
      XElement ttl = channel.Element("ttl");
      int ttlValue = 0;
      int.TryParse((string)ttl, out ttlValue);
      // In order to assign serials correctly, we need to walk
      // the item list in reverse.
      IEnumerable<XElement> items = channel.Elements("item").Reverse();
      dispatch(() =>
      {
        Title = title;
        Description = description;
        PublicURI = publicURI;
        TTL = ttlValue;
        foreach (XElement item in items) {
          UpdateEntry(item, GetUniqueIdFromRss(item, error), UpdateFromRss, error);
        }
      });
    }

    private void UpdateFromRss(WebEntry entry, XElement item, Action<Exception> error)
    {
      XElement title = item.Element("title");
      XElement description = item.Element("description");
      XElement encoded = item.Element(XName.Get("encoded", "http://purl.org/rss/1.0/modules/content/"));
      XElement pubDate = item.Element("pubDate");
      string ItemURI = GetRssItemUri(item);
      entry.Title = (title != null ? (string)title : null);
      if (encoded != null) {
        entry.Description = (string)encoded;
      }
      else if (description != null) {
        entry.Description = (string)description;
      }
      else {
        entry.Description = null;
      }
      entry.URI = ItemURI;
      try {
        if (pubDate != null) {
          entry.Date = Tools.RFC822Date((string)pubDate);
        }
      }
      catch (Exception e) {
        error(new SubscriptionParsingException(string.Format("invalid date: {0}", (string)pubDate),
                                               e));
      }
      // TODO author, category, comments, enclosure, source 
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

    /// <summary>
    /// Find the unique ID for an item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <remarks>
    /// <para>If the feed doesn't provide a GUID, or a link, and
    /// changes the description, then the article will be
    /// duplicated.</para>
    /// </remarks>
    private string GetUniqueIdFromRss(XElement item, Action<Exception> error)
    {
      string unique = null;
      // If possible use <guid> for unique ID
      XElement guid = item.Element("guid");
      if (guid != null) {
        unique = (string)guid;
      }
      // If <guid> didn't provide a unique ID, try <link>
      if (unique == null) {
        XElement link = item.Element("link");
        if (link != null) {
          unique = (string)link;
        }
      }
      // If we still don't have a unique ID use <description>
      // (one of <link> and <description> are required in RSS)
      if (unique == null) {
        unique = (string)GetMandatoryElement(item, "description", error);
      }
      // Convert unique string to something managable.  We include
      // subscription ID to ensure global uniqueness.
      return HashId(unique);
    }

  }
}
