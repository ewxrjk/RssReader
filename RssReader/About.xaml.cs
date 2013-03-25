﻿using System;
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
  }
}
