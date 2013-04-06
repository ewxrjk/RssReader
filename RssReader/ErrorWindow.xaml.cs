using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace RssReader
{
  /// <summary>
  /// Interaction logic for ErrorWindow.xaml
  /// </summary>
  public partial class ErrorWindow : Window, INotifyPropertyChanged
  {
    public ErrorWindow()
    {
      InitializeComponent();
      this.DataContext = this;
    }

    public Exception Error
    {
      get
      {
        return _Error;
      }
      set
      {
        _Error = value;
        OnPropertyChanged("Summary");
        OnPropertyChanged("Trace");
      }
    }

    Exception _Error;

    public string Summary
    {
      get
      {
        return _Error.Message;
      }
    }

    public string Trace
    {
      get
      {
        return _Error.ToString();
      }
      set
      { 
      }
    }

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

    private void OK(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

  }
}
