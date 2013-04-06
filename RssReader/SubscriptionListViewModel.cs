using ReaderLib;
using System.Collections.ObjectModel;

namespace RssReader
{
  /// <summary>
  /// View model for a subscription list
  /// </summary>
  public class SubscriptionListViewModel
  {
    public SubscriptionListViewModel(SubscriptionList sl)
    {
      _Subscriptions = sl;
      Subscriptions = new ObservableCollection<SubscriptionViewModel>();
      foreach (Subscription sub in _Subscriptions.Subscriptions) {
        Subscriptions.Add(new SubscriptionViewModel(sub));
      }
      _Subscriptions.SubscriptionAdded += SubscriptionAdded;
      _Subscriptions.SubscriptionRemoved += SubscriptionRemoved;
    }

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

  }
}
