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

namespace Tests
{
  [TestClass]
  [DeploymentItem(@"..\..\..\Tests\rss2sample.xml")]
  public class TestSubscriptionList
  {
    [TestMethod]
    public void TestSubscriptionListSerialization()
    {
      {
        SubscriptionList sl = new SubscriptionList();
        sl.Subscriptions.Add(new WebSubscription()
        {
          URI = "http://www.example.com/rss"
        });
        sl.Subscriptions.Add(new WebSubscription()
        {
          URI = "http://www.example.com/atom"
        });
        sl.Save("subscriptions.xml");
      }
      SubscriptionList sl2 = SubscriptionList.Load("subscriptions.xml");
      Assert.AreEqual(2, sl2.Subscriptions.Count);
      Assert.AreNotEqual(null, sl2.Subscriptions[0] as WebSubscription);
      Assert.AreEqual("http://www.example.com/rss", ((WebSubscription)sl2.Subscriptions[0]).URI);
      Assert.AreNotEqual(null, sl2.Subscriptions[1] as WebSubscription);
      Assert.AreEqual("http://www.example.com/atom", ((WebSubscription)sl2.Subscriptions[1]).URI);
    }

    [TestMethod]
    public void TestSubscriptionListNotification()
    {
      SubscriptionList sl = new SubscriptionList();
      int changes = 0;
      sl.SubscriptionAdded += (Subscription s) =>
      {
        ++changes;
      };
      sl.Add(new WebSubscription()
      {
        URI = "http://www.example.com/rss"
      });
      Assert.AreEqual(1, changes);
    }

  }
}
