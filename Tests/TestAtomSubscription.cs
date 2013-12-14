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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReaderLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Tests
{
  [TestClass]
  [DeploymentItem(@"..\..\..\Tests\atomsample.xml")]
  public class TestAtomSubscription
  {
    [TestMethod]
    public void TestAtomFeedDetails()
    {
      SubscriptionList sl = new SubscriptionList();
      WebSubscription sub = new WebSubscription() { Parent = sl };
      sub.Load();
      Dictionary<string, int> propertyChanges = DetectSubscriptionChanges(sub);
      using (FileStream fs = new FileStream("atomsample.xml", FileMode.Open)) {
        sub.UpdateFromStreamReader(new StreamReader(fs), SynchronousDispatch, RejectError);
      }
      Assert.AreEqual(1, propertyChanges["Title"]);
      Assert.AreEqual(1, propertyChanges["PublicURI"]);
      Assert.AreEqual("Example Feed", sub.Title);
      Assert.AreEqual("http://example.org/", sub.PublicURI);
    }

    [TestMethod]
    public void TestAtomFeedItems()
    {
      SubscriptionList sl = new SubscriptionList();
      WebSubscription sub = new WebSubscription() { Parent = sl };
      sub.Load();
      Dictionary<string, int> propertyChanges = DetectSubscriptionChanges(sub);
      using (FileStream fs = new FileStream("atomsample.xml", FileMode.Open)) {
        sub.UpdateFromStreamReader(new StreamReader(fs), SynchronousDispatch, RejectError);
      }
      ValidateAtom(sub);
    }

    [TestMethod]
    public void TestAtomFeedSerialization()
    {
      SubscriptionList sl = new SubscriptionList();
      WebSubscription sub = new WebSubscription() { Parent = sl };
      sub.Load();
      using (FileStream fs = new FileStream("atomsample.xml", FileMode.Open)) {
        sub.UpdateFromStreamReader(new StreamReader(fs), SynchronousDispatch, RejectError);
      }
      foreach (KeyValuePair<string, Entry> kvp in sub.Entries) {
        if (kvp.Value.Serial % 2 == 1) {
          kvp.Value.Read = true;
        }
      }
      sub.Save("data.xml");

      WebSubscription sub2 = new WebSubscription() { Parent = sl };
      Dictionary<string, int> propertyChanges = DetectSubscriptionChanges(sub2);
      sub2.Load("data.xml");
      Assert.AreEqual(1, propertyChanges["Entries"]);
      ValidateAtom(sub2);
      foreach (KeyValuePair<string, Entry> kvp in sub2.Entries) {
        if (kvp.Value.Serial % 2 == 1) {
          Assert.AreEqual(true, kvp.Value.Read);
        }
        else {
          Assert.AreEqual(false, kvp.Value.Read);
        }
      }
    }

    public void ValidateAtom(WebSubscription sub)
    {
      // TODO sample atom data is a bit thin!
      int[] found = new int[2] { 0, 0 };
      Assert.AreEqual(2, sub.Entries.Count);
      foreach (KeyValuePair<string, Entry> kvp in sub.Entries) {
        WebEntry entry = sub.Entries[kvp.Key] as WebEntry;
        Assert.AreNotEqual(null, entry);
        Assert.AreEqual(true, entry.Serial >= 0);
        Assert.AreEqual(true, entry.Serial < 2);
        ++found[entry.Serial];
        switch (entry.Serial) {
          case 1:
            Assert.AreEqual("http://example.org/foobar.html", entry.URI);
            Assert.AreEqual("Title", entry.Title);
            Assert.AreEqual("contents", entry.Description);
            Assert.AreEqual(new DateTime(2013, 01, 28, 04, 04, 02, DateTimeKind.Utc), entry.Date);
            break;
          case 0:
            Assert.AreEqual("http://example.org/2003/12/13/atom03", entry.URI);
            Assert.AreEqual("Atom-Powered Robots Run Amok", entry.Title);
            Assert.AreEqual("Some text.", entry.Description);
            Assert.AreEqual(new DateTime(2003, 12, 13, 18, 30, 2, DateTimeKind.Utc), entry.Date);
            break;
        }
      }
      Assert.AreEqual(1, found[0]);
      Assert.AreEqual(1, found[1]);
    }

    public Dictionary<string, int> DetectSubscriptionChanges(Subscription sub)
    {
      Dictionary<string, int> propertyChanges = new Dictionary<string, int>();
      sub.PropertyChanged += (object e, PropertyChangedEventArgs args) =>
      {
        if (!propertyChanges.ContainsKey(args.PropertyName)) {
          propertyChanges[args.PropertyName] = 0;
        }
        ++propertyChanges[args.PropertyName];
      };
      return propertyChanges;
    }

    static public void SynchronousDispatch(Action a)
    {
      a();
    }

    static public void RejectError(Exception e)
    {
      Assert.AreEqual(null, e);
      Assert.AreNotEqual(null, e);
    }
  }
}
