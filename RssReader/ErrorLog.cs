using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RssReader
{

  public class ErrorLog: INotifyPropertyChanged
  {
    public ErrorLog()
    {
      Errors.CollectionChanged += OnCollectionChanged;
    }

    #region Actions

    /// <summary>
    /// Add an error to the log
    /// </summary>
    /// <param name="error"></param>
    public void Add(Exception error)
    {
      Errors.Add(new ErrorViewModel()
      {
        TimeStamp = DateTime.Now,
        Error = error,
      });
    }

    /// <summary>
    /// Clear all errors
    /// </summary>
    public void Clear()
    {
      Errors.Clear();
    }

    #endregion

    #region Public properties

    /// <summary>
    /// A simple status string, for use in the main menu
    /// </summary>
    public string Status
    {
      get
      {
        if (Errors.Count == 0) {
          return "";
        }
        else {
          return string.Format("{0} errors", Errors.Count);
        }
      }
    }

    /// <summary>
    /// The error list
    /// </summary>
    public ObservableCollection<ErrorViewModel> Errors
    {
      get
      {
        return _Errors;
      }
    }

    private ObservableCollection<ErrorViewModel> _Errors = new ObservableCollection<ErrorViewModel>();

    private void OnCollectionChanged(Object sender,
                                    NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action) {
        case NotifyCollectionChangedAction.Add:
        case NotifyCollectionChangedAction.Reset:
        case NotifyCollectionChangedAction.Remove:
          OnPropertyChanged("Status");
          break;
      }
    }

    #endregion

    #region Window Lifetime

    /// <summary>
    /// The error window or null
    /// </summary>
    public ErrorLogWindow CurrentWindow = null;

    /// <summary>
    /// Open the error log window
    /// </summary>
    public void OpenWindow()
    {
      if (CurrentWindow == null) {
        CurrentWindow = new ErrorLogWindow()
        {
          Errors = this
        };
        CurrentWindow.Show();
      }
      else {
        CurrentWindow.Activate();
      }
    }

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
