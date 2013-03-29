using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReaderLib;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace RssReader
{
  /// <summary>
  /// View model for a subscription
  /// </summary>
  public class SubscriptionViewModel: INotifyPropertyChanged
  {
    public SubscriptionViewModel(Subscription subscription)
    {
      _Subscription = subscription;
      foreach (Entry entry in from kvp in _Subscription.Entries
                              orderby kvp.Value.Serial descending
                              select kvp.Value) {
        Entries.Add(CreateEntryViewModel(entry)); 
      }
      _Subscription.EntryAdded += ModelEntryAdded;
      _Subscription.PropertyChanged += ModelPropertyChanged;
    }

    private Subscription _Subscription;

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

    public string Title
    {
      get
      {
        return _Subscription.Title;
      }
    }

    public string TitleTooltip
    {
      get
      {
        if (UnreadEntries > 0) {
          return string.Format("{0} ({1} unread)", _Subscription.Title, UnreadEntries);
        } else {
          return _Subscription.Title;
        }
      }
    }

    public int UnreadEntries
    {
      get
      {
        return (from entry in Entries
                where entry.Read == false
                select entry).Count();
      }
    }

    public FontWeight TitleWeight
    {
      get
      {
        return UnreadEntries > 0 ? FontWeights.Bold : FontWeights.Normal;
      }
    }

    private void ModelEntryAdded(Entry entry)
    {
      Entries.Insert(0, CreateEntryViewModel(entry));
      OnPropertyChanged("UnreadEntries");
      OnPropertyChanged("TitleWeight");
    }

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName) {
        case "Title":
          OnPropertyChanged(e.PropertyName);
          break;
      }
    }

    private void EntryModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName) {
        case "Read":
          OnPropertyChanged("UnreadEntries");
          OnPropertyChanged("TitleWeight");
          OnPropertyChanged("TitleTooltip");
          break;
      }
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
