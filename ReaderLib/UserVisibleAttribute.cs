using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReaderLib
{
  /// <summary>
  /// All user-visible properties must be marked with this attribute
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class UserVisibleAttribute : System.Attribute
  {
    public UserVisibleAttribute()
    {
      Modifiable = false;
      Type = "text/plain";
      Priority = 0;
    }

    /// <summary>
    /// Human-readable description of the property
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Whether the property is modifiable
    /// </summary>
    public bool Modifiable { get; set; }

    /// <summary>
    /// Type (MIME plus extensions)
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Priority
    /// </summary>
    public int Priority { get; set; }
  }
}
