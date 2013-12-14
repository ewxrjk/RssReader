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
using System.Linq;
using System.Reflection;
using System.Windows;

namespace RssReader
{
  /// <summary>
  /// Interaction logic for About.xaml
  /// </summary>
  public partial class About : Window
  {
    public About()
    {
      InitializeComponent();
      this.DataContext = this;
    }

    private void OK(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void VisitHomePage(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(link.NavigateUri.ToString());
      e.Handled = true;
    }

    static public void OpenWindow(Window parent)
    {
      About about = new About()
      {
        Owner = parent
      };
      about.ShowDialog();
    }

    public string Version
    {
      get
      {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
      }
    }

    public string Copyright
    {
      get
      {
        return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
      }

    }
  }
}
