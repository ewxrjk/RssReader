using ReaderLib;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using HTML = ReaderLib.HTML;

namespace RssReader
{
  /// <summary>
  /// View model for an entry
  /// </summary>
  public class EntryViewModel : INotifyPropertyChanged
  {
    public EntryViewModel(Entry entry)
    {
      _Entry = entry;
      _Entry.PropertyChanged += ModelPropertyChanged;
    }

    private Entry _Entry;

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName) {
        case "Title":
          OnPropertyChanged(e.PropertyName);
          break;
        case "Description":
          OnPropertyChanged("HTML");
          OnPropertyChanged(e.PropertyName);
          break;
        case "Read":
          OnPropertyChanged("TitleWeight");
          OnPropertyChanged(e.PropertyName);
          break;
      }
    }

    #region Public properties

    public string Title
    {
      get
      {
        return _Entry.Title;
      }
    }

    public string Description
    {
      get
      {
        return _Entry.Description;
      }
    }

    public HTML.Document HtmlDescription
    {
      get
      {
        return HTML.Document.Parse(Description);
      }
    }

    public bool Read
    {
      get
      {
        return _Entry.Read;
      }
      set
      {
        _Entry.Read = value;
      }
    }

    public FontWeight TitleWeight
    {
      get
      {
        return _Entry.Read ? FontWeights.Normal : FontWeights.Bold;
      }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string propertyName)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    #endregion

  }
}
