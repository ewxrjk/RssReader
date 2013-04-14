using System.Linq;
using System.Reflection;
using System.Windows;

namespace RssReader
{
  /// <summary>
  /// Interaction logic for About.xaml
  /// </summary>
  public partial class About : Window
  {
    public About()
    {
      InitializeComponent();
      this.DataContext = this;
    }

    private void OK(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void VisitHomePage(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(link.NavigateUri.ToString());
      e.Handled = true;
    }

    static public void OpenWindow(Window parent)
    {
      About about = new About()
      {
        Owner = parent
      };
      about.ShowDialog();
    }

    public string Version
    {
      get
      {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
      }
    }

    public string Copyright
    {
      get
      {
        return (Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute)).First() as AssemblyCopyrightAttribute).Copyright;
      }

    }
  }
}
