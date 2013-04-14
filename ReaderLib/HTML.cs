using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReaderLib.HTML
{
  /// <summary>
  /// Base class for HTML document nodes
  /// </summary>
  public abstract class Node
  {
    /// <summary>
    /// Write string format
    /// </summary>
    /// <param name="writer"></param>
    public abstract void Write(TextWriter writer);

    public override string ToString()
    {
      StringWriter writer = new StringWriter();
      Write(writer);
      return writer.ToString();
    }
  }

  /// <summary>
  /// HTML element node
  /// </summary>
  public class Element : Node
  {
    public Element()
    {
      Name = null;
      Contents = new List<Node>();
      Attributes = new Dictionary<string, string>();
    }

    /// <summary>
    /// Element name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Contents of this element
    /// </summary>
    public List<Node> Contents { get; set; }

    /// <summary>
    /// Attributes of this element
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; }

    public bool IsBlockElement()
    {
      return (HTML.Parser.GetElementType(Name) & (Parser.ElementType.Block | Parser.ElementType.BlockSingle)) != 0;
    }

    public bool IsListElement()
    {
      return (HTML.Parser.GetElementType(Name) & (Parser.ElementType.ListContainer)) != 0;
    }

    public bool IsTableElement()
    {
      return (HTML.Parser.GetElementType(Name) & (Parser.ElementType.Table)) != 0;
    }

    public bool IsInlineElement()
    {
      return (HTML.Parser.GetElementType(Name) & (Parser.ElementType.Inline | Parser.ElementType.InlineSingle)) != 0;
    }

    public bool IsFlowContainer()
    {
      return (HTML.Parser.GetElementType(Name) & (Parser.ElementType.FlowContainer)) != 0;
    }

    /// <summary>
    /// Find the first child element with a given name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Element FindChild(string name)
    {
      foreach (Node node in Contents) {
        if (node is Element && ((Element)node).Name == name) {
          return (Element)node;
        }
      }
      return null;
    }

    /// <summary>
    /// Follow a path to a descendant element
    /// </summary>
    /// <param name="Names"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    /// <remarks><code>this</code> is indicate by an empty path.
    /// The path to the first child with a given name of an element
    /// is the path to the element extended by the child name.</remarks>
    public Element Follow(IList<string> path, int pos = 0)
    {
      if (pos >= path.Count()) {
        return this;
      }
      Element child = FindChild(path[pos]);
      return child != null ? child.Follow(path, pos + 1) : null;
    }

    /// <summary>
    /// Follow a path to a descendant element
    /// </summary>
    /// <param name="path">Node names in path separated by dots</param>
    /// <returns></returns>
    public Element Follow(string path)
    {
      return Follow(path.Split(new char[] { '.' }));
    }

    public override void Write(TextWriter writer)
    {
      writer.Write("<{0}", Name);
      foreach (KeyValuePair<string, string> kvp in from kvp in Attributes orderby kvp.Key select kvp) {
        writer.Write(" {0}=\"", kvp.Key);
        foreach (char c in kvp.Value) {
          switch (c) {
            case '<':
            case '&':
            case '"':
              writer.Write("&#{0};", (int)c);
              break;
            default:
              writer.Write(c);
              break;
          }
        }
        writer.Write("\"");
      }
      writer.Write(">");
      switch (Parser.GetElementType(Name)) {
        case Parser.ElementType.InlineSingle:
        case Parser.ElementType.BlockSingle:
        case Parser.ElementType.HeadSingle:
          break;
        case Parser.ElementType.HeadSpecial:
          foreach (Node node in Contents) {
            if (node is Cdata) {
              writer.Write(((Cdata)node).Content);
            }
            else {
              node.Write(writer);
            }
          }
          writer.Write("</{0}>", Name);
          break;
        default:
          foreach (Node node in Contents) {
            node.Write(writer);
          }
          writer.Write("</{0}>", Name);
          break;
      }
    }
  }

  /// <summary>
  /// HTML character data node
  /// </summary>
  public class Cdata : Node
  {
    /// <summary>
    /// Character data
    /// </summary>
    public string Content;

    public override void Write(TextWriter writer)
    {
      foreach (char c in Content) {
        switch (c) {
          case '<':
          case '&':
            writer.Write("&#{0};", (int)c);
            break;
          default:

            writer.Write(c);
            break;
        }
      }
    }
  }

  /// <summary>
  /// HTML document container
  /// </summary>
  public class Document
  {
    public Document()
    {
      HTML = null;
    }

    /// <summary>
    /// HTML element for this document
    /// </summary>
    public Element HTML { get; set; }

    public void Write(TextWriter writer)
    {
      HTML.Write(writer);
    }

    public override string ToString()
    {
      return HTML.ToString();
    }

    /// <summary>
    /// Read an HTML document
    /// </summary>
    /// <param name="tr"></param>
    /// <returns></returns>
    static public Document Parse(TextReader tr)
    {
      return (new Parser() { Input = tr }).Parse();
    }

    /// <summary>
    /// Convert a string to an HTML document
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    static public Document Parse(string s)
    {
      return Parse(new StringReader(s));
    }
  }
}
