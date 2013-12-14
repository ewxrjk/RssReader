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
using System.IO;

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
        Assert.AreNotEqual(null, subs.Subscriptions[0] as WebSubscription);
        Assert.AreNotEqual(null, subs.Subscriptions[1] as WebSubscription);
        Assert.AreNotEqual(null, subs.Subscriptions[2] as WebSubscription);
        Assert.AreEqual("Embedded in Academia", (subs.Subscriptions[0] as WebSubscription).Title);
        Assert.AreEqual("http://blog.regehr.org/feed", (subs.Subscriptions[0] as WebSubscription).URI);
        Assert.AreEqual("http://blog.regehr.org", (subs.Subscriptions[0] as WebSubscription).PublicURI);
        Assert.AreEqual("john hawks weblog", (subs.Subscriptions[1] as WebSubscription).Title);
        Assert.AreEqual("http://johnhawks.net/rss.xml", (subs.Subscriptions[1] as WebSubscription).URI);
        Assert.AreEqual("http://johnhawks.net", (subs.Subscriptions[1] as WebSubscription).PublicURI);
        Assert.AreEqual("LWN.net", (subs.Subscriptions[2] as WebSubscription).Title);
        Assert.AreEqual("http://lwn.net/headlines/rss", (subs.Subscriptions[2] as WebSubscription).URI);
        Assert.AreEqual("http://lwn.net", (subs.Subscriptions[2] as WebSubscription).PublicURI);
      }
    }
  }
}
