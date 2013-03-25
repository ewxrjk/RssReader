using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// Exception representing an error fetching a subscription
  /// </summary>
  public class SubscriptionFetchingException : SubscriptionException
  {
    public SubscriptionFetchingException() { }
    public SubscriptionFetchingException(string message)
      : base(message) { }
    public SubscriptionFetchingException(string message, System.Exception inner)
      : base(message, inner) { }
    protected SubscriptionFetchingException(SerializationInfo info, StreamingContext context) { }

  }
}