// This file is part of HalfMoon RSS reader
// Copyright (C) 2013 Richard Kettlewell
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
