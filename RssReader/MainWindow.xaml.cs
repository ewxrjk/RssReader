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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReaderLib;
using Microsoft.Win32;
using System.IO;

namespace RssReader
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    #region Commands

    public static RoutedCommand ExitCommand = new RoutedCommand();
    public static RoutedCommand SubscribeCommand = new RoutedCommand();
    public static RoutedCommand ImportCommand = new RoutedCommand();
    public static RoutedCommand OpenErrorLogCommand = new RoutedCommand();
    public static RoutedCommand AboutCommand = new RoutedCommand();

    static MainWindow()
    {
      ApplicationCommands.Close.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
      ExitCommand.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
      ImportCommand.InputGestures.Add(new KeyGesture(Key.I, ModifierKeys.Control));
      SubscribeCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
    }

    #endregion

    #region Initialization

    public MainWindow(): this(null)
    {
    }

    public MainWindow(SubscriptionList sl)
    {
      InitializeComponent();
      if (sl == null) {
        try {
          sl = SubscriptionList.LoadDefault();
        }
        catch (Exception e) {
          Error(e);
          this.Close();
        }
      }
      _Subscriptions = sl;
      _Subscriptions.BackgroundRegister(Dispatch, Error);
      Subscriptions = new SubscriptionListViewModel(_Subscriptions);
      // http://msdn.microsoft.com/en-us/library/ms745786.aspx for how to make listview sortable!
      this.DataContext = this;
    }

    public SubscriptionListViewModel Subscriptions { get; set; }

    private SubscriptionList _Subscriptions;

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      _Subscriptions.BackgroundDeregister();
    }

    #endregion

    #region Dispatch and Error Reporting

    /// <summary>
    /// Log of all errors encountered
    /// </summary>
    public ErrorLog Errors
    {
      get
      {
        return _Errors;
      }
    }

    public readonly ErrorLog _Errors = new ErrorLog();

    /// <summary>
    /// Run an action in the UI thread
    /// </summary>
    /// <param name="action"></param>
    public void Dispatch(Action action)
    {
      Dispatcher.Invoke(action);
    }

    /// <summary>
    /// Report an error
    /// </summary>
    /// <param name="error"></param>
    public void Error(Exception error)
    {
      if (error is SubscriptionException) {
        Errors.Add(error);
      }
      else {
        Errors.Add(error);
        // Pop up an error description
        ErrorWindow ew = new ErrorWindow()
        {
          Error = error,
          Owner = this
        };
        ew.ShowDialog();
      }
    }

    #endregion

    #region File Menu

    private void SubscribeExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      Error(new Exception("TODO"));
    }

    private void ImportExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      try {
        OpenFileDialog openFileDialog = new OpenFileDialog()
        {
          CheckFileExists = true,
          CheckPathExists = true,
          DereferenceLinks = true,
          Multiselect = false,
          Title = "Import subscriptions",
          Filter = "XML files|*.xml",
        };
        bool? result = openFileDialog.ShowDialog(this);
        if (result == true) {
          using (FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open)) {
            GoogleSubscriptions importer = new GoogleSubscriptions(stream);
            foreach(Subscription sub in importer.GetSubscriptions()) {
              _Subscriptions.Add(sub);
            }
          }
        }
      }
      catch (Exception error) {
        Error(error);
      }
    }

    private void ExitExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      Environment.Exit(0);
    }

    private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      this.Close();
    }

    #endregion

    #region Tools Menu

    private void OpenErrorLogExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      Errors.OpenWindow();
    }

    #endregion

    #region Help Menu

    private void AboutExecuted(object sender, RoutedEventArgs e)
    {
      About.OpenWindow(this);
    }

    #endregion

    #region Subscription list

    private void SelectSubscription(object sender, SelectionChangedEventArgs e)
    {
      object selected = SubscriptionsWidget.SelectedItem;
      if (selected == EntriesWidget.ItemsSource) {
        return;
      }
      EntriesWidget.ItemsSource = ((SubscriptionViewModel)selected).Entries;
    }

    #endregion

    private void EntryExpanded(object sender, RoutedEventArgs e)
    {
      // We only render the content when the user wants to read it
      // - rendering all the content for an entire feed up front is
      // much too expensive.
      Expander expander = (sender as Expander);
      if (expander != null) {
        if (expander.Content == null) {
          EntryViewModel evm = expander.Tag as EntryViewModel;
          if (evm != null) {
            evm.PropertyChanged += EntryPropertyChanged;
            expander.Content = evm.Rendered(EntriesScrollViewer);
            evm.Read = true;
          }
        }
      }
    }

    private void EntryPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "HtmlDescription") {
        foreach (var item in EntriesWidget.Items) {
          Expander expander = item as Expander;
          if (expander != null && expander.Tag == this) {
            EntryViewModel evm = expander.Tag as EntryViewModel;
            if (expander.IsExpanded) {
              expander.Content = evm.Rendered(EntriesScrollViewer);
            }
            else {
              evm.PropertyChanged -= EntryPropertyChanged;
              expander.Content = null;
            }
            return;
          }
        }
      }
    }


  }
}
