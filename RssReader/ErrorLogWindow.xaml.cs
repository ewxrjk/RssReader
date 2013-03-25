using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
