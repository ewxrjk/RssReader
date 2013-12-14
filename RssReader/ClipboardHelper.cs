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
using System.Runtime.InteropServices;
using System.Threading;

namespace RssReader
{
  public static class Clipboard
  {
    public static void Copy(string s)
    {
      try {
        System.Windows.Clipboard.SetData(System.Windows.DataFormats.Text, s);
      }
      catch (COMException) {
        // If it didn't work try again a bit later
        ThreadPool.QueueUserWorkItem(unused =>
        {
          Thread.Sleep(10);
          try {
            System.Windows.Clipboard.SetData(System.Windows.DataFormats.Text, s);
          }
          catch (COMException) { }
        });
      }
    }
  }
}
