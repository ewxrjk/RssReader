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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;

namespace RssReader
{
  /// <summary>
  /// View model for a subscription list
  /// </summary>
  public class SubscriptionListViewModel : INotifyPropertyChanged
  {
    public SubscriptionListViewModel(SubscriptionList sl, ListBox SubscriptionsWidget, ItemsControl EntriesWidget)
    {
      this._Subscriptions = sl;
      this.SubscriptionsWidget = SubscriptionsWidget;
      this.EntriesWidget = EntriesWidget;
      this.SubscriptionsWidget.Loaded += (sender, e) =>
      {
        UpdateSortOrder();
        UpdateFilter();
      };
      Subscriptions = new ObservableCollection<SubscriptionViewModel>();
      foreach (Subscription sub in _Subscriptions.Subscriptions) {
        Subscriptions.Add(new SubscriptionViewModel(sub));
      }
      _Subscriptions.SubscriptionAdded += SubscriptionAdded;
      _Subscriptions.SubscriptionRemoved += SubscriptionRemoved;
    }

    private ListBox SubscriptionsWidget;
    private ItemsControl EntriesWidget;

    private SubscriptionList _Subscriptions;

    public ObservableCollection<SubscriptionViewModel> Subscriptions { get; set; }

    private void SubscriptionAdded(Subscription subscription)
    {
      Subscriptions.Add(new SubscriptionViewModel(subscription));
    }

    private void SubscriptionRemoved(int index)
    {
      Subscriptions.RemoveAt(index);
    }

    #region Sorting

    private void UpdateSortOrder()
    {
      if (SubscriptionsWidget != null
         && SubscriptionsWidget.ItemsSource != null) {
        ICollectionView dataView = CollectionViewSource.GetDefaultView(SubscriptionsWidget.ItemsSource);
        dataView.SortDescriptions.Clear();
        if (Properties.Settings.Default.SortSubscriptionsByUnread) {
          dataView.SortDescriptions.Add(new SortDescription("UnreadEntries", ListSortDirection.Descending));
        }
        dataView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
      }
    }

    public bool SortByName
    {
      get
      {
        return Properties.Settings.Default.SortSubscriptionsByName;
      }
      set
      {
        if (value != Properties.Settings.Default.SortSubscriptionsByName) {
          Properties.Settings.Default.SortSubscriptionsByName = value;
          if (value == true) {
            Properties.Settings.Default.SortSubscriptionsByUnread = false;
            OnPropertyChanged("SortByUnread");
          }
          OnPropertyChanged();
          UpdateSortOrder();
          Properties.Settings.Default.Save();
        }
      }
    }

    public bool SortByUnread
    {
      get
      {
        return Properties.Settings.Default.SortSubscriptionsByUnread;
      }
      set
      {
        if (value != Properties.Settings.Default.SortSubscriptionsByUnread) {
          Properties.Settings.Default.SortSubscriptionsByUnread = value;
          if (value == true) {
            Properties.Settings.Default.SortSubscriptionsByName = false;
            OnPropertyChanged("SortByName");
          }
          OnPropertyChanged();
          UpdateSortOrder();
          Properties.Settings.Default.Save();
        }
      }
    }

    #endregion

    #region Filtering

    public bool ShowAll
    {
      get
      {
        return Properties.Settings.Default.ShowAllSubscriptions;
      }
      set
      {
        if (value != Properties.Settings.Default.ShowAllSubscriptions) {
          Properties.Settings.Default.ShowAllSubscriptions = value;
          OnPropertyChanged();
          OnPropertyChanged("ShowUnreadOnly");
          UpdateFilter();
          Properties.Settings.Default.Save();
        }
      }
    }

    public bool ShowUnreadOnly
    {
      get
      {
        return !Properties.Settings.Default.ShowAllSubscriptions;
      }
      set
      {
        ShowAll = !value;
      }
    }

    private void UpdateFilter()
    {
      if (SubscriptionsWidget != null
         && SubscriptionsWidget.ItemsSource != null) {
        ICollectionView dataView = CollectionViewSource.GetDefaultView(SubscriptionsWidget.ItemsSource);
        if (Properties.Settings.Default.ShowAllSubscriptions) {
          dataView.Filter = ShowAllFilter;
        }
        else {
          dataView.Filter = ShowUnreadOnlyFilter;
        }
      }
    }

    static private bool ShowAllFilter(object o)
    {
      return true;
    }

    static private bool ShowUnreadOnlyFilter(object o)
    {
      SubscriptionViewModel svm = o as SubscriptionViewModel;
      if (svm != null) {
        return svm.HasUnreadEntries;
      }
      else {
        return false;
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
