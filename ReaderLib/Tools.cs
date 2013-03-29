using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net;

namespace ReaderLib
{
  static public class Tools
  {

    /// <summary>
    /// RFC822 month names
    /// </summary>
    static private Dictionary<string, int> Months = new Dictionary<string, int>()
    {
      { "jan", 1 },
      { "feb", 2 },
      { "mar", 3 },
      { "apr", 4 },
      { "may", 5 },
      { "jun", 6 },
      { "jul", 7 },
      { "aug", 8 },
      { "sep", 9 },
      { "oct", 10 },
      { "nov", 11 },
      { "dec", 12 },
    };
    
    /// <summary>
    /// Timezone names
    /// </summary>
    /// <remarks>Based on a list in Wikipedia.  These values aren't standardized and there
    /// are duplicates, which have been removed in a pretty arbitrary way.</remarks>
    static private Dictionary<string, int> Timezones = new Dictionary<string, int>()
    {
      { "ACDT", 60*(60*10+30) },
      { "ACST", 60*(60*9+30) },
      { "ACT", 3600*8 },
      { "ADT", -3600*3 },
      { "AEDT", 3600*11 },
      { "AEST", 3600*10 },
      { "AFT", 60*(60*4+30) },
      { "AKDT", -3600*8 },
      { "AKST", -3600*9 },
      { "AMST", 3600*5 },
      { "AMT", 3600*4 },
      { "ART", -3600*3 },
      { "AST", 3600*3 },
      { "AWDT", 3600*9 },
      { "AWST", 3600*8 },
      { "AZOST", -3600*1 },
      { "AZT", 3600*4 },
      { "BDT", 3600*8 },
      { "BIOT", 3600*6 },
      { "BIT", -3600*12 },
      { "BOT", -3600*4 },
      { "BRT", -3600*3 },
      { "BST", 3600*1 },
      { "BTT", 3600*6 },
      { "CAT", 3600*2 },
      { "CCT", 60*(60*6+30) },
      { "CDT", -3600*5 },
      { "CEDT", 3600*2 },
      { "CEST", 3600*2 },
      { "CET", 3600*1 },
      { "CHADT", 60*(60*13+45) },
      { "CHAST", 60*(60*12+45) },
      { "CHOT", -3600*8 },
      { "CHST", 3600*10 },
      { "CHUT", 3600*10 },
      { "CIST", -3600*8 },
      { "CIT", 3600*8 },
      { "CKT", -3600*10 },
      { "CLST", -3600*3 },
      { "CLT", -3600*4 },
      { "COST", -3600*4 },
      { "COT", -3600*5 },
      { "CST", -3600*6 },
      { "CT", 3600*8 },
      { "CVT", -3600*1 },
      { "CWST", 60*(60*8+45) },
      { "CXT", 3600*7 },
      { "DAVT", 3600*7 },
      { "DDUT", 3600*10 },
      { "DFT", 3600*1 },
      { "EASST", -3600*5 },
      { "EAST", -3600*6 },
      { "EAT", 3600*3 },
      { "ECT", -3600*5 },
      { "EDT", -3600*4 },
      { "EEDT", 3600*3 },
      { "EEST", 3600*3 },
      { "EET", 3600*2 },
      { "EGST", 3600*0 },
      { "EGT", -3600*1 },
      { "EIT", 3600*9 },
      { "EST", -3600*5 },
      { "FET", 3600*3 },
      { "FJT", 3600*12 },
      { "FKST", -3600*3 },
      { "FKT", -3600*4 },
      { "FNT", -3600*2 },
      { "GALT", -3600*6 },
      { "GAMT", -3600*9 },
      { "GET", 3600*4 },
      { "GFT", -3600*3 },
      { "GILT", 3600*12 },
      { "GIT", -3600*9 },
      { "GMT", 0 },
      { "GST", 3600*4 },
      { "GYT", -3600*4 },
      { "HADT", -3600*9 },
      { "HAEC", 3600*2 },
      { "HAST", -3600*10 },
      { "HKT", 3600*8 },
      { "HMT", 3600*5 },
      { "HOVT", 3600*7 },
      { "HST", -3600*10 },
      { "ICT", 3600*7 },
      { "IDT", 3600*3 },
      { "IOT", 3600*3 },
      { "IRDT", 3600*8 },
      { "IRKT", 3600*9 },
      { "IRST", 60*(60*3+30) },
      { "IST", 3600*1 },
      { "JST", 3600*9 },
      { "KGT", 3600*6 },
      { "KOST", 3600*11 },
      { "KRAT", 3600*7 },
      { "KST", 3600*9 },
      { "LHST", 60*(60*10+30) },
      { "LINT", 3600*14 },
      { "MAGT", 3600*12 },
      { "MART", -60*(60*9+30) },
      { "MAWT", 3600*5 },
      { "MDT", -3600*6 },
      { "MET", 3600*1 },
      { "MEST", 3600*2 },
      { "MHT", 3600*12 },
      { "MIST", 3600*11 },
      { "MIT", -60*(60*9+30) },
      { "MMT", 60*(60*6+30) },
      { "MSK", 3600*4 },
      { "MST", -3600*7 },
      { "MUT", 3600*4 },
      { "MVT", 3600*5 },
      { "MYT", 3600*8 },
      { "NCT", 3600*11 },
      { "NDT", -60*(60*2+30) },
      { "NFT", 60*(60*11+30) },
      { "NPT", 60*(60*5+45) },
      { "NST", -60*(60*3+30) },
      { "NT", -60*(60*3+30) },
      { "NUT", -60*(60*11+30) },
      { "NZDT", 3600*13 },
      { "NZST", 3600*12 },
      { "OMST", 3600*6 },
      { "ORAT", 3600*5 },
      { "PDT", -3600*7 },
      { "PET", -3600*5 },
      { "PETT", 3600*12 },
      { "PGT", 3600*10 },
      { "PHOT", 3600*13 },
      { "PHT", 3600*8 },
      { "PKT", 3600*5 },
      { "PMDT", -3600*2 },
      { "PMST", -3600*3 },
      { "PONT", 3600*11 },
      { "PST", -3600*8 },
      { "RET", 3600*4 },
      { "ROTT", -3600*3 },
      { "SAKT", 3600*11 },
      { "SAMT", 3600*4 },
      { "SAST", 3600*2 },
      { "SBT", 3600*11 },
      { "SCT", 3600*4 },
      { "SGT", 3600*8 },
      { "SLT", 60*(60*5+30) },
      { "SRT", -3600*3 },
      { "SST", 3600*8 },
      { "SYOT", 3600*3 },
      { "TAHT", -3600*10 },
      { "THA", 3600*7 },
      { "TFT", 3600*5 },
      { "TJT", 3600*5 },
      { "TKT", 3600*14 },
      { "TLT", 3600*9 },
      { "TMT", 3600*5 },
      { "TOT", 3600*13 },
      { "TVT", 3600*12 },
      { "UCT", 0 },
      { "ULAT", 3600*8 },
      { "UTC", 0 },
      { "UYST", -3600*2 },
      { "UYT", -3600*3 },
      { "UZT", 3600*5 },
      { "VET", -60*(60*4+30) },
      { "VLAT", 3600*10 },
      { "VOLT", 3600*4 },
      { "VOST", 3600*6 },
      { "VUT", 3600*11 },
      { "WAKT", 3600*12 },
      { "WAST", 3600*2 },
      { "WAT", 3600*1 },
      { "WEDT", 3600*1 },
      { "WEST", 3600*1 },
      { "WET", 0 },
      { "WST", 3600*8 },
      { "YAKT", 3600*9 },
      { "YEKT", 3600*5 },
    };


    // [day,] DD MONTH [YY]YY HH:MM:SS ZONE
    static Regex RFC822DateRegex = new Regex(@"^\s*(?:\w+,\s*)?(\d+)\s+(\S+)\s+(\d+)\s+(\d+):(\d+):(\d+)\s+(\S+)$",
      RegexOptions.IgnoreCase|RegexOptions.Compiled);

    /// <summary>
    /// Convert an RFC822 date string to a DateTime
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    static public DateTime RFC822Date(string s)
    {
      Match m = RFC822DateRegex.Match(s);
      if (m.Success) {
        int day = int.Parse(m.Groups[1].Value);
        int month = Months[m.Groups[2].Value.ToLowerInvariant()];
        int year = int.Parse(m.Groups[3].Value);
        if (year < 100) {
          year += 1900;
        }
        int hour = int.Parse(m.Groups[4].Value);
        int minute = int.Parse(m.Groups[5].Value);
        int second = int.Parse(m.Groups[6].Value);
        string zone = m.Groups[7].Value.ToUpperInvariant();
        int offset = 0;
        if (zone.Length == 5 && (zone[0] == '+' || zone[0] == '-')) {
          // Numeric offsets are best
          int zoneHours = int.Parse(zone.Substring(1, 2));
          int zoneMinutes = int.Parse(zone.Substring(3, 2));
          offset = 60 * (60 * zoneHours + zoneMinutes);
          if (zone[0] == '-') {
            offset = -offset;
          }
        }
        else {
          if (Timezones.ContainsKey(zone)) {
            offset = Timezones[zone];
          }
        }
        DateTime when = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        return when.AddSeconds(-offset);
      }
      else {
        throw new Exception(string.Format("invalid RFC822 date {0}", s));
      }
    }

    static Regex RFC3339DateRegex = new Regex(
      @"(\d+)-(\d+)-(\d+)T(\d+):(\d+):(\d+)(\.\d+)?(Z|(\+|-)(\d+):(\d+))",
      RegexOptions.IgnoreCase | RegexOptions.Compiled);

    static public DateTime RFC3339Date(string s)
    {
      Match m = RFC3339DateRegex.Match(s);
      if (m.Success) {
        int year = int.Parse(m.Groups[1].Value);
        int month = int.Parse(m.Groups[2].Value);
        int day = int.Parse(m.Groups[3].Value);
        int hour = int.Parse(m.Groups[4].Value);
        int minute = int.Parse(m.Groups[5].Value);
        int second = int.Parse(m.Groups[6].Value);
        if (second > 59) { // DateTime won't accept leap seconds, so we bodge them...
          second = 59;
        }
        int ms = m.Groups[7].Success ? (int)Math.Floor(double.Parse(m.Groups[7].Value) * 1000) : 0;
        int offset;
        if (m.Groups[8].Value == "Z") {
          offset = 0;
        }
        else {
          int zoneHours = int.Parse(m.Groups[10].Value);
          int zoneMinutes = int.Parse(m.Groups[11].Value);
          offset = 60 * (60 * zoneHours + zoneMinutes);
          if (m.Groups[9].Value == "-") {
            offset = -offset;
          }
        }
        DateTime when = new DateTime(year, month, day, hour, minute, second, ms, DateTimeKind.Utc);
        return when.AddSeconds(-offset);
      }
      else {
        throw new Exception(string.Format("invalid RFC3339 date {0}", s));
      }
    }

    /// <summary>
    /// Extend a collection
    /// </summary>
    /// <typeparam name="T">Member type</typeparam>
    /// <param name="collection">Collection to extend</param>
    /// <param name="source">Source of additional items</param>
    /// <returns><paramref name="collection"/></returns>
    static public ICollection<T> Extend<T>(this ICollection<T> collection, IEnumerable<T> source) {
      foreach (T element in source) {
        collection.Add(element);
      }
      return collection;
    }

    /// <summary>
    /// Look up a value in a dictionary, returning a default if not found
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    static public TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
                                                         TKey key,
                                                         TValue defaultValue)
    {
      TValue value;
      return dictionary.TryGetValue(key, out value) ? value : defaultValue;
    }

    /// <summary>
    /// Find the character encoding used in a WebResponse
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static Encoding GetEncoding(WebResponse response)
    {
      HttpWebResponse httpResponse = response as HttpWebResponse;
      string encodingName = "UTF-8"; // Assume UTF-8 for non-HTTP responses (TODO maybe can do better?)
      if (httpResponse != null) {
        encodingName = httpResponse.CharacterSet;
      }
      try {
        return Encoding.GetEncoding(encodingName);
      }
      catch (ArgumentException) {
        //Console.Error.WriteLine("Never heard of '{0}', assuming UTF-8", encodingName);  // TODO this should go in the error log
        // Assume UTF-8 if it goes wrong
        return Encoding.UTF8;
      }
    }

  }
}
