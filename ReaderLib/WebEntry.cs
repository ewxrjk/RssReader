using System;
using System.ComponentModel;
using System.Xml.Serialization;

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
