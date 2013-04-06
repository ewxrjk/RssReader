using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace RssReader
{
  /// <summary>
  /// Interaction logic for ErrorLogWindow.xaml
  /// </summary>
  public partial class ErrorLogWindow : Window, INotifyPropertyChanged
  {
    public ErrorLogWindow()
    {
      this.DataContext = this;
      InitializeComponent();
    }

    public ErrorLog Errors
    {
      get
      {
        return _Errors;
      }
      set
      {
        _Errors = value;
        OnPropertyChanged();
      }
    }

    private ErrorLog _Errors;

    private void Window_Closed(object sender, EventArgs e)
    {
      Errors.CurrentWindow = null;
    }

    private void OK(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void Clear(object sender, RoutedEventArgs e)
    {
      _Errors.Clear();
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

  }
}
