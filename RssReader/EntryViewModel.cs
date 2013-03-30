﻿using System;
using ReaderLib;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using HTML = ReaderLib.HTML;
using System.Windows.Documents;
using System.Windows.Data;
using System.Windows.Media;
using System.Runtime.CompilerServices;

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

    public string Date
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

    #endregion

    public FrameworkElement Rendered(ScrollViewer scrollViewer)
    {
      EntryDisplay sp = new EntryDisplay()
      {
        DataContext = this
      };
      sp.Panel.Children.Add(RenderHTML.Render(HtmlDescription, scrollViewer));
      return sp;
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
