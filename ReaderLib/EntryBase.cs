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
  /// Base class for per-entry objects
  /// </summary>
  /// <remarks>Just used for links back up the object tree and notification.</remarks>
  public class EntryBase : UniquelyIdentifiable, INotifyPropertyChanged
  {
    /// <summary>
    /// Containing subscription
    /// </summary>
    [XmlIgnore]
    public Subscription Parent;

    /// <summary>
    /// Containing list
    /// </summary>
    /// <remarks>We only need this to set the <c>Dirty</c> flag. Also here in the base
    /// class we don't know its full type.  Hence the use of an interface.</remarks>
    [XmlIgnore]
    public IDirtyable Container;

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
