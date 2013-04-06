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
