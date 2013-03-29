using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace ReaderLib.HTML
{
  /// <summary>
  /// Half-arsed HTML parser
  /// </summary>
  /// <remarks>
  /// <para>Hugely limited.</para>
  /// <para>Tags supported:</para>
  /// <list type="bullet">
  /// <item>
  /// <term>BODY</term>
  /// <description>Always present even if implicitly, provides block context</description>
  /// </item>
  /// <item>
  /// <term>Block level containers</term>
  /// <description>P, PRE, etc; appear in block context, provide inline context</description>
  /// </item>
  /// <item>
  /// <term>List containers</term>
  /// <description>OL, UL; appear in block context, provide list context</description>
  /// </item>
  /// <item>
  /// <term>List elements</term>
  /// <description>LI; appear in a list context</description></item>
  /// <item>
  /// <term>Inline elements</term>
  /// <description>B, I, etc; appear in inline context</description>
  /// </item>
  /// <item>
  /// <term>Character data</term>
  /// <description>Appears in inline context</description></item>
  /// </list>
  /// <para>This isn't really enough but it'll do for now as I don't fancy
  /// writing a complete SGML parser.</para>
  /// </remarks>
  /// TODO something better.  webkitdotnot?
  public partial class Parser
  {
    public TextReader Input { get; set; }

    public Document Document { get; set; }

    public Document Parse()
    {
      Document = new Document();
      Elements = new Stack<Element>();
      TokenType t;
      Context ctx = Context.CON;
      Stack<Context> stack = new Stack<Context>();
      TokenType lit = TokenType.eof;
      TokenType tagType = TokenType.eof;
      string tagName = null;
      string attributeName = null;
      Dictionary<string, string> attributes = null;
      StringBuilder value = new StringBuilder();
      Stack<StringBuilder> valueStack = new Stack<StringBuilder>();
      while ((t = GetToken(ctx)) != TokenType.eof) {

        if (t == TokenType.com) { // Start or end of a comment
          if (ctx == Context.CXT) {
            ctx = stack.Pop();
          }
          else {
            stack.Push(ctx);
            ctx = Context.CXT;
          }
          continue;
        }

        // Literals

        if (ctx == Context.LIT) { // In a literal
          if (t == lit) {
            ctx = stack.Pop();
            if (ctx == Context.TAG_attrvalue) {
              attributes.Add(attributeName, value.ToString());
              ctx = Context.TAG_attrname;
              value = new StringBuilder();
            }
            continue;
          }
          else {
            if (t == TokenType.lit || t == TokenType.lita) { // treat other delimiter is just a charcater
              t = TokenType.character;
            }
          }
        }

        if (t == TokenType.lit || t == TokenType.lita) { // Start of a literal
          lit = t;
          stack.Push(ctx);
          ctx = Context.LIT;
          continue;
        }

        // Processing instructions

        if (t == TokenType.pio) {
          stack.Push(ctx);
          ctx = Context.PI;
          continue;
        }
        if (t == TokenType.pic) {
          ctx = stack.Pop();
          continue;
        }

        // Markup declarations

        if (t == TokenType.mdo) {
          stack.Push(ctx);
          valueStack.Push(value);
          value = new StringBuilder();
          ctx = Context.MD;
          continue;
        }
        if (t == TokenType.mdc) {
          ctx = stack.Pop();
          value = valueStack.Pop();
          continue;
        }

        // References

        if (t == TokenType.cro || t == TokenType.ero) { // Start of a character reference
          stack.Push(ctx);
          valueStack.Push(value);
          value = new StringBuilder();
          if (t == TokenType.cro) {
            value.Append('#');
          }
          ctx = Context.REF;
          continue;
        }

        if (t == TokenType.refc) { // End of a character reference
          string entity = value.ToString();
          ctx = stack.Pop();
          value = valueStack.Pop();
          int code = -1;
          if (entity.StartsWith("#x")) {
            int.TryParse(entity.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out code);
          }
          else if (entity.StartsWith("#")) {
            int.TryParse(entity.Substring(1), NumberStyles.None, NumberFormatInfo.InvariantInfo, out code);
          }
          else {
            Entities.TryGetValue(entity, out code);
          }
          if ((code >= 0 && code <= 0xD7FF)
              || (code > 0xDFFF && code <= 0x10FFFF)) {
            value.Append(char.ConvertFromUtf32(code));
          }
          else {
            value.Append('&');
            value.Append(entity);
            if (!FakeRefc) {
              value.Append(';');
            }
          }
          if (t == TokenType.refc) {
            continue;
          }
        }

        // Tags

        if (t == TokenType.stago || t == TokenType.etago) { // Start of a tag
          if (value.Length > 0) {
            ProcessContent(value.ToString());
          }
          tagType = t;
          stack.Push(ctx);
          value = new StringBuilder();
          if (tagType == TokenType.stago) {
            attributes = new Dictionary<string, string>();
            ctx = Context.TAG_sname;
          }
          else {
            ctx = Context.TAG_ename;
          }
          continue;
        }

        if (ctx == Context.TAG_sname
            || ctx == Context.TAG_ename
            || ctx == Context.TAG_attrname
            || ctx == Context.TAG_attrvalue) { // Parsing a tag
          if (t == TokenType.character && IsWhiteSpace(LastCharacter) && value.Length == 0) { // Early whitespace
            continue;
          }
          if (t == TokenType.vi && ctx == Context.TAG_attrvalue) {
            continue;
          }
          if ((t == TokenType.character && IsWhiteSpace(LastCharacter))
             || t != TokenType.character) { // end of some name
            switch (ctx) {
              case Context.TAG_sname:
              case Context.TAG_ename:
                tagName = value.ToString().ToLowerInvariant();
                ctx = Context.TAG_attrname;
                break;
              case Context.TAG_attrname:
                attributeName = value.ToString().ToLowerInvariant();
                ctx = Context.TAG_attrvalue;
                break;
              case Context.TAG_attrvalue:
                attributes.Add(attributeName, value.ToString());
                ctx = Context.TAG_attrname;
                break;
            }
            value = new StringBuilder();
            if (t == TokenType.character && IsWhiteSpace(LastCharacter)) {
              continue;
            }
          }
        }

        if (t == TokenType.tagc) { // End of a tag
          ctx = stack.Pop();
          if (tagType == TokenType.stago) {
            ProcessOpenTag(tagName, attributes);
          }
          else {
            ProcessCloseTag(tagName);
          }
          value = new StringBuilder();
        }

        if (t == TokenType.character) { // Character data
          value.Append(LastCharacter);
        }
      }
      if (ctx == Context.CON && value.Length > 0) {
        ProcessContent(value.ToString());
      }
      if (Elements.Count == 0) {
        RequireBlockContext();
      }
      return Document;
    }

    private Stack<Element> Elements;

    private bool StackContainsElement(string name)
    {
      return (from element in Elements
              where element.Name == name
              select element).Count() > 0;
    }

    private bool StackContainsType(ElementType type)
    {
      return (from element in Elements
              where GetElementType(element.Name) == type
              select element).Count() > 0;
    }

    private void RequireHtml()
    {
      if (Elements.Count == 0) {
        ProcessOpenTag("html", null);
      }
    }

    private void RequireSomeContext()
    {
      RequireHtml();
      if (!StackContainsElement("body")) {
        while (Elements.Count > 1) {
          ProcessCloseTag(Elements.Peek().Name);
        }
        ProcessOpenTag("body", null);
      }
    }

    private void RequireBlockContext()
    {
      RequireSomeContext();
      if(Elements.Peek().Name == "table") {
        RequireTableRowContext();
      }
      if(Elements.Peek().Name == "tr") {
        RequireTableCellContext();
      }
      while (GetElementType(Elements.Peek().Name) == ElementType.Inline
             || GetElementType(Elements.Peek().Name) == ElementType.Block
             || GetElementType(Elements.Peek().Name) == ElementType.Table
             || GetElementType(Elements.Peek().Name) == ElementType.ListContainer) {
        Elements.Pop();
      }
    }

    private void RequireInlineContext()
    {
      if (!StackContainsType(ElementType.Block)) {
        RequireSomeContext();
        ProcessOpenTag("p", null);
      }
    }

    private void RequireListContext()
    {
      RequireSomeContext();
      if (GetElementType(Elements.Peek().Name) != ElementType.ListContainer) {
        if (StackContainsType(ElementType.ListContainer)) {
          while (GetElementType(Elements.Peek().Name) != ElementType.ListContainer) {
            Elements.Pop();
          }
        }
        else {
          ProcessOpenTag("ul", null);
        }
      }
    }

    private void RequireTableContext()
    {
      RequireSomeContext();
      while (Elements.Peek().Name != "table" && Elements.Peek().Name != "body") {
        Elements.Pop();
      }
      if (Elements.Peek().Name != "table") {
        ProcessOpenTag("table", null);
      }
    }

    private void RequireTableRowContext()
    {
      RequireSomeContext();
      while (Elements.Peek().Name != "tr"
             && Elements.Peek().Name != "table"
             && Elements.Peek().Name != "body") {
        Elements.Pop();
      }
      if (Elements.Peek().Name != "tr") {
        if (Elements.Peek().Name != "table") {
          ProcessOpenTag("table", null);
        }
        ProcessOpenTag("tr", null);
      }
    }

    private void RequireTableCellContext()
    {
      RequireSomeContext();
      while (Elements.Peek().Name != "td"
             && Elements.Peek().Name != "th"
             && Elements.Peek().Name != "tr"
             && Elements.Peek().Name != "table"
             && Elements.Peek().Name != "body") {
        Elements.Pop();
      }
      if (Elements.Peek().Name != "td" && Elements.Peek().Name != "th") {
        if (Elements.Peek().Name != "tr") {
          if (Elements.Peek().Name != "table") {
            ProcessOpenTag("table", null);
          }
          ProcessOpenTag("tr", null);
        }
        ProcessOpenTag("td", null);
      }
    }

    private void ProcessContent(string s)
    {
      if(IsWhiteSpace(s)) {
        if(Elements.Count == 0) {
          return;
        }
        if(GetElementType(Elements.Peek().Name) != ElementType.Block
           && GetElementType(Elements.Peek().Name) != ElementType.Inline) {
          return;
        }
      }
      RequireInlineContext();
      Elements.Peek().Contents.Add(new Cdata() { Content = s });
    }

    private void ProcessOpenTag(string name, Dictionary<string, string> attributes)
    {
      if (attributes == null) {
        attributes = new Dictionary<string, string>();
      }
      if (name == "html") {
        if (StackContainsElement("html")) {
          return;
        }
      }
      else if (name == "body") {
        if (StackContainsElement("body")) {
          return;
        }
        while (Elements.Count > 1) {
          ProcessCloseTag(Elements.Peek().Name);
        }
      }
      else {
        switch (GetElementType(name)) {
          case ElementType.Other:
          case ElementType.Ignore:
            return;
          case ElementType.ListElement:
            RequireListContext();
            break;
          case ElementType.ListContainer:
          case ElementType.Table:
          case ElementType.Block:
          case ElementType.BlockSingle:
            RequireBlockContext();
            break;
          case ElementType.Inline:
          case ElementType.InlineSingle:
            RequireInlineContext();
            break;
          case ElementType.TableRow:
            RequireTableContext();
            break;
          case ElementType.TableCell:
            RequireTableRowContext();
            break;
        }
      }
      Element newElement = new Element()
      {
        Name = name,
        Attributes = attributes,
        Contents = new List<Node>(),
      };
      if (Elements.Count > 0) {
        Elements.Peek().Contents.Add(newElement);
      }
      else {
        Document.HTML = newElement;
      }
      switch (GetElementType(name)) {
        case ElementType.InlineSingle:
        case ElementType.BlockSingle:
          break;
        default:
          Elements.Push(newElement);
          break;
      }
    }

    private void ProcessCloseTag(string name)
    {
      // Close inner containers
      if (StackContainsElement(name)) {
        while (Elements.Peek().Name != name) {
          Elements.Pop();
        }
      }
      if (Elements.Peek().Name == name) {
        Elements.Pop();
      }
    }

    private bool IsWhiteSpace(char c)
    {
      switch (c) {
        case ' ':
        case '\t':
        case '\n':
        case '\r': return true;
        default: return false;
      }
    }

    private bool IsWhiteSpace(string s)
    {
      foreach (char c in s) {
        if (!IsWhiteSpace(c)) {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Token types
    /// </summary>
    private enum TokenType
    {
      eof,        // end of input
      // Known delimiters
      com,        // --
      cro,        // &#
      ero,        // &
      etago,      // </
      lit,        // "
      lita,       // '
      mdc,        // >
      mdo,        // <!
      pic,        // >
      pio,        // <?
      refc,       // ;
      stago,      // <
      tagc,       // >
      vi,         // =
      // Other
      character,  // character data
    }

    /// <summary>
    /// Recognition contexts
    /// </summary>
    private enum Context
    {
      CON,
      CXT,
      DS,
      DSM,
      GRP,
      LIT,
      MD,
      PI,
      REF,
      TAG_sname, // expecting name (start tag)
      TAG_ename, // expecting name (end tag)
      TAG_attrname, // expecting attribute name
      TAG_attrvalue, // expecting attribute value
    };

    private delegate TokenType ParseDelegate();

    private char LastCharacter;

    private bool FakeRefc;

    /// <summary>
    /// Get a single token
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    private TokenType GetToken(Context ctx)
    {
      int ch = Input.Peek();
      if (ch == -1) {
        return TokenType.eof;
      }
      // Special case to detect unterminated entity references
      if (ctx == Context.REF && !char.IsLetterOrDigit((char)ch)) {
        if (ch == (int)';') {
          Input.Read();
          FakeRefc = false;
        }
        else {
          FakeRefc = true;
        }
        return TokenType.refc;
      }
      Input.Read();
      LastCharacter = (char)ch;
      switch (LastCharacter) {
        case '-':
          switch (ctx) {
            case Context.CXT: return TokenType.com;
            case Context.MD: return TokenType.com;
          }
          break;
        case '&':
          switch (ctx) {
            case Context.CON:
            case Context.LIT:
              if (Input.Peek() == (int)'#') {
                Input.Read();
                return TokenType.cro;
              }
              return TokenType.ero;
          }
          break;
        case '<':
          switch (ctx) {
            case Context.CON:
            case Context.DSM:
              if (Input.Peek() == (char)'!') {
                Input.Read();
                return TokenType.mdo;
              }
              if (Input.Peek() == (char)'?') {
                Input.Read();
                return TokenType.pio;
              }
              break;
          }
          switch (ctx) {
            case Context.CON:
            case Context.TAG_sname:
            case Context.TAG_ename:
            case Context.TAG_attrname:
            case Context.TAG_attrvalue:
              if (Input.Peek() == (int)'/') {
                Input.Read();
                return TokenType.etago;
              }
              return TokenType.stago;
          }
          break;
        case '"':
          switch (ctx) {
            case Context.GRP: return TokenType.lit;
            case Context.LIT: return TokenType.lit;
            case Context.MD: return TokenType.lit;
            case Context.TAG_sname: return TokenType.lit;
            case Context.TAG_ename: return TokenType.lit;
            case Context.TAG_attrname: return TokenType.lit;
            case Context.TAG_attrvalue: return TokenType.lit;
          }
          break;
        case '\'':
          switch (ctx) {
            case Context.GRP: return TokenType.lita;
            case Context.LIT: return TokenType.lita;
            case Context.MD: return TokenType.lita;
            case Context.TAG_sname: return TokenType.lita;
            case Context.TAG_ename: return TokenType.lita;
            case Context.TAG_attrname: return TokenType.lita;
            case Context.TAG_attrvalue: return TokenType.lita;
          }
          break;
        case '>':
          switch (ctx) {
            case Context.MD: return TokenType.mdc;
            case Context.PI: return TokenType.pic;
            case Context.CXT: return TokenType.mdc;
            case Context.TAG_sname: return TokenType.tagc;
            case Context.TAG_ename: return TokenType.tagc;
            case Context.TAG_attrname: return TokenType.tagc;
            case Context.TAG_attrvalue: return TokenType.tagc;
          }
          break;
        case '=':
          switch (ctx) {
            case Context.TAG_attrname: return TokenType.vi;
          }
          break;
      }
      return TokenType.character;
    }
  }
}
