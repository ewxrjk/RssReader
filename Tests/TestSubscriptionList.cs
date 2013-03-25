using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReaderLib;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;

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
        sl.Subscriptions.Add(new RssSubscription()
        {
          URI = "http://www.example.com/rss"
        });
        sl.Subscriptions.Add(new RssSubscription()
        {
          URI = "http://www.example.com/atom"
        });
        sl.Save("subscriptions.xml");
      }
      SubscriptionList sl2 = SubscriptionList.Load("subscriptions.xml");
      Assert.AreEqual(2, sl2.Subscriptions.Count);
      Assert.AreNotEqual(null, sl2.Subscriptions[0] as RssSubscription);
      Assert.AreEqual("http://www.example.com/rss", ((RssSubscription)sl2.Subscriptions[0]).URI);
      Assert.AreNotEqual(null, sl2.Subscriptions[1] as RssSubscription);
      Assert.AreEqual("http://www.example.com/atom", ((RssSubscription)sl2.Subscriptions[1]).URI);
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
      sl.Add(new RssSubscription()
      {
        URI = "http://www.example.com/rss"
      });
      Assert.AreEqual(1, changes);
    }

  }
}
