using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReaderLib
{
  /// <summary>
  /// Data for a single subscription entry
  /// </summary>
  /// <remarks>
  /// <para>Subclassed by each subscription type.</para>
  /// </remarks>
  [XmlInclude(typeof(RssEntryData))]
  public class Entry : EntryBase
  {

    /// <summary>
    /// Entry title
    /// </summary>
    /// <remarks>Used in summary of subscription contents.</remarks>
    public string Title
    {
      get
      {
        return _Title;
      }
      set
      {
        if (_Title != value) {
          _Title = value;
          OnPropertyChanged();
        }
      }
    }

    [XmlIgnore]
    private string _Title = null;

    /// <summary>
    /// Entry description
    /// </summary>
    /// <remarks>The full text of the entry.  Currently assumed to be HTML, which is all that's required,
    /// but maybe more flexibility would be appropriate.</remarks>
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
    private string _Description = null;

    /// <summary>
    /// Entry read/unread state
    /// </summary>
    [XmlAttribute("Read")]
    public bool Read
    {
      get
      {
        return _Read;
      }
      set
      {
        if (_Read != value) {
          _Read = value;
          OnPropertyChanged();
          ParentPropertyChanged("UnreadEntries");
        }
      }
    }

    private bool _Read = false;

    /// <summary>
    /// Entry serial number
    /// </summary>
    [XmlAttribute("Serial")]
    public int Serial;

  }
}
