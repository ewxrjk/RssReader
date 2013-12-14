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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace RssReader
{
  /// <summary>
  /// Interaction logic for EntryDisplay.xaml
  /// </summary>
  public partial class EntryDisplay : UserControl
  {
    public EntryDisplay()
    {
      InitializeComponent();
    }

    // TODO this is duplicate a bit...
    private void RequestedNavigate(object sender, RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Uri.ToString());
      e.Handled = true;
    }

    private EntryViewModel Model { get { return (EntryViewModel)DataContext; } }

    #region Entry Expansion

    static private ScrollViewer FindScrollViewer(FrameworkElement element)
    {
      // This is a bit of a hack...
      ScrollViewer sv;
      while ((sv = element as ScrollViewer) == null) {
        element = (FrameworkElement)VisualTreeHelper.GetParent(element);
      }
      return sv;
    }

    private void SetBody()
    {
      RenderHTML renderer = new RenderHTML()
      {
        ParentScrollViewer = FindScrollViewer(Expander),
        BaseUri = Model.URI,
      };
      FrameworkElement newContent = renderer.Render(Model.HtmlDescription);
      if (HasBody()) {
        Panel.Children[1] = newContent;
      }
      else {
        Panel.Children.Add(newContent);
      }
    }

    private void RemoveBody()
    {
      if (HasBody()) {
        Panel.Children.RemoveAt(1);
      }
    }

    private bool HasBody()
    {
      return Panel.Children.Count > 1;
    }

    private void EntryExpanded(object sender, RoutedEventArgs e)
    {
      // We only render the content when the user wants to read it
      // - rendering all the content for an entire subscription up front is
      // much too expensive.
      if (!HasBody()) {
        Model.PropertyChanged += EntryPropertyChanged;
        Model.Read = true;
        SetBody();
      }
    }

    private void EntryPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "HtmlDescription" && HasBody()) {
        if(Expander.IsExpanded) {
          SetBody();
        }
        else {
          // Obsolete invisible content is discarded
          Model.PropertyChanged -= EntryPropertyChanged;
          RemoveBody();
        }
      }
    }

    #endregion

    #region Entry Context Menu

    private void ReadEntryOnline(object sender, RoutedEventArgs e)
    {
      System.Diagnostics.Process.Start(Model.URI.ToString());
    }

    private void CopyEntryUri(object sender, RoutedEventArgs e)
    {
      Clipboard.Copy(Model.URI.ToString());
    }

    private void MarkEntryRead(object sender, RoutedEventArgs e)
    {
      Model.Read = true;
    }

    private void MarkEntryUnread(object sender, RoutedEventArgs e)
    {
      Model.Read = false;
    }

    #endregion

  }
}
