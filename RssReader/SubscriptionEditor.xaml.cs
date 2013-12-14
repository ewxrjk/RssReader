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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace RssReader
{
  /// <summary>
  /// Interaction logic for SubscriptionEditor.xaml
  /// </summary>
  public partial class SubscriptionEditor : Window, INotifyPropertyChanged
  {
    public SubscriptionEditor()
    {
      InitializeComponent();
      DataContext = this;
      NewURI.TextChanged += NewURI_TextChanged;
    }

    #region Commands

    public static RoutedCommand CancelCommand = new RoutedCommand();

    static SubscriptionEditor()
    {
      CancelCommand.InputGestures.Add(new KeyGesture(Key.Escape));
    }

    #endregion

    #region Public Fields And Properties

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
          OnPropertyChanged("ResetVisibility");
          OnPropertyChanged("CheckVisibility");
          OnPropertyChanged("OKText");
          if (_Subscription != null && _Subscription.Parent == null) {
            NewURI.Focus();
          }
        }
      }
    }

    public bool Accept = false;

    private ReaderLib.Subscription _Subscription = null;

    public Visibility ResetVisibility
    {
      get
      {
        return _Subscription != null && _Subscription.Parent != null ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Visibility CheckVisibility
    {
      get
      {
        return _Subscription != null && _Subscription.Parent == null ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public bool Checking
    {
      get
      {
        return _Checking;
      }
      set
      {
        if (_Checking != value) {
          _Checking = value;
          OnPropertyChanged();
          OnPropertyChanged("Checkable");
          OnPropertyChanged("Acceptable");
        }
      }
    }

    private bool _Checking = false;

    public string LastError
    {
      get
      {
        if (CheckFailed != null && LastChecked != null && NewURI.Text == LastChecked) {
          return CheckFailed;
        }
        else {
          return "";
        }
      }
    }

    public bool Checkable
    {
      get
      {
        Uri parsedUri;
        return !_Checking && Uri.TryCreate(NewURI.Text, UriKind.Absolute, out parsedUri);
      }
    }

    public bool Acceptable
    {
      get
      {
        return _Subscription.Parent != null || (Checking == false
                                                && LastChecked != null
                                                && NewURI.Text == LastChecked
                                                && _CheckFailed == null);
      }
    }

    public string OKText
    {
      get
      {
        return _Subscription.Parent != null ? "OK" : "Add";
      }
    }

    #endregion

    #region Layout

    private void Layout(PropertyInfo[] pis)
    {
      int npis = (pis != null ? pis.Length : 0);
      // Throw away old data
      GridWidget.RowDefinitions.Clear();
      GridWidget.Children.Clear();
      ResetList = new Dictionary<PropertyInfo, object>();
      // Generate the new layout
      if (_Subscription != null && _Subscription.Parent == null) {
        LayoutInput();
      }
      foreach (PropertyInfo pi in pis) {
        Layout(pi);
      }
      LayoutButtons();
    }

    private int LayoutNewRow()
    {
      int row = GridWidget.RowDefinitions.Count();
      GridWidget.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
      return row;
    }

    private void LayoutInput()
    {
      int row = LayoutNewRow();
      GridWidget.Children.Add(NewURILabel);
      Grid.SetRow(NewURILabel, row);
      GridWidget.Children.Add(NewURI);
      Grid.SetRow(NewURI, row);
      GridWidget.Children.Add(NewURIError);
      Grid.SetRow(NewURIError, LayoutNewRow());
    }

    private void Layout(PropertyInfo pi)
    {
      int row = LayoutNewRow();
      UserVisibleAttribute uva = pi.GetCustomAttribute<UserVisibleAttribute>();
      bool modifiable = uva.Modifiable && _Subscription.Parent != null;
      Label l = new Label()
      {
        Content = uva.Description != null ? uva.Description : pi.Name,
        Margin = new Thickness(2),
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Top,
      };
      GridWidget.Children.Add(l);
      Grid.SetRow(l, row);
      Grid.SetColumn(l, 0);
      if (uva.Type == "URI" && !modifiable) {
        TextBlock t = new TextBlock()
        {
          Margin = new Thickness(2),
          HorizontalAlignment = HorizontalAlignment.Left,
          VerticalAlignment = VerticalAlignment.Center,
        };
        Hyperlink hl = new Hyperlink();
        hl.SetBinding(Hyperlink.NavigateUriProperty,
                      new Binding()
                      {
                        Source = _Subscription,
                        Path = new PropertyPath(pi.Name),
                        Mode = BindingMode.OneWay,
                      });
        Run r = new Run();
        r.SetBinding(Run.TextProperty,
                    new Binding()
                    {
                      Source = _Subscription,
                      Path = new PropertyPath(pi.Name),
                      Mode = BindingMode.OneWay,
                    });
        hl.Inlines.Add(r);
        hl.RequestNavigate += RequestedNavigate;
        t.Inlines.Add(hl);
        GridWidget.Children.Add(t);
        Grid.SetRow(t, row);
        Grid.SetColumn(t, 1);
      }
      else {
        TextBox t = new TextBox()
        {
          IsReadOnly = !modifiable,
          Margin = new Thickness(2),
          HorizontalAlignment = HorizontalAlignment.Left,
          VerticalAlignment = VerticalAlignment.Center,
          MinWidth = 384,
        };
        if (!modifiable) {
          t.BorderThickness = new Thickness(0);
        }
        t.SetBinding(TextBox.TextProperty,
                     new Binding()
                     {
                       Source = _Subscription,
                       Path = new PropertyPath(pi.Name),
                       Mode = modifiable ? BindingMode.TwoWay : BindingMode.OneWay,
                     });
        GridWidget.Children.Add(t);
        Grid.SetRow(t, row);
        Grid.SetColumn(t, 1);
      }
      if (modifiable) {
        object original = pi.GetValue(_Subscription);
        ResetList[pi] = original is ICloneable ? ((ICloneable)original).Clone() : original;
      }
    }

    private void LayoutButtons()
    {
      GridWidget.Children.Add(ButtonContainer);
      Grid.SetRow(ButtonContainer, LayoutNewRow());
    }

    #endregion

    #region Reset

    private Dictionary<PropertyInfo, object> ResetList = null;

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

    #endregion

    #region Checking New Subscriptions

    private string LastChecked = null;

    private string CheckFailed
    {
      get
      {
        return _CheckFailed;
      }
      set
      {
        if (_CheckFailed != value) {
          _CheckFailed = value;
          OnPropertyChanged("LastError");
          OnPropertyChanged("Acceptable");
        }
      }
    }

    private string _CheckFailed;

    private void Check()
    {
      if (_Subscription != null && _Subscription.Parent == null && Checking == false) {
        CheckFailed = null;
        LastChecked = NewURI.Text;
        Checking = true;
        ThreadPool.QueueUserWorkItem((_) =>
        {
          try {
            ((WebSubscription)_Subscription).Configure(Dispatcher.Invoke, CheckError, new Uri(LastChecked));
          }
          catch (Exception e) {
            Dispatcher.Invoke(() => CheckError(e));
          }
          Dispatcher.Invoke(() => CheckComplete(_Subscription));
        });
      }
    }

    private void CheckError(Exception error)
    {
      CheckFailed = error.Message;
    }

    private void CheckComplete(Subscription subscription)
    {
      Checking = false;
    }

    #endregion

    #region Event Handlers

    private void Reset(object sender, RoutedEventArgs e)
    {
      Reset();
    }

    private void OK(object sender, RoutedEventArgs e)
    {
      if (Acceptable) {
        this.Close();
        Accept = true;
      }
    }

    private void CancelExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      if (_Subscription != null && _Subscription.Parent != null) {
        Reset();
      }
      this.Close();
    }

    private void Check(object sender, RoutedEventArgs e)
    {
      Check();
    }

    static private void RequestedNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Uri.ToString());
      e.Handled = true;
    }

    private void NewURI_TextChanged(object sender, TextChangedEventArgs e)
    {
      OnPropertyChanged("Acceptable");
      OnPropertyChanged("Checkable");
      OnPropertyChanged("LastError");
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

