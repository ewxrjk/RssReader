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

  }
}
