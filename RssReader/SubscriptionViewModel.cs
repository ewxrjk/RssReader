﻿// This file is part of HalfMoon RSS reader
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace RssReader
{
  /// <summary>
  /// View model for a subscription
  /// </summary>
  public class SubscriptionViewModel: INotifyPropertyChanged
  {
    public SubscriptionViewModel(ReaderLib.Subscription Subscription)
    {
      this.Subscription = Subscription;
      foreach (Entry entry in from kvp in Subscription.Entries
                              orderby kvp.Value.Serial descending
                              select kvp.Value) {
        Entries.Add(CreateEntryViewModel(entry)); 
      }
      this.Subscription.EntryAdded += ModelEntryAdded;
      this.Subscription.PropertyChanged += ModelPropertyChanged;
    }

    public Subscription Subscription;

    private EntryViewModel CreateEntryViewModel(Entry entry)
    {
      EntryViewModel evm = new EntryViewModel(entry);
      evm.PropertyChanged += EntryModelPropertyChanged;
      return evm;
    }

    /// <summary>
    /// Entries in this subscription
    /// </summary>
    public ObservableCollection<EntryViewModel> Entries = new ObservableCollection<EntryViewModel>();

    #region Public Properties

    public string Title
    {
      get
      {
        return Subscription.Title;
      }
    }

    public string TitleTooltip
    {
      get
      {
        if (Subscription.Error != null) {
          return Subscription.Error.Message;
        } else if (UnreadEntries > 0) {
          return string.Format("{0} ({1} unread)", Subscription.Title, UnreadEntries);
        } else {
          return Subscription.Title;
        }
      }
    }

    public int UnreadEntries
    {
      get
      {
        return GetUnreadEntries().Count();
      }
    }

    public FontWeight TitleWeight
    {
      get
      {
        return UnreadEntries > 0 ? FontWeights.Bold : FontWeights.Normal;
      }
    }

    public Brush TitleBrush
    {
      get
      {
        return Subscription.Error == null ? Brushes.Black : Brushes.Red;
      }
    }

    public bool CanReadOnline
    {
      get
      {
        WebSubscription ws = Subscription as WebSubscription;
        return ws != null && ws.PublicURI != null;
      }
    }

    public bool HasUnreadEntries
    {
      get
      {
        return UnreadEntries > 0;
      }
    }

    public string PublicURI
    {
      get
      {
        WebSubscription ws = Subscription as WebSubscription;
        return ws != null ? ws.PublicURI : null;
      }
    }

    #endregion

    #region Commands

    public void ReadOnline()
    {
      System.Diagnostics.Process.Start(((WebSubscription)Subscription).PublicURI.ToString());
    }

    public IEnumerable<EntryViewModel> GetUnreadEntries()
    {
      return from entry in Entries
             where entry.Read == false
             select entry;
    }

    #endregion

    #region Notification Handling

    private void ModelEntryAdded(Entry entry)
    {
      Entries.Insert(0, CreateEntryViewModel(entry));
      OnPropertyChanged("UnreadEntries");
      OnPropertyChanged("HasUnreadEntries");
      OnPropertyChanged("TitleWeight");
    }

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName) {
        case "Title":
          OnPropertyChanged(e.PropertyName);
          OnPropertyChanged("TitleTooltip");
          break;
        case "PublicURI":
          OnPropertyChanged(e.PropertyName);
          OnPropertyChanged("CanReadOnline");
          break;
        case "Error":
          OnPropertyChanged("TitleBrush");
          OnPropertyChanged("TitleTooltip");
          break;
      }
    }

    private void EntryModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName) {
        case "Read":
          OnPropertyChanged("HasUnreadEntries");
          OnPropertyChanged("UnreadEntries");
          OnPropertyChanged("TitleWeight");
          OnPropertyChanged("TitleTooltip");
          break;
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
