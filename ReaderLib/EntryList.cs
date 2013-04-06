using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// Data for all the entries in a subscription
  /// </summary>
  public class EntryList: INotifyPropertyChanged
  {
    public EntryList()
    {
      Dirty = false;
    }

    /// <summary>
    /// The collection of entries
    /// </summary>
    [XmlIgnore]
    public Dictionary<string, Entry> Entries = new Dictionary<string, Entry>();

    /// <summary>
    /// Parent subscription object
    /// </summary>
    [XmlIgnore]
    public Subscription ParentSubscription;

    /// <summary>
    /// Dirty flag, set when anything changes
    /// </summary>
    [XmlIgnore]
    public bool Dirty { get; set; }

    #region Public Properties

    /// <summary>
    /// The last time this subscription was rechecked against the underlying data.
    /// </summary>
    [XmlAttribute("LastCheck")]
    public DateTime LastCheck
    {
      get { return _LastCheck; }
      set
      {
        _LastCheck = value;
        OnPropertyChanged();
      }
    }

    [XmlIgnore]
    private DateTime _LastCheck = new DateTime(0, DateTimeKind.Utc);

    #endregion

    #region Serialization

    #region Infrastructure

    /// <summary>
    /// Filename for an object of this type
    /// </summary>
    /// <returns></returns>
    private static string Filename(Subscription sub)
    {
      return Path.Combine(sub.Directory(), string.Format("{0}.xml", sub.Identity.ToString()));
    }

    /// <summary>
    /// Filename for this object
    /// </summary>
    /// <returns></returns>
    private string Filename()
    {
      return Filename(ParentSubscription);
    }

    /// <summary>
    /// XML serializer for this type
    /// </summary>
    static public XmlSerializer Serializer = new XmlSerializer(typeof(EntryList));

    #endregion

    #region Saving

    /// <summary>
    /// Save component contents
    /// </summary>
    public void Save(bool force = false)
    {
      if (ParentSubscription.Parent != null && (Dirty || force)) {
        Directory.CreateDirectory(ParentSubscription.Directory());
        Save(Filename());
        Dirty = false;
      }
    }

    /// <summary>
    /// Save to a specific path
    /// </summary>
    /// <param name="path"></param>
    public void Save(string path)
    {
      using (StreamWriter sw = new StreamWriter(path)) {
        Serializer.Serialize(sw, this);
        sw.Flush();
      }
    }

    #endregion

    #region Loading

    /// <summary>
    /// Load component contents
    /// </summary>
    /// <returns></returns>
    public static EntryList Load(Subscription sub, string path = null)
    {
      if (path == null && sub.Parent != null) {
        path = Filename(sub);
      }
      EntryList newComponent;
      if (path != null && File.Exists(path)) {
        using (StreamReader sr = new StreamReader(path)) {
          newComponent = (EntryList)Serializer.Deserialize(sr);
        }
      }
      else {
        newComponent = new EntryList();
      }
      newComponent.ParentSubscription = sub;
      foreach (Entry entry in newComponent.Entries.Values) {
        entry.ParentSubscription = sub;
        entry.ParentEntryList = newComponent;
      }
      return newComponent;
    }

    #endregion

    #endregion

    #region Proxies

    /// <summary>
    /// XML serialization proxy for Entries
    /// </summary>
    /// <remarks>
    /// <para>Dictionaries aren't serializable.  As it happens we don't actually
    /// need it to be serialized as a dictionary since each entry knows its own key
    /// anyway.</para></remarks>
    [XmlElement("Entry")]
    public Entry[] ProxyEntries
    {
      get
      {
        return Entries.Values.ToArray();
      }
      set
      {
        foreach (Entry e in value) {
          Entries[e.Identity] = e;
        }
      }
    }

    #endregion
    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName]string propertyName = "")
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    #endregion

  }
}
