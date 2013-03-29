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
  /// <summary>
  /// An RSS subscription
  /// </summary>
  public class WebSubscription : Subscription
  {
    public WebSubscription()
    {
      Description = null;
      URI = null;
      PublicURI = null;
      NextSerial = 0;
      Hash = "SHA1";
    }

    public override string Type()
    {
      return "RSS";
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

    [UserVisible(Modifiable = false)]
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
    [UserVisible(Description = "Feed URI", Modifiable = true)]
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
    [UserVisible(Description = "Browser URI", Modifiable = false)]
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
      // TODO atom support
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
          error(new SubscriptionFetchingException(e.Message, e)
          {
            Subscription = this
          });
        });
      }
    }

    public void UpdateFromStreamReader(StreamReader reader, Action<Action> dispatch, Action<Exception> error)
    {
      XDocument doc = XDocument.Load(reader);
      XElement root = doc.Root;
      switch (root.Name.LocalName) {
        case "rss":
          UpdateFromRss(root, dispatch, error);
          break;
        case "feed":
          UpdateFromAtom(root, dispatch, error);
          break;
        case "RDF":
          UpdateFromRdf(root, dispatch, error);
          break;
        default:
          // TODO RDF
          throw new SubscriptionParsingException(string.Format("Unrecognized web feed type: {0}", root.Name.LocalName))
          {
            Subscription = this
          };
      }
    }

    private void UpdateFromRss(XElement rss, Action<Action> dispatch, Action<Exception> error)
    {
      XElement channel = GetMandatoryElement(rss, "channel", dispatch, error);
      // Mandatory <channel> subelements
      Title = (string)GetMandatoryElement(channel, "title", dispatch, error);
      Description = (string)GetMandatoryElement(channel, "description", dispatch, error);
      PublicURI = (string)GetMandatoryElement(channel, "link", dispatch, error);
      // Optional <channel> subelements
      XElement ttl = channel.Element("ttl");
      int ttlValue = 0;
      int.TryParse((string)ttl, out ttlValue);
      TTL = ttlValue;
      // In order to assign serials correctly, we need to walk
      // the item list in reverse.
      IEnumerable<XElement> items = channel.Elements("item").Reverse();
      dispatch(() =>
      {
        foreach (XElement item in items) {
          string unique = GetUniqueIdFromRss(item, dispatch, error);
          // Retrieve or create entry
          WebEntry data = null;
          bool newEntry = false;
          // Find out what we know
          data = (WebEntry)_Entries.Entries.GetValueOrDefault(unique, null);
          if (data == null) {
            // A new entry.
            data = new WebEntry()
            {
              Identity = unique,
              Serial = this.NextSerial++,
              Parent = this,
              Container = _Entries,
            };
            newEntry = true;
          }
          // Update the item
          try {
            data.UpdateFromRss(item, dispatch, error);
          }
          catch (SubscriptionParsingException spe) {
            spe.Subscription = this;
            error(spe);
          }
          // If anything new was created, record it
          if (newEntry) {
            Add(data);
          }
        }
      });
    }

    private void UpdateFromAtom(XElement atom, Action<Action> dispatch, Action<Exception> error)
    {
      // TODO
    }

    private void UpdateFromRdf(XElement rdf, Action<Action> dispatch, Action<Exception> error)
    {
      // TODO
    }

    private XElement GetMandatoryElement(XElement container, string name,
                                         Action<Action> dispatch, Action<Exception> error)
    {
      XElement element = container.Element(name);
      if (element == null) {
        dispatch(() =>
        {
          error(new SubscriptionParsingException(string.Format("Missing <{0}> element", name))
          {
            Subscription = this,
          });
        });
      }
      return element;
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
    private string GetUniqueIdFromRss(XElement item, Action<Action> dispatch, Action<Exception> error)
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
        unique = (string)GetMandatoryElement(item, "description", dispatch, error);
      }
      // Convert unique string to something managable.  We include
      // subscription ID to ensure global uniqueness.
      return Convert.ToBase64String(HashFunction.ComputeHash(Encoding.UTF8.GetBytes(Identity + " " + unique)));
    }

    #endregion

  }
}
