﻿// This file is part of HalfMoon RSS reader
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
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// An RSS subscription
  /// </summary>
  public partial class WebSubscription : Subscription
  {
    public WebSubscription()
    {
      Description = null;
      URI = null;
      PublicURI = null;
      NextSerial = 0;
      Hash = "SHA1";
    }

    /// <summary>
    /// Return the suggested next update time
    /// </summary>
    /// <returns></returns>
    /// <remarks>Override to honor TTL (within reason)</remarks>
    public override DateTime NextUpdate()
    {
      DateTime baseUpdate = base.NextUpdate();
      if (TTL > 0) {
        int limitedTTL = TTL;
        if (limitedTTL > 86400) {
          limitedTTL = 86400;
        }
        DateTime subscriptionUpdate = LastCheck.AddSeconds((double)limitedTTL);
        if (subscriptionUpdate > baseUpdate) {
          return subscriptionUpdate;
        }
      }
      return baseUpdate;
    }

    #region Public Properties

    [UserVisible(Modifiable = false, Priority = 1)]
    public string Description
    {
      get
      {
        return _Description;
      }
      set
      {
        if (_Description != value) {
          _Description = value;
          OnPropertyChanged();
        }
      }
    }

    [XmlIgnore]
    private string _Description;

    /// <summary>
    /// The URI of the subscription
    /// </summary>
    [UserVisible(Description = "Subscription URI", Modifiable = true, Priority = 0, Type = "URI")]
    public string URI
    {
      get
      {
        return _URI;
      }
      set
      {
        if (_URI != value) {
          _URI = value;
          OnPropertyChanged();
        }
      }
    }

    [XmlIgnore]
    private string _URI;

    /// <summary>
    /// The user-facing URI of the subscription
    /// </summary>
    /// <remarks>
    /// <para><code>null</code> if there is no user-facing URI.</para>
    /// </remarks>
    [UserVisible(Description = "Browser URI", Modifiable = false, Priority = 2, Type = "URI")]
    public string PublicURI
    {
      get
      {
        return _PublicURI;
      }
      set
      {
        if (_PublicURI != value) {
          _PublicURI = value;
          OnPropertyChanged();
        }
      }
    }

    [XmlIgnore]
    private string _PublicURI;

    [XmlIgnore]
    [UserVisible(Description = "Subscription type", Modifiable = false, Priority = 257)]
    public string Type
    {
      get
      {
        return _Type;
      }
      set
      {
        if (_Type != value) {
          _Type = value;
          OnPropertyChanged();
        }
      }
    }

    [XmlIgnore]
    private string _Type = "unknown";

    /// <summary>
    /// Time to live
    /// </summary>
    /// <remarks>Normalized to seconds.</remarks>
    [XmlAttribute("TTL")]
    public int TTL { get; set; }

    /// <summary>
    /// Item ID hash function
    /// </summary>
    /// <remarks>Needs to be something understood by <c>HashAlgorithm.Create</c>.</remarks>
    [XmlAttribute("Hash")]
    public string Hash
    {
      get { return _Hash; }
      set
      {
        if (_Hash != value) {
          _Hash = value;
          HashFunction = HashAlgorithm.Create(_Hash);
        }
      }
    }

    private string _Hash;

    private HashAlgorithm HashFunction = null;

    #endregion

    #region Updating

    override public void Update(Action<Action> dispatch, Action<Exception> error)
    {
      try {
        WebRequest request = WebRequest.Create(URI);
        WebResponse response = request.GetResponse();
        using (Stream rs = response.GetResponseStream()) {
          UpdateFromStreamReader(new StreamReader(rs, Tools.GetEncoding(response)), dispatch, error);
        }
      }
      catch (Exception e) {
        dispatch(() =>
        {
          Error = new SubscriptionFetchingException(e.Message, e)
          {
            Subscription = this
          };
          error(Error);
        });
      }
    }

    public void UpdateFromStreamReader(StreamReader reader, Action<Action> dispatch, Action<Exception> error)
    {
      XDocument doc = XDocument.Load(reader);
      XElement root = doc.Root;
      switch (root.Name.LocalName) {
        case "rss":
        case "RDF":
          UpdateFromRss(root, dispatch, error);
          break;
        case "feed":
          UpdateFromAtom(root, dispatch, error);
          break;
        default:
          throw new SubscriptionParsingException(string.Format("Unrecognized web subscription type: {0}", root.Name.LocalName))
          {
            Subscription = this
          };
      }
    }

    private void UpdateEntry(XElement item,
                             string id,
                             Action<WebEntry, XElement, Action<Exception>> update,
                             Action<Exception> error)
    {
      WebEntry entry = null;
      bool newEntry = false;
      // Find out what we know
      entry = (WebEntry)_Entries.Entries.GetValueOrDefault(id, null);
      if (entry == null) {
        // A new entry.
        entry = new WebEntry()
        {
          Identity = id,
          Serial = this.NextSerial++,
          ParentSubscription = this,
          ParentEntryList = _Entries,
        };
        newEntry = true;
      }
      // Update the item
      try {
        update(entry, item, error);
      }
      catch (SubscriptionParsingException spe) {
        // If individual errors fail, we just log them and move on
        spe.Subscription = this;
        error(spe);
      }
      // If anything new was created, record it
      if (newEntry) {
        Add(entry);
      }

    }

    static private XElement GetMandatoryElement(XElement container, XName name)
    {
      XElement element = container.Element(name);
      if (element == null) {
        throw new SubscriptionParsingException(string.Format("Missing <{0}> element", name));
      }
      return element;
    }

    private string HashId(string id)
    {
      return Convert.ToBase64String(HashFunction.ComputeHash(Encoding.UTF8.GetBytes(Identity + " " + id)));
    }

    #endregion

    #region Configuring New Subscriptions

    public void Configure(Action<Action> dispatch, Action<Exception> error, Uri URI, bool acceptHTML = true)
    {
      try {
        WebRequest request = WebRequest.Create(URI);
        WebResponse response = request.GetResponse();
        if (response.ContentType.ToLowerInvariant().Contains("html")) {
          ConfigureWithHtml(dispatch, error, URI, response);
          return;
        }
        ConfigureWithSubscription(dispatch, error, URI, response);
      }
      catch (Exception e) {
        dispatch(() =>
        {
          error(e);
        });
      }

    }

    private void ConfigureWithHtml(Action<Action> dispatch, Action<Exception> error, Uri URI, WebResponse response)
    {
      HTML.Document html;
      using (StreamReader outerReader = new StreamReader(response.GetResponseStream(), Tools.GetEncoding(response))) {
        try {
          HTML.Parser parser = new HTML.Parser() { Input = outerReader };
          html = parser.Parse();
        }
        catch (Exception e) {
          throw new ApplicationException(string.Format("Error parsing HTML: {0}", e.Message), e);
        }
      }
      Exception firstError = null;
      foreach (HTML.Element link in from node in html.HTML.FindChild("head").Contents
                                    where node is HTML.Element && ((HTML.Element)node).Name == "link"
                                    select node) {
        if (link.Attributes.ContainsKey("rel")
           && link.Attributes["rel"].ToLowerInvariant() == "alternate"
           && link.Attributes.ContainsKey("type")
           && (link.Attributes["type"].ToLowerInvariant() == "application/rss+xml"
               || link.Attributes["type"].ToLowerInvariant() == "application/x.atom+xml")
           && link.Attributes.ContainsKey("href")) {
          Uri SubscriptionUri;
          if (Uri.TryCreate(link.Attributes["href"], UriKind.Absolute, out SubscriptionUri)
              || Uri.TryCreate(URI, link.Attributes["href"], out SubscriptionUri)) {
            try {
              Configure(dispatch, error, SubscriptionUri, false);
              return; // accept the first one that actually works
            }
            catch (Exception e) {
              if (firstError == null) {
                firstError = e;
              }
            }
          }
        }
      }
      if (firstError != null) {
        throw firstError;
      }
      throw new SubscriptionParsingException(string.Format("No subscription link found in HTML"));
    }

    private void ConfigureWithSubscription(Action<Action> dispatch, Action<Exception> error, Uri URI, WebResponse response)
    {
      using (StreamReader reader = new StreamReader(response.GetResponseStream(), Tools.GetEncoding(response))) {
        XDocument doc = XDocument.Load(reader);
        XElement root = doc.Root;
        switch (root.Name.LocalName) {
          case "rss":
          case "feed": // looks like atom
          case "RDF":
            dispatch(() =>
            {
              this.URI = URI.ToString();
              Reset();
            });
            Update(dispatch, error);
            break;
          default:
            throw new SubscriptionParsingException(string.Format("Unrecognized web subscription type: {0}", root.Name.LocalName))
            {
              Subscription = this
            };
        }
      }
    }


    #endregion

  }
}
