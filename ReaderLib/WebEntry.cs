using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.ComponentModel;

namespace ReaderLib
{
  public partial class WebEntry : Entry, INotifyPropertyChanged
  {
    #region Public Properties

    [UserVisible]
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
    private string _URI = null;

    [UserVisible]
    public DateTime Date
    {
      get
      {
        return _Date;
      }
      set
      {
        if (_Date != value) {
          _Date = value;
          OnPropertyChanged();
        }
      }
    }

    [XmlIgnore]
    private DateTime _Date;

    #endregion

  }
}
