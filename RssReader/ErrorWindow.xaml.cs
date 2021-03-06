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
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace RssReader
{
  /// <summary>
  /// Interaction logic for ErrorWindow.xaml
  /// </summary>
  public partial class ErrorWindow : Window, INotifyPropertyChanged
  {
    public ErrorWindow()
    {
      InitializeComponent();
      this.DataContext = this;
    }

    public Exception Error
    {
      get
      {
        return _Error;
      }
      set
      {
        _Error = value;
        OnPropertyChanged("Summary");
        OnPropertyChanged("Trace");
      }
    }

    Exception _Error;

    public string Summary
    {
      get
      {
        return _Error.Message;
      }
    }

    public string Trace
    {
      get
      {
        return _Error.ToString();
      }
      set
      { 
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

    private void OK(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

  }
}
