using System.Runtime.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// Exception representing an error parsing a subscription
  /// </summary>
  public class SubscriptionParsingException : SubscriptionException
  {
    public SubscriptionParsingException() { }
    public SubscriptionParsingException(string message)
      : base(message) { }
    public SubscriptionParsingException(string message, System.Exception inner)
      : base(message, inner) { }
    protected SubscriptionParsingException(SerializationInfo info, StreamingContext context) { }

  }
}
