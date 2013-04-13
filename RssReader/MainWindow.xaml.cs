using Microsoft.Win32;
using ReaderLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RssReader
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged
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
      ApplicationCommands.Undo.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control));
      ApplicationCommands.Redo.InputGestures.Add(new KeyGesture(Key.Y, ModifierKeys.Control));
    }

    #endregion

    #region Initialization

    public MainWindow()
      : this(null)
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
      Subscriptions = new SubscriptionListViewModel(_Subscriptions, SubscriptionsWidget, EntriesWidget);
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
      SubscriptionEditor editor = new SubscriptionEditor()
      {
        Subscription = new WebSubscription(),
        Owner = this,
      };
      editor.ShowDialog();
      if (editor.Accept) {
        Subscription sub = editor.Subscription;
        Perform(new Performable()
        {
          Description = string.Format("subscribe to {0}", sub.Title),
          Redo = () => _Subscriptions.Add(sub),
          Undo = () => _Subscriptions.Remove(sub)
        });
      }
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
            Subscription[] newSubscriptions = (from sub in importer.GetSubscriptions()
                                            where !_Subscriptions.Contains(sub)
                                            select sub).ToArray();
            if (newSubscriptions.Length > 0) {
              Perform(new Performable()
              {
                Description = (newSubscriptions.Length == 1
                               ? string.Format("subscribe to {0}", newSubscriptions[0].Title)
                               : string.Format("{0} subscriptions", newSubscriptions.Length)),
                Redo = () =>
                {
                  foreach (Subscription sub in newSubscriptions) {
                    _Subscriptions.Add(sub);
                  }
                },
                Undo = () =>
                {
                  foreach (Subscription sub in newSubscriptions) {
                    _Subscriptions.Remove(sub);
                  }
                }
              });
            }
          }
        }
      }
      catch (Exception error) {
        Error(error);
      }
    }

    private void OpenErrorLogExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      Errors.OpenWindow();
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

    #region Edit Menu

    private void UndoExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      if (UndoStack.Count > 0) {
        Performable p = UndoStack.Pop();
        RedoStack.Push(p);
        p.Undo();
        OnPropertyChanged("UndoDescription");
        OnPropertyChanged("UndoEnabled");
        OnPropertyChanged("RedoDescription");
        OnPropertyChanged("RedoEnabled");
      }
    }

    private void RedoExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      if (RedoStack.Count > 0) {
        Performable p = RedoStack.Pop();
        UndoStack.Push(p);
        p.Redo();
        OnPropertyChanged("UndoDescription");
        OnPropertyChanged("UndoEnabled");
        OnPropertyChanged("RedoDescription");
        OnPropertyChanged("RedoEnabled");
      }
    }

    #endregion

    #region Help Menu

    private void AboutExecuted(object sender, RoutedEventArgs e)
    {
      About.OpenWindow(this);
    }

    #endregion

    #region Subscription list

    private SubscriptionViewModel OldSubscriptionViewModel = null;

    private void SelectSubscription(object sender, SelectionChangedEventArgs e)
    {
      OnPropertyChanged("SelectedSubscriptionViewModel");
      OnPropertyChanged("SubscriptionSelected");
      OnPropertyChanged("SubscriptionCanReadOnline");
      OnPropertyChanged("SubscriptionHasUnreadEntries");
      if (OldSubscriptionViewModel != null) {
        SelectedSubscriptionViewModel.PropertyChanged -= SubscriptionViewModelPropertyChanged;
      }
      if (SelectedSubscriptionViewModel != null) {
        SelectedSubscriptionViewModel.PropertyChanged += SubscriptionViewModelPropertyChanged;
      }
      object selected = SubscriptionsWidget.SelectedItem;
      EntriesWidget.ItemsSource = selected != null ? ((SubscriptionViewModel)selected).Entries : null;
      EntriesScrollViewer.ScrollToHome();
    }

    private void SubscriptionViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      OnPropertyChanged("Subscription" + e.PropertyName);
    }

    #endregion

    #region Subscription Context Menu

    public SubscriptionViewModel SelectedSubscriptionViewModel
    {
      get
      {
        if (SubscriptionsWidget.SelectedIndex != -1) {
          return (SubscriptionViewModel)SubscriptionsWidget.SelectedItem;
        }
        else {
          return null;
        }
      }
    }

    private void ReadSubscriptionOnline(object sender, RoutedEventArgs e)
    {
      SelectedSubscriptionViewModel.ReadOnline();
    }

    private void MarkSubscriptionRead(object sender, RoutedEventArgs e)
    {
      // We need to tie down the things affected, i.e. and not use anything
      // mutable inside the closures.
      Subscription subscription = SelectedSubscriptionViewModel.Subscription;
      EntryViewModel[] affectedEntries = SelectedSubscriptionViewModel.GetUnreadEntries().ToArray();
      Perform(new Performable()
      {
        Description = string.Format("mark “{0}” read", subscription.Title),
        Redo = () => 
        {
          foreach (EntryViewModel evm in affectedEntries) {
            evm.Read = true;
          }
        },
        Undo = () =>
        {
          foreach (EntryViewModel evm in affectedEntries) {
            evm.Read = false;
          }
        }
      });
    }

    private void EditSubscription(object sender, RoutedEventArgs e)
    {
      SubscriptionEditor editor = new SubscriptionEditor()
      {
        Subscription = SelectedSubscriptionViewModel.Subscription,
        Owner = this
      };
      editor.ShowDialog();
    }

    private void UnsubscribeSubscription(object sender, RoutedEventArgs e)
    {
      Subscription subscription = SelectedSubscriptionViewModel.Subscription;
      Perform(new Performable()
      {
        Description = string.Format("unsubscribe from “{0}”", subscription.Title),
        Redo = () => _Subscriptions.Remove(subscription),
        Undo = () => _Subscriptions.Add(subscription)
      });
    }

    public bool SubscriptionSelected
    {
      get
      {
        return SubscriptionsWidget.SelectedIndex != -1;
      }
    }

    public bool SubscriptionCanReadOnline
    {
      get
      {
        SubscriptionViewModel svm = SelectedSubscriptionViewModel;
        return svm != null ? svm.CanReadOnline : false;
      }
    }

    public bool SubscriptionHasUnreadEntries
    {
      get
      {
        SubscriptionViewModel svm = SelectedSubscriptionViewModel;
        return svm != null ? svm.HasUnreadEntries : false;
      }
    }

    #endregion

    #region Undo and Redo

    Stack<Performable> UndoStack = new Stack<Performable>();

    Stack<Performable> RedoStack = new Stack<Performable>();

    public void Perform(Performable p)
    {
      RedoStack.Clear();
      UndoStack.Push(p);
      p.Redo();
      OnPropertyChanged("UndoDescription");
      OnPropertyChanged("UndoEnabled");
      OnPropertyChanged("RedoDescription");
      OnPropertyChanged("RedoEnabled");
    }

    public string UndoDescription
    {
      get
      {
        return UndoStack.Count > 0 ? string.Format("Undo {0}", UndoStack.Peek().Description) : "Undo";
      }
    }

    public bool UndoEnabled { get { return UndoStack.Count > 0; } }

    public string RedoDescription
    {
      get
      {
        return RedoStack.Count > 0 ? string.Format("Redo {0}", RedoStack.Peek().Description) : "Redo";
      }
    }

    public bool RedoEnabled { get { return RedoStack.Count > 0; } }

    #endregion

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
