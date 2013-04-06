using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReaderLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Tests
{
  [TestClass]
  [DeploymentItem(@"..\..\..\Tests\rss-rdf-sample.xml")]
  public class TestRdfSubscription
  {
    [TestMethod]
    public void TestRdfFeedDetails()
    {
      SubscriptionList sl = new SubscriptionList();
      WebSubscription sub = new WebSubscription() { Parent = sl };
      sub.Load();
      Dictionary<string, int> propertyChanges = DetectSubscriptionChanges(sub);
      using (FileStream fs = new FileStream("rss-rdf-sample.xml", FileMode.Open)) {
        sub.UpdateFromStreamReader(new StreamReader(fs), SynchronousDispatch, RejectError);
      }
      Assert.AreEqual(1, propertyChanges["Title"]);
      Assert.AreEqual(1, propertyChanges["PublicURI"]);
      Assert.AreEqual(1, propertyChanges["Description"]);
      Assert.AreEqual("LWN.net", sub.Title);
      Assert.AreEqual("http://lwn.net", sub.PublicURI);
      Assert.AreEqual("\n LWN.net is a ...\n    ", sub.Description);
    }

    [TestMethod]
    public void TestRdfFeedItems()
    {
      SubscriptionList sl = new SubscriptionList();
      WebSubscription sub = new WebSubscription() { Parent = sl };
      sub.Load();
      Dictionary<string, int> propertyChanges = DetectSubscriptionChanges(sub);
      using (FileStream fs = new FileStream("rss-rdf-sample.xml", FileMode.Open)) {
        sub.UpdateFromStreamReader(new StreamReader(fs), SynchronousDispatch, RejectError);
      }
      ValidateRdf(sub);
    }

    public void ValidateRdf(WebSubscription sub)
    {
      int[] found = new int[3] {0, 0, 0 };
      foreach (KeyValuePair<string, Entry> kvp in sub.Entries) {
        WebEntry data = sub.Entries[kvp.Key] as WebEntry;
        Assert.AreNotEqual(null, data);
        Assert.AreEqual(true, data.Serial >= 0);
        Assert.AreEqual(true, data.Serial < 3);
        ++found[data.Serial];
        switch (data.Serial) {
          case 2:
            Assert.AreEqual("http://lwn.net/Articles/546186/rss", data.URI);
            Assert.AreEqual("Ubuntu 13.04 (Raring Ringtail) Beta 2 released", data.Title);
            Assert.AreEqual("\n      The second and final Ubuntu 13.04 ...\n",
                            data.Description);
            Assert.AreEqual(new DateTime(2013, 4, 5, 17, 28, 0, DateTimeKind.Utc), data.Date);
            break;
          case 1:
            Assert.AreEqual("http://lwn.net/Articles/546120/rss", data.URI);
            Assert.AreEqual("Friday's security updates", data.Title);
            Assert.AreEqual("\n      <p>\n<b>Fedora</b> has updated ...\n ",
                            data.Description);
            Assert.AreEqual(new DateTime(2013, 4, 5, 14, 53, 13, DateTimeKind.Utc), data.Date);
            break;
          case 0:
            Assert.AreEqual("http://lwn.net/Articles/545918/rss", data.URI);
            Assert.AreEqual("Thursday's security updates", data.Title);
            Assert.AreEqual("\n      <p>\n<b>Debian</b> has updated ...\n",
                            data.Description);
            Assert.AreEqual(new DateTime(2013, 4, 4, 16, 10, 58, DateTimeKind.Utc), data.Date);
            break;
        }
      }
      Assert.AreEqual(1, found[0]);
      Assert.AreEqual(1, found[1]);
      Assert.AreEqual(1, found[2]);
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
