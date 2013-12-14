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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// A subscription of some kind
  /// </summary>
  [XmlInclude(typeof(WebSubscription))]
  public abstract class Subscription: UniquelyIdentifiable, INotifyPropertyChanged
  {
    public Subscription()
    {
      _Title = null;
    }

    #region Virtual methods

    /// <summary>
    /// Update this subscription
    /// </summary>
    /// <param name="dispatch">Callback method to perform updates</param>
    /// <param name="error">Callback method to report errors</param>
    /// <returns><c>true</c> if anything was changed.</returns>
    /// <remarks>
    /// <para>This method is called in a background thread.  It must
    /// perform any accesses to the subscription or the entities it contains
    /// via the dispatch argument.</para>
    /// <para><c>_EntryState</c> and <c>_EntryData</c> will already exist when it runs.</para>
    /// <para>If any entries are added to <c>_EntryState</c> or _EntryData</c> then
    /// <c>OnPropertyChanged</c> must be called with the name of the corresponding
    /// public property (via <param name="dispatch".</para>
    /// </remarks>
    /// <exception cref="SubscriptionFetchingException">The underlying subscription data could not be fetched.</exception>
    /// <exception cref="SubscriptionParsingException">The underlying subscription data was malformed.
    /// This should only be thrown for whole-subscription problems; if one entry among many is
    /// broken then that should be reported via <paramref name="error"/>.</exception>
    abstract public void Update(Action<Action> dispatch,
                                Action<Exception> error);

    /// <summary>
    /// Return the suggested next update time
    /// </summary>
    /// <returns></returns>
    /// <remarks>This is run in the foregroud (dispatcher) thread.</remarks>
    virtual public DateTime NextUpdate()
    {
      // TODO make this user-configurable
      return LastCheck.AddHours(1);
    }

    #endregion

    #region Public Fields And Properties

    /// <summary>
    /// Subscription title
    /// </summary>
    [UserVisible(Modifiable = false, Priority = 0)]
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
    /// When the subscription was last checked
    /// </summary>
    [XmlIgnore]
    [UserVisible(Description = "Last checked", Modifiable = false, Priority = 256)]
    public DateTime LastCheck {
      get {
        Load();
        return _Entries.LastCheck;
      }
    }

    /// <summary>
    /// Last error for this subscription
    /// </summary>
    [XmlIgnore]
    public Exception Error
    {
      get { return _Error; }
      set
      {
        if (_Error != value) {
          _Error = value;
          OnPropertyChanged();
          OnPropertyChanged("ErrorString");
        }
      }
    }

    [XmlIgnore]
    [UserVisible(Description = "Last error", Modifiable = false, Priority = 257)]
    public string ErrorString
    {
      get
      {
        return _Error != null ? _Error.Message : "";
      }
    }

    [XmlIgnore]
    private Exception _Error = null;

    /// <summary>
    /// Called when this subscription has just been checked
    /// </summary>
    public void Checked()
    {
      _Entries.LastCheck = DateTime.UtcNow;
    }

    /// <summary>
    /// Proxy for LastCheck changes
    /// </summary>
    private void EntryDataChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "LastCheck") {
        OnPropertyChanged(e.PropertyName);
      }
    }

    /// <summary>
    /// Data for entries in this subscription
    /// </summary>
    [XmlIgnore]
    public IReadOnlyDictionary<string, Entry> Entries
    {
      get
      {
        Load();
        return _Entries.Entries;
      }
    }
    
    /// <summary>
    /// Serial number
    /// </summary>
    /// <remarks>This allows an ordering to be maintained, even in the absence of timestamps.</remarks>
    [XmlIgnore]
    protected int NextSerial;

    /// <summary>
    /// Parent subscription list
    /// </summary>
    [XmlIgnore]
    public SubscriptionList Parent;

    #endregion

    #region Modifiable Properties

    static private bool HasUserVisibleAttribute(PropertyInfo pi) {
      return pi.GetCustomAttribute<UserVisibleAttribute>() != null;
    }

    static private string Order(PropertyInfo pi)
    {
      UserVisibleAttribute uva = pi.GetCustomAttribute<UserVisibleAttribute>();
      return string.Format("{0}-{1}-{2}",
                           uva.Modifiable ? 0 : 1,
                           uva.Priority,
                           uva.Description != null ? uva.Description : pi.Name);
    }

    public PropertyInfo[] GetUserVisibleProperties()
    {
      return (from pi in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
              where HasUserVisibleAttribute(pi)
              orderby Order(pi)
              select pi).ToArray();
    }

    #endregion

    #region Protected fields and properties

    /// <summary>
    /// Entry data
    /// </summary>
    /// <remarks>Caution!  Call <c>Load()</c> before use.</remarks>
    protected EntryList _Entries;

    #endregion

    #region Adding Entries

    public delegate void EntryAddedEventHandler(Entry entry);
    public event EntryAddedEventHandler EntryAdded;

    /// <summary>
    /// Add a subscription
    /// </summary>
    /// <param name="subscription"></param>
    public void Add(Entry entry)
    {
      _Entries.Entries[entry.Identity] = entry;
      _Entries.Dirty = true;
      EntryAddedEventHandler handler = EntryAdded;
      if (handler != null) {
        handler(entry);
      }
    }

    #endregion

    #region Initialization and serialization

    /// <summary>
    /// Ensure that <c>_EntryData</c> and <c>_EntryState</c> are set
    /// </summary>
    /// <remarks>
    /// This is the only place _Entries should be set.
    /// Applications shouldn't need to call this.</remarks>
    public void Load()
    {
      Load(null);
    }

    /// <summary>
    /// Load data from specific paths
    /// </summary>
    /// <remarks>Just for testing.</remarks>
    public void Load(string path)
    {
      if (_Entries == null) {
        _Entries = EntryList.Load(this, path);
        _Entries.PropertyChanged += EntryDataChanged;
        // Compute the next serial number
        if (_Entries.Entries.Count > 0) {
          NextSerial = _Entries.Entries.Values.Max(entry => entry.Serial) + 1;
        }
        else {
          NextSerial = 0;
        }
        OnPropertyChanged("Entries");
        OnPropertyChanged("UnreadEntries");
      }
    }

    /// <summary>
    /// Save persistent state and subscription data
    /// </summary>
    public void Save(bool force = false)
    {
      if (_Entries != null) {
        _Entries.Save(force);
      }
    }

    /// <summary>
    /// Save persistent state and subscription data to specific files
    /// </summary>
    /// <remarks>Just for testing.</remarks>
    public void Save(string path)
    {
      _Entries.Save(path);
    }

    /// <summary>
    /// Junk state and reload it
    /// </summary>
    /// <remarks>Used in initial subscription.</remarks>
    public void Reset()
    {
      _Entries = null;
      Load();
    }

    #endregion

    #region Files

    /// <summary>
    /// The directory in which the data and state files lives
    /// </summary>
    /// <returns></returns>
    public string Directory()
    {
      if (Parent != null) {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "RssReader",
                            Parent.Identity.ToString());
      }
      else {
        return null;
      }
    }

    #endregion

    #region Updating

    /// <summary>
    /// Set when an update is in progress
    /// </summary>
    /// <remarks>This field belongs to the update code in the background thread.
    /// Keep clear.  A lock must be held on the subscription when accessing it.</remarks>
    [XmlIgnore]
    public bool Updating = false;

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
