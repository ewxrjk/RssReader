using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// Data for a single subscription entry
  /// </summary>
  /// <remarks>
  /// <para>Subclassed by each subscription type.</para>
  /// </remarks>
  [XmlInclude(typeof(WebEntry))]
  public class Entry : UniquelyIdentifiable, INotifyPropertyChanged
  {
    /// <summary>
    /// Containing subscription
    /// </summary>
    [XmlIgnore]
    public Subscription Parent;

    /// <summary>
    /// Containing list
    /// </summary>
    /// <remarks>We only need this to set the <c>Dirty</c> flag.</remarks>
    [XmlIgnore]
    public EntryList Container;

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

    /// <summary>
    /// Notify the parent subscription that a subscription property changed
    /// </summary>
    /// <param name="propertyName"></param>
    /// <remarks>For example, if one entry's read state changes then the
    /// subscription's unread count must change to match.</remarks>
    protected void ParentPropertyChanged(string propertyName)
    {
      if (Parent != null) {
        Parent.OnPropertyChanged(propertyName);
      }
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName]string propertyName = "")
    {
      if (Container != null) {
        Container.Dirty = true;
      }
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    #endregion

  }
}
