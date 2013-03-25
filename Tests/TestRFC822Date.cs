using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReaderLib;

namespace Tests
{
  [TestClass]
  public class TestRFC822Date
  {
    [TestMethod]
    public void TestUtcDates()
    {
      Assert.AreEqual(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                      Tools.RFC822Date("1 Jan 90 00:00:00 GMT"));
      Assert.AreEqual(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                      Tools.RFC822Date("Mon, 1 Jan 1990 00:00:00 +0000"));
      Assert.AreEqual(new DateTime(2011, 6, 3, 1, 2, 4, DateTimeKind.Utc),
                      Tools.RFC822Date("3 Jun 2011 01:02:04 UTC"));
    }

    [TestMethod]
    public void TestBstDates()
    {
      Assert.AreEqual(new DateTime(1977, 8, 1, 23, 0, 0, DateTimeKind.Utc),
                Tools.RFC822Date("Tue, 02 Aug 1977 00:00:00 +0100"));
      Assert.AreEqual(new DateTime(1990, 7, 9, 12, 0, 0, DateTimeKind.Utc),
                      Tools.RFC822Date("09 Jul 1990 13:00:00 +0100"));
      Assert.AreEqual(new DateTime(2011, 6, 3, 0, 2, 4, DateTimeKind.Utc),
                      Tools.RFC822Date("3 Jun 2011 01:02:04 BST"));
    }
  }
}
