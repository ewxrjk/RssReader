using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ReaderLib;
using System.Collections.Generic;
using System.ComponentModel;

namespace Tests
{
  [TestClass]
  [DeploymentItem(@"..\..\..\Tests\rss2sample.xml")]
  public class TestRssSubscription
  {
    [TestMethod]
    public void TestRssFeedDetails()
    {
      SubscriptionList sl = new SubscriptionList();
      RssSubscription sub = new RssSubscription() { Parent = sl };
      sub.Load();
      Dictionary<string, int> propertyChanges = DetectSubscriptionChanges(sub);
      using (FileStream fs = new FileStream("rss2sample.xml", FileMode.Open)) {
        sub.UpdateFromStreamReader(new StreamReader(fs), SynchronousDispatch, RejectError);
      }
      Assert.AreEqual(1, propertyChanges["Title"]);
      Assert.AreEqual(1, propertyChanges["PublicURI"]);
      Assert.AreEqual(1, propertyChanges["Description"]);
      Assert.AreEqual("Liftoff News", sub.Title);
      Assert.AreEqual("http://liftoff.msfc.nasa.gov/", sub.PublicURI);
      Assert.AreEqual("Liftoff to Space Exploration.", sub.Description);
    }

    [TestMethod]
    public void TestRssFeedItems()
    {
      SubscriptionList sl = new SubscriptionList();
      RssSubscription sub = new RssSubscription() { Parent = sl };
      sub.Load();
      Dictionary<string, int> propertyChanges = DetectSubscriptionChanges(sub);
      using (FileStream fs = new FileStream("rss2sample.xml", FileMode.Open)) {
        sub.UpdateFromStreamReader(new StreamReader(fs), SynchronousDispatch, RejectError);
      }
      ValidateRss(sub);
    }

    [TestMethod]
    public void TestRssFeedSerialization()
    {
      SubscriptionList sl = new SubscriptionList();
      RssSubscription sub = new RssSubscription() { Parent = sl };
      sub.Load();
      using (FileStream fs = new FileStream("rss2sample.xml", FileMode.Open)) {
        sub.UpdateFromStreamReader(new StreamReader(fs), SynchronousDispatch, RejectError);
      }
      foreach (KeyValuePair<string, Entry> kvp in sub.Entries) {
        if (kvp.Value.Serial % 2 == 1) {
          kvp.Value.Read = true;
        }
      }
      sub.Save("data.xml");

      RssSubscription sub2 = new RssSubscription() { Parent = sl };
      Dictionary<string, int> propertyChanges = DetectSubscriptionChanges(sub2);
      sub2.Load("data.xml");
      Assert.AreEqual(1, propertyChanges["Entries"]);
      ValidateRss(sub2);
      foreach (KeyValuePair<string, Entry> kvp in sub2.Entries) {
        if (kvp.Value.Serial % 2 == 1) {
          Assert.AreEqual(true, kvp.Value.Read);
        }
        else {
          Assert.AreEqual(false, kvp.Value.Read);
        }
      }
    }

    public void ValidateRss(RssSubscription sub)
    {
      int[] found = new int[4] { 0, 0, 0, 0 };
      foreach (KeyValuePair<string, Entry> kvp in sub.Entries) {
        RssEntryData data = sub.Entries[kvp.Key] as RssEntryData;
        Assert.AreNotEqual(null, data);
        Assert.AreEqual(true, data.Serial >= 0);
        Assert.AreEqual(true, data.Serial < 4);
        ++found[data.Serial];
        switch (data.Serial) {
          case 3:
            Assert.AreEqual("http://liftoff.msfc.nasa.gov/news/2003/news-starcity.asp", data.URI);
            Assert.AreEqual("Star City", data.Title);
            Assert.AreEqual("How do Americans get ready to work with Russians aboard the International Space Station? They take a crash course in culture, language and protocol at Russia's <a href=\"http://howe.iki.rssi.ru/GCTC/gctc_e.htm\">Star City</a>.",
                            data.Description);
            Assert.AreEqual(new DateTime(2003, 6, 3, 9, 39, 21, DateTimeKind.Utc), data.Date);
            break;
          case 2:
            Assert.AreEqual("http://liftoff.msfc.nasa.gov/2003/05/30.html#item572", data.URI);
            Assert.AreEqual(null, data.Title);
            Assert.AreEqual("Sky watchers in Europe, Asia, and parts of Alaska and Canada will experience a <a href=\"http://science.nasa.gov/headlines/y2003/30may_solareclipse.htm\">partial eclipse of the Sun</a> on Saturday, May 31st.",
                            data.Description);
            Assert.AreEqual(new DateTime(2003, 5, 30, 11, 6, 42, DateTimeKind.Utc), data.Date);
            break;
          case 1:
            Assert.AreEqual("http://liftoff.msfc.nasa.gov/news/2003/news-VASIMR.asp", data.URI);
            Assert.AreEqual("The Engine That Does More", data.Title);
            Assert.AreEqual("Before man travels to Mars, NASA hopes to design new engines that will let us fly through the Solar System more quickly.  The proposed VASIMR engine would do that.",
                            data.Description);
            Assert.AreEqual(new DateTime(2003, 5, 27, 8, 37, 32, DateTimeKind.Utc), data.Date);
            break;
          case 0:
            Assert.AreEqual("http://liftoff.msfc.nasa.gov/news/2003/news-laundry.asp", data.URI);
            Assert.AreEqual("Astronauts' Dirty Laundry", data.Title);
            Assert.AreEqual("Compared to earlier spacecraft, the International Space Station has many luxuries, but laundry facilities are not one of them.  Instead, astronauts have other options.",
                            data.Description);
            Assert.AreEqual(new DateTime(2003, 5, 20, 8, 56, 2, DateTimeKind.Utc), data.Date);
            break;
        }
      }
      Assert.AreEqual(1, found[0]);
      Assert.AreEqual(1, found[1]);
      Assert.AreEqual(1, found[2]);
      Assert.AreEqual(1, found[3]);
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
