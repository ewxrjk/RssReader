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
