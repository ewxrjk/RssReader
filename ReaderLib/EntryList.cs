using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.IO;

namespace ReaderLib
{
  /// <summary>
  /// Data for all the entries in a subscription
  /// </summary>
  public class EntryList: EntryListBase, INotifyPropertyChanged
  {

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

    /// <summary>
    /// XML serializer for this type
    /// </summary>
    static public XmlSerializer Serializer = new XmlSerializer(typeof(EntryList));

    /// <summary>
    /// Deserialize from a file
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static EntryList Load(Subscription parent, string path = null)
    {
      return EntryListBase.Load<EntryList>(parent, path);
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
