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

namespace Tests
{
  [TestClass]
  public class TestRFC822Date
  {
    [TestMethod]
    public void Test822UtcDates()
    {
      Assert.AreEqual(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                      Tools.RFC822Date("1 Jan 90 00:00:00 GMT"));
      Assert.AreEqual(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                      Tools.RFC822Date("Mon, 1 Jan 1990 00:00:00 +0000"));
      Assert.AreEqual(new DateTime(2011, 6, 3, 1, 2, 4, DateTimeKind.Utc),
                      Tools.RFC822Date("3 Jun 2011 01:02:04 UTC"));
    }

    [TestMethod]
    public void Test822BstDates()
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
