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
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

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
            WebSubscription r = new WebSubscription() {
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
