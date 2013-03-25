using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReaderLib;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace parsegsub
{
  class Program
  {
    static void Main(string[] args)
    {
      using (FileStream fs = new FileStream(args[0], FileMode.Open)) {
        SubscriptionList subs = new SubscriptionList();
        subs.Subscriptions.Extend(new GoogleSubscriptions(fs).GetSubscriptions());
        foreach (Subscription sub in subs.Subscriptions) {
          Console.WriteLine("Subscription type: {0}", sub.Type());
          foreach (PropertyInfo pi in sub.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)) {
            object pv = pi.GetValue(sub);
            if (pv != null) {
              Console.WriteLine("  {0}: {1}", pi.Name, pv);
            }
          }
          Console.WriteLine("");
        }
        if (args.Length > 1) {
          XmlSerializer xs = new XmlSerializer(subs.GetType());
          using (StreamWriter sw = new StreamWriter(args[1])) {
            xs.Serialize(sw, subs);
            sw.Flush();
          }
        }
      }
    }
  }
}
