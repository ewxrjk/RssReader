using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

namespace ReaderLib
{
  /// <summary>
  /// Base class for entry containers
  /// </summary>
  /// <typeparam name="ET">Entry type</typeparam>
  /// <remarks>
  /// <para>Derived classes must have at least the following static fields:</para>
  /// <list type="bullet">
  /// <item>
  /// <term><c>Serializer</c></term>
  /// <description>The XML serializer for the derived class</description>
  /// </item>
  /// </list>
  /// </remarks>
  /// TODO kind of overdesigned now we only have one type of entry class!
  public class EntryListBase : IDirtyable
  {
    public EntryListBase()
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
    public Subscription Parent;

    /// <summary>
    /// Dirty flag, set when anything changes
    /// </summary>
    [XmlIgnore]
    public bool Dirty { get; set; }

    #region Serialization

    /// <summary>
    /// Filename for an object of this type
    /// </summary>
    /// <returns></returns>
    protected static string Filename(Subscription sub)
    {
      return Path.Combine(sub.Directory(), string.Format("{0}.xml", sub.Identity.ToString()));
    }

    /// <summary>
    /// Filename for this object
    /// </summary>
    /// <returns></returns>
    protected string Filename()
    {
      return Filename(Parent);
    }

    /// <summary>
    /// Save component contents
    /// </summary>
    public void Save(bool force = false)
    {
      if (Parent.Parent != null && (Dirty || force)) {
        Directory.CreateDirectory(Parent.Directory());
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
        GetSerializer(this.GetType()).Serialize(sw, this);
        sw.Flush();
      }
    }

    /// <summary>
    /// Load component contents
    /// </summary>
    /// <returns></returns>
    public static T Load<T>(Subscription sub, string path = null) where T : EntryListBase, new()
    {
      if (path == null && sub.Parent != null) {
        path = Filename(sub);
      }
      T newComponent;
      if (path != null && File.Exists(path)) {
        using (StreamReader sr = new StreamReader(path)) {
          newComponent = (T)GetSerializer(typeof(T)).Deserialize(sr);
        }
      }
      else {
        newComponent = new T();
      }
      newComponent.Parent = sub;
      foreach (Entry entry in newComponent.Entries.Values) {
        entry.Parent = sub;
        entry.Container = newComponent;
      }
      return newComponent;
    }

    #endregion

    #region Base class static fields

    public static XmlSerializer GetSerializer(Type type)
    {
      return (XmlSerializer)type.InvokeMember("Serializer", BindingFlags.GetField, null, null, null);
    }

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

  }
}
