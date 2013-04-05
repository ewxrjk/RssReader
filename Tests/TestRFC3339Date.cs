using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReaderLib;

namespace Tests
{
  [TestClass]
  public class TestRFC3339Date
  {
    [TestMethod]
    public void Test3339UtcDates()
    {
      Assert.AreEqual(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                      Tools.RFC3339Date("1990-01-01T00:00:00Z"));
      Assert.AreEqual(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                      Tools.RFC3339Date("1990-01-01T00:00:00+00:00"));
      Assert.AreEqual(new DateTime(2011, 6, 3, 1, 2, 4, DateTimeKind.Utc),
                      Tools.RFC3339Date("2011-06-03T01:02:04Z"));
      Assert.AreEqual(new DateTime(2013, 4, 5, 17, 28, 0, DateTimeKind.Utc),
                      Tools.RFC3339Date("2013-04-05T17:28:00+00:00"));
    }

    [TestMethod]
    public void Test3339BstDates()
    {
      Assert.AreEqual(new DateTime(1977, 8, 1, 23, 0, 0, DateTimeKind.Utc),
                      Tools.RFC3339Date("1977-08-02T00:00:00+01:00"));
      Assert.AreEqual(new DateTime(1990, 7, 9, 12, 0, 0, DateTimeKind.Utc),
                      Tools.RFC3339Date("1990-07-09T13:00:00+01:00"));
      Assert.AreEqual(new DateTime(2011, 6, 3, 0, 12, 4, DateTimeKind.Utc),
                      Tools.RFC3339Date("2011-06-03T01:12:04+01:00"));
    }

    [TestMethod]
    public void Test3339Rfc3339Vectors()
    {
      Assert.AreEqual(new DateTime(1985, 4, 12, 23, 20, 50, 520, DateTimeKind.Utc),
                      Tools.RFC3339Date("1985-04-12T23:20:50.52Z"));
      Assert.AreEqual(new DateTime(1996, 12, 20, 0, 39, 57, DateTimeKind.Utc),
                      Tools.RFC3339Date("1996-12-19T16:39:57-08:00"));
      Assert.AreEqual(new DateTime(1996, 12, 20, 0, 39, 57, DateTimeKind.Utc),
                      Tools.RFC3339Date("1996-12-20T00:39:57Z"));
      Assert.AreEqual(new DateTime(1990, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                      Tools.RFC3339Date("1990-12-31T23:59:60Z"));
      Assert.AreEqual(new DateTime(1990, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                      Tools.RFC3339Date("1990-12-31T15:59:60-08:00"));
      Assert.AreEqual(new DateTime(1937, 1, 1, 11, 40, 27, 870, DateTimeKind.Utc),
                      Tools.RFC3339Date("1937-01-01T12:00:27.87+00:20"));
    }

    [TestMethod]
    public void Test3339Rfc4287Vectors()
    {
      Assert.AreEqual(new DateTime(2003, 12, 13, 18, 30, 2, DateTimeKind.Utc),
                Tools.RFC3339Date("2003-12-13T18:30:02Z"));
      Assert.AreEqual(new DateTime(2003, 12, 13, 18, 30, 2, 250, DateTimeKind.Utc),
                Tools.RFC3339Date("2003-12-13T18:30:02.25Z"));
      Assert.AreEqual(new DateTime(2003, 12, 13, 17, 30, 2, DateTimeKind.Utc),
                Tools.RFC3339Date("2003-12-13T18:30:02+01:00"));
      Assert.AreEqual(new DateTime(2003, 12, 13, 17, 30, 2, 250, DateTimeKind.Utc),
                Tools.RFC3339Date("2003-12-13T18:30:02.25+01:00"));

    }
  }
}
