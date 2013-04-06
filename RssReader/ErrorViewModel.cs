using ReaderLib;
using System;

namespace RssReader
{
  public class ErrorViewModel
  {
    #region Error Information

    /// <summary>
    /// The error
    /// </summary>
    public Exception Error;

    /// <summary>
    /// When the error occurred
    /// </summary>
    public DateTime TimeStamp { get; set; }

    #endregion

    #region View model properties

    /// <summary>
    /// The abbreviated time string
    /// </summary>
    public string TimeString
    {
      get
      {
        DateTime now = DateTime.Now;
        if (now.Date == TimeStamp.Date) {
          return TimeStamp.ToString("HH:mm:ss");
        }
        else {
          return TimeStamp.ToString("yyyy-MM-dd");
        }
      }
    }

    /// <summary>
    /// The full time string
    /// </summary>
    public string FullTimeString
    {
      get
      {
        return TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
      }
    }

    /// <summary>
    /// The summary error message
    /// </summary>
    public string Message
    {
      get
      {
        return Error.Message;
      }
    }

    /// <summary>
    /// The context in which the error occurred
    /// </summary>
    public string Context
    {
      get
      {
        SubscriptionException se = Error as SubscriptionException;
        if (se != null && se.Subscription != null) {
          return se.Subscription.Title;
        }
        return "";
      }
    }

    /// <summary>
    /// The full description of the error
    /// </summary>
    public string Description
    {
      get
      {
        return Error.ToString();
      }
      set
      {
      }
    }

    #endregion
  }
}
