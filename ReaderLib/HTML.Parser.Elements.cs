using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReaderLib.HTML
{
  public partial class Parser
  {
    internal enum ElementType {
      Other,
      Block,
      BlockSingle,
      Inline,
      InlineSingle,
      ListContainer,
      ListElement,
      Table,
      TableRow,
      TableCell,
      Ignore,
    };

    internal static ElementType GetElementType(string name)
    {
      ElementType e;
      if (ElementTypes.TryGetValue(name, out e)) {
        return e;
      }
      return ElementType.Ignore;
    }

    internal static Dictionary<string, ElementType> ElementTypes = new Dictionary<string, ElementType>() {
      { "a", ElementType.Inline },
      { "abbr", ElementType.Inline },
      { "acronym", ElementType.Inline },
      { "address", ElementType.Block },
      { "applet", ElementType.Ignore },
      { "area", ElementType.Ignore },
      { "b", ElementType.Inline },
      { "base", ElementType.Ignore },
      { "basefont", ElementType.Ignore },
      { "bdo", ElementType.Ignore },
      { "big", ElementType.Inline },
      { "blockquote", ElementType.Block },
      { "body", ElementType.Ignore },
      { "br", ElementType.InlineSingle },
      { "button", ElementType.Inline },
      { "caption", ElementType.Ignore },
      { "center", ElementType.Ignore },
      { "cite", ElementType.Inline },
      { "code", ElementType.Inline },
      { "col", ElementType.Ignore },
      { "colgroup", ElementType.Ignore },
      { "dd", ElementType.Block },
      { "del", ElementType.Ignore },
      { "dfn", ElementType.Inline },
      { "dir", ElementType.Ignore },
      { "div", ElementType.Block },
      { "dl", ElementType.Block },
      { "dt", ElementType.Ignore },
      { "em", ElementType.Inline },
      { "fieldset", ElementType.Block },
      { "font", ElementType.Ignore },
      { "form", ElementType.Block },
      { "frame", ElementType.Ignore },
      { "frameset", ElementType.Ignore },
      { "h1", ElementType.Block },
      { "h2", ElementType.Block },
      { "h3", ElementType.Block },
      { "h4", ElementType.Block },
      { "h5", ElementType.Block },
      { "h6", ElementType.Block },
      { "head", ElementType.Ignore },
      { "hr", ElementType.BlockSingle },
      { "html", ElementType.Ignore },
      { "i", ElementType.Inline },
      { "iframe", ElementType.Ignore },
      { "img", ElementType.InlineSingle },
      { "input", ElementType.Inline },
      { "ins", ElementType.Ignore },
      { "isindex", ElementType.Ignore },
      { "kbd", ElementType.Inline },
      { "label", ElementType.Inline },
      { "legend", ElementType.Ignore },
      { "li", ElementType.ListElement },
      { "link", ElementType.Ignore },
      { "map", ElementType.Inline },
      { "menu", ElementType.Ignore },
      { "meta", ElementType.Ignore },
      { "noframes", ElementType.Ignore },
      { "noscript", ElementType.Block },
      { "object", ElementType.Ignore },
      { "ol", ElementType.ListContainer },
      { "optgroup", ElementType.Ignore },
      { "option", ElementType.Ignore },
      { "p", ElementType.Block },
      { "param", ElementType.Ignore },
      { "pre", ElementType.Block },
      { "q", ElementType.Inline },
      { "s", ElementType.Inline },
      { "samp", ElementType.Inline },
      { "script", ElementType.Ignore },
      { "select", ElementType.Inline },
      { "small", ElementType.Inline },
      { "span", ElementType.Inline },
      { "strike", ElementType.Inline },
      { "strong", ElementType.Inline },
      { "style", ElementType.Ignore },
      { "sub", ElementType.Inline },
      { "sup", ElementType.Inline },
      { "table", ElementType.Table },
      { "tbody", ElementType.Ignore },
      { "td", ElementType.TableCell },
      { "textarea", ElementType.Inline },
      { "tfoot", ElementType.Ignore },
      { "th", ElementType.TableCell },
      { "thead", ElementType.Ignore },
      { "title", ElementType.Ignore },
      { "tr", ElementType.TableRow },
      { "tt", ElementType.Inline },
      { "u", ElementType.Inline },
      { "ul", ElementType.ListContainer },
      { "var", ElementType.Ignore },
    };
  }
}
