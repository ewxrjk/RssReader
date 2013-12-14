// This file is part of HalfMoon RSS reader
// Copyright (C) 2013 Richard Kettlewell
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ReaderLib
{
  public partial class WebSubscription
  {
    static private XNamespace RDF = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    static private XNamespace RSS = "http://purl.org/rss/1.0/";
    static private XNamespace RSSContent = "http://purl.org/rss/1.0/modules/content/";
    static private XNamespace DcmiMetadata = "http://purl.org/dc/elements/1.1/";

    private void UpdateFromRss(XElement rss, Action<Action> dispatch, Action<Exception> error)
    {
      string type;
      XElement channel;
      if((channel = rss.Element("channel")) != null) {
        type = "RSS 2.0";
      } else if((channel = rss.Element(RSS + "channel")) != null) {
        type = "RSS (RDF)";
      } else {
        throw new SubscriptionParsingException("No <channel> element") { Subscription = this };
      }
      // Mandatory <channel> subelements (actually we cope gracefuly without now)
      XElement title = channel.Element("title") ?? channel.Element(RSS + "title");
      XElement description = channel.Element("description") ?? channel.Element(RSS + "description");
      XElement link = channel.Element("link") ?? channel.Element(RSS + "link");
      // Optional <channel> subelements
      XElement ttl = channel.Element("ttl");
      int ttlValue = 0;
      int.TryParse((string)ttl, out ttlValue);
      // Item list isn't in a consistent place
      IEnumerable<XElement> items = channel.Elements("item");
      if (items.Count() == 0) {
        items = rss.Elements(RSS + "item");
      }
      // In order to assign serials correctly, we need to walk
      // the item list in reverse.
      IEnumerable<XElement> ritems = items.Reverse();
      dispatch(() =>
      {
        try {
          Title = title != null ? (string)title : "";
          Description = description != null ? (string)description : "";
          PublicURI = link != null ? (string)link : "";
          TTL = ttlValue;
          foreach (XElement item in ritems) {
            try {
              UpdateEntry(item, GetUniqueIdFromRss(item, error), UpdateFromRss, error);
            }
            catch (SubscriptionException se) {
              se.Subscription = this;
              error(se);
            }
            catch (Exception e) {
              error(new SubscriptionParsingException(e.Message, e) { Subscription = this });
            }
          }
          Type = type;
          Error = null;
        }
        catch (SubscriptionException se) {
          if (se.Subscription == null) {
            se.Subscription = this;
          }
          error(se);
          Error = se;
        }
        catch (Exception e) {
          error(e);
          Error = e;
        }
      });
    }

    private void UpdateFromRss(WebEntry entry, XElement item, Action<Exception> error)
    {
      XElement title = item.Element("title") ?? item.Element(RSS + "title");
      XElement description = item.Element(RSSContent + "encoded")
                             ?? item.Element("description")
                             ?? item.Element(RSS + "description");
      XElement pubDate = item.Element("pubDate");
      XElement dcDate = item.Element(DcmiMetadata + "date");
      entry.Title = (title != null ? (string)title : "");
      entry.Description = description != null ? (string)description : "";
      entry.URI = GetRssItemUri(item);
      if (pubDate != null) {
        try {
          entry.Date = Tools.RFC822Date((string)pubDate);
        }
        catch (Exception e) {
          error(new SubscriptionParsingException(string.Format("Invalid date string “{0}”", (string)pubDate),
                                                 e) { Subscription = this });
        }
      }
      else if (dcDate != null) {
        try {
          entry.Date = Tools.RFC3339Date((string)dcDate);
        }
        catch (Exception e) {
          error(new SubscriptionParsingException(string.Format("Invalid date string “{0}”", (string)dcDate),
                                                 e) { Subscription = this });
        }
      }
      // TODO author, category, comments, enclosure, source 
    }

    static private string GetRssItemUri(XElement item)
    {
      // If the <guid> advertizes itself as a link, use that
      XElement guid = item.Element("guid") ?? item.Element(RSS + "guid");
      if (guid != null) {
        XAttribute isPermaLink = guid.Attribute("isPermaLink");
        if (isPermaLink != null && (string)isPermaLink == "true") {
          return (string)guid;
        }
      }
      // Otherwise if <link> is present use that
      XElement link = item.Element("link") ?? item.Element(RSS + "link");
      if (link != null) {
        return (string)link;
      }
      // Maybe there's an RDF about attribute
      XAttribute about = item.Attribute(RDF + "about");
      if (about != null) {
        return about.Value;
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
    /// <para>If the subscription doesn't provide a GUID, or a link, and
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
      if (unique == null) {
        // Maybe there's an RDF about attribute
        XAttribute about = item.Attribute(RDF + "about");
        if (about != null) {
          unique = about.Value;
        }
      }
      // If we still don't have a unique ID use <description>
      // (one of <link> and <description> are required in RSS)
      if (unique == null) {
        unique = (string)GetMandatoryElement(item, "description");
      }
      // Convert unique string to something managable.  We include
      // subscription ID to ensure global uniqueness.
      return HashId(unique);
    }

  }
}
