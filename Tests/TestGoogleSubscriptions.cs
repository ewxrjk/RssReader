using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ReaderLib;

namespace Tests
{
  [TestClass]
  [DeploymentItem(@"..\..\..\Tests\GoogleSubscriptions.xml")]
  public class TestGoogleSubscriptions
  {
    [TestMethod]
    public void TestGoogle()
    {
      using (FileStream fs = new FileStream("GoogleSubscriptions.xml", FileMode.Open)) {
        SubscriptionList subs = new SubscriptionList();
        subs.Subscriptions.Extend(new GoogleSubscriptions(fs).GetSubscriptions());
        Assert.AreEqual(3, subs.Subscriptions.Count);
        Assert.AreNotEqual(null, subs.Subscriptions[0] as RssSubscription);
        Assert.AreNotEqual(null, subs.Subscriptions[1] as RssSubscription);
        Assert.AreNotEqual(null, subs.Subscriptions[2] as RssSubscription);
        Assert.AreEqual("Embedded in Academia", (subs.Subscriptions[0] as RssSubscription).Title);
        Assert.AreEqual("http://blog.regehr.org/feed", (subs.Subscriptions[0] as RssSubscription).URI);
        Assert.AreEqual("http://blog.regehr.org", (subs.Subscriptions[0] as RssSubscription).PublicURI);
        Assert.AreEqual("john hawks weblog", (subs.Subscriptions[1] as RssSubscription).Title);
        Assert.AreEqual("http://johnhawks.net/rss.xml", (subs.Subscriptions[1] as RssSubscription).URI);
        Assert.AreEqual("http://johnhawks.net", (subs.Subscriptions[1] as RssSubscription).PublicURI);
        Assert.AreEqual("LWN.net", (subs.Subscriptions[2] as RssSubscription).Title);
        Assert.AreEqual("http://lwn.net/headlines/rss", (subs.Subscriptions[2] as RssSubscription).URI);
        Assert.AreEqual("http://lwn.net", (subs.Subscriptions[2] as RssSubscription).PublicURI);
      }
    }
  }
}
