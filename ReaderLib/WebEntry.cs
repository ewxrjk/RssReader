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
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ReaderLib
{
  public partial class WebEntry : Entry, INotifyPropertyChanged
  {
    #region Public Properties

    [UserVisible]
    public string URI
    {
      get
      {
        return _URI;
      }
      set
      {
        if (_URI != value) {
          _URI = value;
          OnPropertyChanged();
        }
      }
    }

    [XmlIgnore]
    private string _URI = null;

    [UserVisible]
    public DateTime Date
    {
      get
      {
        return _Date;
      }
      set
      {
        if (_Date != value) {
          _Date = value;
          OnPropertyChanged();
        }
      }
    }

    [XmlIgnore]
    private DateTime _Date;

    #endregion

  }
}
