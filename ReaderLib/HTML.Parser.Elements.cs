// This file is part of HalfMoon RSS reader
// Copyright (C) 2013 Richard Kettlewell
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;

namespace ReaderLib.HTML
{
  public partial class Parser
  {
    [Flags]
    internal enum ElementType {
      Other = 0x1,
      Block = 0x2,
      BlockSingle = 0x4,
      FlowContainer = 0x8,
      Inline = 0x10,
      InlineSingle = 0x20,
      ListContainer = 0x40,
      ListElement = 0x80,
      Table = 0x100,
      TableRow = 0x200,
      TableCell = 0x400,
      HeadContent = 0x800,
      HeadSpecial = 0x1000,
      HeadSingle = 0x2000,
      Ignore = 0x4000,
      Body = 0x8000,
      Head = 0x10000,
      HTML = 0x20000,
      TableContent = 0x40000,
      DefinitionList = 0x80000,
      Definition = 0x100000,
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
      { "base", ElementType.HeadSingle },
      { "basefont", ElementType.Ignore },
      { "bdo", ElementType.Ignore },
      { "big", ElementType.Inline },
      { "blockquote", ElementType.FlowContainer },
      { "body", ElementType.Body },
      { "br", ElementType.InlineSingle },
      { "button", ElementType.Inline },
      { "caption", ElementType.TableContent },
      { "center", ElementType.Ignore },
      { "cite", ElementType.Inline },
      { "code", ElementType.Inline },
      { "col", ElementType.Ignore },
      { "colgroup", ElementType.Ignore },
      { "dd", ElementType.Definition },
      { "del", ElementType.Ignore },
      { "dfn", ElementType.Inline },
      { "dir", ElementType.Ignore },
      { "div", ElementType.FlowContainer },
      { "dl", ElementType.DefinitionList },
      { "dt", ElementType.Definition },
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
      { "head", ElementType.Head },
      { "hr", ElementType.BlockSingle },
      { "html", ElementType.HTML },
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
      { "link", ElementType.HeadSingle },
      { "map", ElementType.Inline },
      { "menu", ElementType.Ignore },
      { "meta", ElementType.HeadSingle },
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
      { "script", ElementType.HeadSpecial },
      { "select", ElementType.Inline },
      { "small", ElementType.Inline },
      { "span", ElementType.Inline },
      { "strike", ElementType.Inline },
      { "strong", ElementType.Inline },
      { "style", ElementType.HeadSpecial },
      { "sub", ElementType.Inline },
      { "sup", ElementType.Inline },
      { "table", ElementType.Table },
      { "tbody", ElementType.Ignore },
      { "td", ElementType.TableCell },
      { "textarea", ElementType.Inline },
      { "tfoot", ElementType.Ignore },
      { "th", ElementType.TableCell },
      { "thead", ElementType.Ignore },
      { "title", ElementType.HeadContent },
      { "tr", ElementType.TableRow },
      { "tt", ElementType.Inline },
      { "u", ElementType.Inline },
      { "ul", ElementType.ListContainer },
      { "var", ElementType.Ignore },
    };
  }
}
