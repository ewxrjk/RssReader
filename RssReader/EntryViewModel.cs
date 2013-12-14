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
using ReaderLib;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
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
        case "Date":
          OnPropertyChanged(e.PropertyName);
          OnPropertyChanged("DateString");
          break;
        case "Title":
        case "URI":
          OnPropertyChanged(e.PropertyName);
          break;
        case "Description":
          OnPropertyChanged("HtmlDescription");
          OnPropertyChanged(e.PropertyName);
          break;
        case "Read":
          OnPropertyChanged("TitleWeight");
          OnPropertyChanged("Unread");
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

    public bool Unread
    {
      get
      {
        return !_Entry.Read;
      }
      set
      {
        _Entry.Read = !value;
      }
    }

    public FontWeight TitleWeight
    {
      get
      {
        return _Entry.Read ? FontWeights.Normal : FontWeights.Bold;
      }
    }

    public Uri URI
    {
      get
      {
        WebEntry e = _Entry as WebEntry;
        Uri result;
        return e != null && Uri.TryCreate(e.URI, UriKind.Absolute, out result) ? result : null;
      }
    }

    public string DateString
    {
      get
      {
        WebEntry e = _Entry as WebEntry;
        if (e != null && e.Date != null) {
          DateTime now = DateTime.Now;
          if (now.Date == e.Date.Date) {
            return e.Date.ToString("HH:mm:ss: ");
          }
          else {
            return e.Date.ToString("yyyy-MM-dd: ");
          }
        }
        else {
          return "";
        }
      }
    }

    public DateTime Date
    {
      get
      {
        WebEntry e = _Entry as WebEntry;
        if (e != null && e.Date != null) {
          return e.Date;
        }
        else {
          return new DateTime();
        }
      }
    }

    public int Serial
    {
      get
      {
        return _Entry.Serial;
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
