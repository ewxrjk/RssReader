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
      SortOrder = SortBy.Unread;
      ShowAll = true;
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

    public enum SortBy
    {
      Name,
      Unread,
    };

    public SortBy SortOrder
    {
      get
      {
        return _SortOrder;
      }
      set
      {
        if (value != _SortOrder) {
          _SortOrder = value;
          OnPropertyChanged();
          OnPropertyChanged("SortByName");
          OnPropertyChanged("SortByUnread");
          UpdateSortOrder();
        }
      }
    }

    private SortBy _SortOrder = SortBy.Name;

    private void UpdateSortOrder()
    {
      if (SubscriptionsWidget != null
         && SubscriptionsWidget.ItemsSource != null) {
        ICollectionView dataView = CollectionViewSource.GetDefaultView(SubscriptionsWidget.ItemsSource);
        dataView.SortDescriptions.Clear();
        switch (_SortOrder) {
          case SortBy.Name:
            break;
          case SortBy.Unread:
            dataView.SortDescriptions.Add(new SortDescription("UnreadEntries", ListSortDirection.Descending));
            break;
        }
        dataView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
      }
    }

    public bool SortByName
    {
      get
      {
        return SortOrder == SortBy.Name;
      }
      set
      {
        SortOrder = SortBy.Name;
      }
    }

    public bool SortByUnread
    {
      get
      {
        return SortOrder == SortBy.Unread;
      }
      set
      {
        SortOrder = SortBy.Unread;
      }
    }

    #endregion

    #region Filtering

    public bool ShowAll
    {
      get
      {
        return !ShowUnreadOnly;
      }
      set
      {
        ShowUnreadOnly = !value;
      }
    }

    public bool ShowUnreadOnly
    {
      get
      {
        return _ShowUnreadOnly;
      }
      set
      {
        if (value != _ShowUnreadOnly) {
          _ShowUnreadOnly = value;
          OnPropertyChanged();
          OnPropertyChanged("ShowAll");
          UpdateFilter();
        }
      }
    }

    private void UpdateFilter()
    {
      if (SubscriptionsWidget != null
         && SubscriptionsWidget.ItemsSource != null) {
        ICollectionView dataView = CollectionViewSource.GetDefaultView(SubscriptionsWidget.ItemsSource);
        if (ShowUnreadOnly) {
          dataView.Filter = ShowUnreadOnlyFilter;
        }
        else {
          dataView.Filter = ShowAllFilter;
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

    private bool _ShowUnreadOnly;

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
