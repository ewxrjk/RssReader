using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using ReaderLib;
using System.Collections.ObjectModel;

namespace RssReader
{
  /// <summary>
  /// Interaction logic for SubscriptionEditor.xaml
  /// </summary>
  public partial class SubscriptionEditor : Window
  {
    public SubscriptionEditor()
    {
      InitializeComponent();
    }

    public ReaderLib.Subscription Subscription
    {
      get
      {
        return _Subscription;
      }
      set
      {
        if (value != _Subscription) {
          _Subscription = value;
          Layout(_Subscription != null ? _Subscription.GetUserVisibleProperties() : null);
        }
      }
    }

    private ReaderLib.Subscription _Subscription = null;

    private Dictionary<PropertyInfo, object> ResetList = null;

    private void Layout(PropertyInfo[] pis)
    {
      int npis = (pis != null ? pis.Length : 0);
      // Throw away old data
      GridWidget.RowDefinitions.Clear();
      GridWidget.Children.Clear();
      ResetList = new Dictionary<PropertyInfo, object>();
      // Generate the new layout
      for(int i = 0; i < npis; ++i) {
        PropertyInfo pi = pis[i];
        UserVisibleAttribute uva = pi.GetCustomAttribute<UserVisibleAttribute>();
        GridWidget.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
        Label l = new Label()
        {
          Content = uva.Description != null ? uva.Description : pi.Name,
          Margin = new Thickness(2),
          HorizontalAlignment = HorizontalAlignment.Right,
          VerticalAlignment = VerticalAlignment.Top,
        };
        GridWidget.Children.Add(l);
        Grid.SetRow(l, i);
        Grid.SetColumn(l, 0);
        TextBox t = new TextBox() { 
          IsReadOnly = !uva.Modifiable,
          Margin = new Thickness(2),
          HorizontalAlignment = HorizontalAlignment.Left,
          VerticalAlignment = VerticalAlignment.Center,
          MinWidth = 384,
        };
        if (!uva.Modifiable) {
          t.BorderThickness = new Thickness(0);
        }
        t.SetBinding(TextBox.TextProperty,
                     new Binding()
                     {
                       Source = _Subscription,
                       Path = new PropertyPath(pi.Name),
                       Mode = uva.Modifiable ? BindingMode.TwoWay : BindingMode.OneWay,
                     });
        GridWidget.Children.Add(t);
        Grid.SetRow(t, i);
        Grid.SetColumn(t, 1);
        if (uva.Modifiable) {
          object original = pi.GetValue(_Subscription);
          ResetList[pi] = original is ICloneable ? ((ICloneable)original).Clone() : original;
        }
      }
      GridWidget.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
      GridWidget.Children.Add(ButtonContainer);
      Grid.SetRow(ButtonContainer, npis);
    }

    private void Reset()
    {
      if (ResetList != null && _Subscription != null) {
        foreach (KeyValuePair<PropertyInfo, object> kvp in ResetList) {
          PropertyInfo pi = kvp.Key;
          object original = kvp.Value;
          pi.SetValue(_Subscription, original);
        }
      }
    }

    #region Event Handlers

    private void Reset(object sender, RoutedEventArgs e)
    {
      Reset();
    }

    private void OK(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    #endregion
  }
}
