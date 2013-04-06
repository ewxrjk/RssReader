using System;
using System.Xml.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// Base class for uniquely identifiable things
  /// </summary>
  public class UniquelyIdentifiable
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <remarks>Initializes <c>Identity</c> to a fresh GUID.</remarks>
    public UniquelyIdentifiable()
    {
      Identity = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Unique identifier
    /// </summary>
    /// <remarks><para>By default this will be a fresh GUID, but in some contexts
    /// a unique ID can come from somewhere else.</para></remarks>
    [XmlAttribute("id")]
    public string Identity { get; set; }

  }
}
