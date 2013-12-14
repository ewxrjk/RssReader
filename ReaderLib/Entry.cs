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
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// Data for a single subscription entry
  /// </summary>
  [XmlInclude(typeof(WebEntry))]
  public class Entry : UniquelyIdentifiable, INotifyPropertyChanged
  {
    /// <summary>
    /// Containing subscription
    /// </summary>
    [XmlIgnore]
    public Subscription ParentSubscription;

    /// <summary>
    /// Containing list
    /// </summary>
    /// <remarks>We only need this to set the <c>Dirty</c> flag.</remarks>
    [XmlIgnore]
    public EntryList ParentEntryList;

    /// <summary>
    /// Entry title
    /// </summary>
    /// <remarks>Used in summary of subscription contents.</remarks>
    public string Title
    {
      get
      {
        return _Title;
      }
      set
      {
        if (_Title != value) {
          _Title = value;
          OnPropertyChanged();
        }
      }
    }

    [XmlIgnore]
    private string _Title = null;

    /// <summary>
    /// Entry description
    /// </summary>
    /// <remarks>The full text of the entry.  Currently assumed to be HTML, which is all that's required,
    /// but maybe more flexibility would be appropriate.</remarks>
    public string Description
    {
      get
      {
        return _Description;
      }
      set
      {
        if (_Description != value) {
          _Description = value;
          OnPropertyChanged();
        }
      }
    }

    [XmlIgnore]
    private string _Description = null;

    /// <summary>
    /// Entry read/unread state
    /// </summary>
    [XmlAttribute("Read")]
    public bool Read
    {
      get
      {
        return _Read;
      }
      set
      {
        if (_Read != value) {
          _Read = value;
          OnPropertyChanged();
          ParentPropertyChanged("UnreadEntries");
        }
      }
    }

    private bool _Read = false;

    /// <summary>
    /// Entry serial number
    /// </summary>
    [XmlAttribute("Serial")]
    public int Serial;

    /// <summary>
    /// Notify the parent subscription that a subscription property changed
    /// </summary>
    /// <param name="propertyName"></param>
    /// <remarks>For example, if one entry's read state changes then the
    /// subscription's unread count must change to match.</remarks>
    private void ParentPropertyChanged(string propertyName)
    {
      if (ParentSubscription != null) {
        ParentSubscription.OnPropertyChanged(propertyName);
      }
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName]string propertyName = "")
    {
      if (ParentEntryList != null) {
        ParentEntryList.Dirty = true;
      }
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    #endregion

  }
}
