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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// A list of subscriptions
  /// </summary>
  public class SubscriptionList : UniquelyIdentifiable, INotifyPropertyChanged
  {
    public SubscriptionList()
    {
      OriginalPath = null;
      Dirty = false;
    }

    [XmlIgnore]
    public bool Dirty { get; set; }

    /// <summary>
    /// XML serializer for this class
    /// </summary>
    static private XmlSerializer Serializer = new XmlSerializer(typeof(SubscriptionList));

    #region Public Properties

    /// <summary>
    /// Ordered list of subscriptions
    /// </summary>
    /// <remarks>Don't attempt to modify the subscription list by modifying this object.
    /// Use the <c>Add</c> and <c>Remove</c> methods instead.</remarks>
    [XmlElement("Subscription")]
    public List<Subscription> Subscriptions
    {
      get
      {
        return _Subscriptions;
      }
    }

    private List<Subscription> _Subscriptions = new List<Subscription>();

    /// <summary>
    /// The path this subscription list was loaded from
    /// </summary>
    [XmlIgnore]
    public string OriginalPath { get; set; }

    #endregion

    #region Adding and removing subscriptions

    public delegate void SubscriptionAddedEventHandler(Subscription subscription);
    public event SubscriptionAddedEventHandler SubscriptionAdded;

    /// <summary>
    /// Add a subscription
    /// </summary>
    /// <param name="subscription"></param>
    public void Add(Subscription subscription)
    {
      if (!Contains(subscription)) {
        subscription.Parent = this;
        subscription.PropertyChanged += this.ChildPropertyChanged;
        _Subscriptions.Add(subscription);
        Dirty = true;
        SubscriptionAddedEventHandler handler = SubscriptionAdded;
        if (handler != null) {
          handler(subscription);
        }
      }
    }

    public delegate void SubscriptionRemovedEventHandler(int index);
    public event SubscriptionRemovedEventHandler SubscriptionRemoved;

    /// <summary>
    /// Remove a subscription
    /// </summary>
    /// <param name="subscription"></param>
    public void Remove(Subscription subscription)
    {
      int index = _Subscriptions.FindIndex(candidate => candidate == subscription);
      if (index >= 0) {
        Dirty = true;
        subscription.PropertyChanged -= this.ChildPropertyChanged;
        _Subscriptions.RemoveAt(index);
        SubscriptionRemovedEventHandler handler = SubscriptionRemoved;
        if (handler != null) {
          handler(index);
        }
      }
    }

    /// <summary>
    /// Test whether a subscription is already present
    /// </summary>
    /// <param name="subscription"></param>
    /// <returns></returns>
    public bool Contains(Subscription subscription)
    {
      return _Subscriptions.FindIndex(candidate => candidate == subscription) >= 0;
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Save user configuration to a named file
    /// </summary>
    /// <param name="Path">The file to save to</param>
    public void Save(string Path)
    {
      string backup = null;
      // Back up existing files (once)
      if (File.Exists(Path)) {
        backup = Path + ".bak";
        File.Delete(backup);
        File.Copy(Path, backup);
      }
      try {
        using (StreamWriter sw = new StreamWriter(Path)) {
          Serializer.Serialize(sw, this);
          sw.Flush();
        }
      }
      catch (Exception) {
        File.Delete(Path);
        if (backup != null) {
          File.Move(backup, Path);
        }
        throw;
      }
      OriginalPath = Path;
    }

    /// <summary>
    /// Save user configuration to the original file
    /// </summary>
    public void Save(bool force = false)
    {
      if (Dirty || force) {
        if (OriginalPath != null) {
          Save(OriginalPath);
          Dirty = false;
        }
        else {
          throw new InvalidOperationException();
        }
      }
    }

    /// <summary>
    /// Load from a named file
    /// </summary>
    /// <param name="Path">File to load from</param>
    /// <returns>A new SubscriptionList</returns>
    static public SubscriptionList Load(string Path)
    {
      SubscriptionList sl;
      // Read user configuration
      using (StreamReader sr = new StreamReader(Path)) {
        sl = (SubscriptionList)Serializer.Deserialize(sr);
      }
      foreach (Subscription sub in sl.Subscriptions) {
        sub.Parent = sl;
        sub.PropertyChanged += sl.ChildPropertyChanged;
      }
      // Remember where it came from
      sl.OriginalPath = Path;
      return sl;
    }

    private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      this.Dirty = true;
    }

    /// <summary>
    /// Load the default subscription list
    /// </summary>
    /// <returns></returns>
    /// <remarks>Creates it if it does not exist.</remarks>
    static public SubscriptionList LoadDefault()
    {
      string path = DefaultPath();
      if (File.Exists(path)) {
        return Load(path);
      }
      else {
        Directory.CreateDirectory(DefaultDirectory());
        SubscriptionList sl = new SubscriptionList()
        {
          OriginalPath = path
        };
        return sl;
      }
    }

    static private string DefaultDirectory()
    {
      return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                          "RssReader");
    }

    static private string DefaultPath()
    {
      return Path.Combine(DefaultDirectory(),
                          "subscriptions.xml");
    }

    #endregion

    #region Background Thread

    /// <summary>
    /// A single subscription list's registration with the background thread
    /// </summary>
    private class BackgroundThreadRegistration
    {
      public SubscriptionList Subscriptions;
      public Action<Action> Dispatch;
      public Action<Exception> Error;

      public void Work()
      {
        Dispatch(() =>
        {
          try {
            Subscriptions.DirtyCheck(Error);
            Subscriptions.UpdateSubscriptions(Dispatch, Error);
          }
          catch (Exception e) {
            Error(e);
          }
        });
      }
    }

    /// <summary>
    /// Registrations to the background thread
    /// </summary>
    private static Dictionary<string, BackgroundThreadRegistration> BackgroundThreadRegistrations = new Dictionary<string, BackgroundThreadRegistration>();

    /// <summary>
    /// Create the background thread
    /// </summary>
    static SubscriptionList()
    {
      Thread BackgroundThread = new Thread(BackgroundThreadImplementation)
      {
        IsBackground = true
      };
      BackgroundThread.Start();
    }

    /// <summary>
    /// Implementation of the background thread
    /// </summary>
    static private void BackgroundThreadImplementation()
    {
      for (; ; ) {
        // Safely get a list of registrations to work on
        BackgroundThreadRegistration[] registrations;
        lock (BackgroundThreadRegistrations) {
          registrations = BackgroundThreadRegistrations.Values.ToArray();
        }
        // Do ecah subscription list's work
        foreach (BackgroundThreadRegistration reg in registrations) {
          reg.Work();
        }
        Thread.Sleep(1000);
      }
    }

    /// <summary>
    /// Register a subscription list with the background thread
    /// </summary>
    /// <param name="dispatch"></param>
    /// <param name="error"></param>
    public void BackgroundRegister(Action<Action> dispatch,
                                   Action<Exception> error)
    {
      lock (BackgroundThreadRegistrations) {
        BackgroundThreadRegistrations[Identity] = new BackgroundThreadRegistration()
        {
          Subscriptions = this,
          Dispatch = dispatch,
          Error = error,
        };
      }
    }

    /// <summary>
    /// Deregister a subscription list from the background thread
    /// </summary>
    public void BackgroundDeregister()
    {
      lock (BackgroundThreadRegistrations) {
        BackgroundThreadRegistrations.Remove(Identity);
      }
    }

    #endregion

    #region Background Operations

    /// <summary>
    /// Save anything that's changed recently
    /// </summary>
    /// <param name="error"></param>
    private void DirtyCheck(Action<Exception> error)
    {
      try {
        // Attempt to flush configuration changes
        Save();
        // Attempt to flush state changes
        foreach (Subscription sub in Subscriptions) {
          sub.Save();
        }
      }
      catch (Exception e) {
        error(e);
      }
    }

    /// <summary>
    /// Update subscriptions that haven't been checked recently
    /// </summary>
    /// <param name="dispatch"></param>
    /// <param name="error"></param>
    private void UpdateSubscriptions(Action<Action> dispatch,
                                     Action<Exception> error)
    {
      DateTime now = DateTime.UtcNow;
      // Kick off updates for subscriptions that haven't had any love lately
      foreach (Subscription sub in from sub in Subscriptions
                                   where !sub.Updating
                                   where sub.NextUpdate() <= now
                                   select sub) {
        lock (sub) {
          sub.Updating = true;
        }
        ThreadPool.QueueUserWorkItem(unused =>
        {
          // Perform the update
          try {
            sub.Update(dispatch, error);
          }
          catch (Exception e) {
            SubscriptionParsingException spe = e as SubscriptionParsingException;
            if (spe != null) {
              spe.Subscription = sub;
            }
            dispatch(() =>
            {
              error(e);
            });
          }
          // Set the last check time after the check has completed
          dispatch(() =>
          {
            sub.Checked();
          });
          // We're done.  It doesn't matter if the next update
          // overlaps ever-so-slightly with the old.
          lock (sub) {
            sub.Updating = false;
          }
        });
      }
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
