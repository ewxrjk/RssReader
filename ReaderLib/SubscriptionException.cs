using System;
using System.Runtime.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// Exception representing an error parsing a subscription
  /// </summary>
  public class SubscriptionException : ApplicationException
  {
    public SubscriptionException() { }
    public SubscriptionException(string message)
      : base(message) { }
    public SubscriptionException(string message, System.Exception inner)
      : base(message, inner) { }
    protected SubscriptionException(SerializationInfo info, StreamingContext context) { }

    /// <summary>
    /// Subscription associated with this error
    /// </summary>
    public Subscription Subscription = null;
  }
}
