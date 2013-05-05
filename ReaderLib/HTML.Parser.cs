using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

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
  /// <term>Block inline containers</term>
  /// <description>P, PRE, etc; appear in block context, provide inline context</description>
  /// </item>
  /// <item>
  /// <item>
  /// <term>Flow containers</term>
  /// <description>BLOCKQUOTE, etc; appear in block context, provide block or inline context</description>
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
  /// <item>
  /// <term>Table elements</term>
  /// <description>Appears in block context, cells provide block context</description>
  /// </item>
  /// <term>Character data</term>
  /// <description>Appears in inline context</description></item>
  /// </list>
  /// <para>This isn't really enough but it'll do for now as I don't fancy
  /// writing a complete SGML parser.</para>
  /// </remarks>
  /// TODO something better.  webkitdotnot?
  public partial class Parser
  {
    /// <summary>
    /// HTML is read from here
    /// </summary>
    public TextReader Input { get; set; }

    /// <summary>
    /// The parsed HTML document
    /// </summary>
    public Document Document { get; set; }

    /// <summary>
    /// Parse from the current input
    /// </summary>
    /// <returns>The parsed HTML document</returns>
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
      if (Peek() == (char)0xFEFF) { // BOM
        Read();
      }
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
                if (!attributes.ContainsKey(attributeName)) {
                  attributes.Add(attributeName, value.ToString());
                }
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
            if (GetElementType(tagName) == ElementType.HeadSpecial) {
              ctx = Context.CON_special;
            }
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
      if (Document.HTML == null) {
        RequireHtml();
      }
      if (Document.HTML.FindChild("head") == null) {
        RequireHead();
      }
      if (Document.HTML.FindChild("body") == null) {
        RequireBody();
      }
      return Document;
    }

    /// <summary>
    /// Stack of currently-open elements
    /// </summary>
    private Stack<Element> Elements;

    private Element StackTop() {
      return Elements.Peek();
    }

    /// <summary>
    /// Test whether a named element is open (at any level of the stack)
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private bool StackContainsElement(string name)
    {
      return (from element in Elements
              where element.Name == name
              select element).Count() > 0;
    }

    /// <summary>
    /// Test whether an element of given type is open (at any level of the stack)
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private bool StackContainsTypes(ElementType type)
    {
      return (from element in Elements
              where (GetElementType(element.Name) & type) != 0
              select element).Count() > 0;
    }

    private bool StackTopTypeIs(ElementType type)
    {
      return (GetElementType(StackTop().Name) & type) != 0;
    }

    private bool StackTopIs(string name)
    {
      return StackTop().Name == name;
    }

    private bool StackTopIsNot(string name)
    {
      return !StackTopIs(name);
    }

    /// <summary>
    /// Ensure that the &lt;html> element is open
    /// </summary>
    private void RequireHtml()
    {
      if (Elements.Count == 0) {
        ProcessOpenTag("html", null);
      }
    }

    /// <summary>
    /// Ensure that the &lt;head> element is open
    /// </summary>
    private void RequireHead()
    {
      RequireHtml();
      if (!StackContainsElement("head")) {
        while (Elements.Count > 1) {
          ProcessCloseTag(StackTop().Name);
        }
        ProcessOpenTag("head", null);
      }
    }

    /// <summary>
    /// Ensure that the &lt;body> element is open
    /// </summary>
    /// <remarks>If there is no &lt;head> element, synthesizes an empty one</remarks>
    private void RequireBody()
    {
      RequireHtml();
      if (Document.HTML.Contents.Count == 0) {
        RequireHead();
      }
      if (!StackContainsElement("body")) {
        while (Elements.Count > 1) {
          ProcessCloseTag(StackTop().Name);
        }
        ProcessOpenTag("body", null);
      }
    }

    /// <summary>
    /// Ensure that the top of the stack provides block context
    /// </summary>
    private void RequireBlockContext()
    {
      RequireBody();
      if(StackTopIs("table")) {
        RequireTableRowContext();
      }
      if(StackTopIs("tr")) {
        RequireTableCellContext();
      }
      while (StackTopTypeIs(ElementType.Inline | ElementType.TableContent | ElementType.Block | ElementType.Table | ElementType.ListContainer | ElementType.DefinitionList)) {
        Elements.Pop();
      }
      // If we are in a flow container that has already seen inline content then
      // retroactively capture that context in a block container
      if (StackTopTypeIs(ElementType.FlowContainer)
          && StackTop().Contents.Count > 0
          && (StackTop().Contents[0] is HTML.Cdata
              || (GetElementType((StackTop().Contents[0] as HTML.Element).Name) & (ElementType.InlineSingle | ElementType.Inline)) != 0)) {
        ProcessCloseTag("p");
      }
    }

    /// <summary>
    /// Ensure inline context
    /// </summary>
    private void RequireInlineContext()
    {
      if (!StackContainsTypes(ElementType.Block | ElementType.TableContent | ElementType.FlowContainer | ElementType.TableCell | ElementType.ListElement | ElementType.Definition)) {
        RequireBody();
        ProcessOpenTag("p", null);
      }
    }

    /// <summary>
    /// Ensure list context
    /// </summary>
    private void RequireListContext()
    {
      RequireBody();
      if (!StackTopTypeIs(ElementType.ListContainer)) {
        if (StackContainsTypes(ElementType.ListContainer)) {
          while (!StackTopTypeIs(ElementType.ListContainer)) {
            Elements.Pop();
          }
        }
        else {
          ProcessOpenTag("ul", null);
        }
      }
    }

    /// <summary>
    /// Ensure table context
    /// </summary>
    private void RequireTableContext()
    {
      RequireBody();
      while (StackTopIsNot("table") && StackTopIsNot("body")) {
        Elements.Pop();
      }
      if (StackTopIsNot("table")) {
        ProcessOpenTag("table", null);
      }
    }

    /// <summary>
    /// Ensure table row context
    /// </summary>
    private void RequireTableRowContext()
    {
      RequireBody();
      while (StackTopIsNot("tr") && StackTopIsNot("table")  && StackTopIsNot("body")) {
        Elements.Pop();
      }
      if (StackTopIsNot("tr")) {
        if (StackTopIsNot("table")) {
          ProcessOpenTag("table", null);
        }
        ProcessOpenTag("tr", null);
      }
    }

    /// <summary>
    /// Ensure table cell context
    /// </summary>
    private void RequireTableCellContext()
    {
      RequireBody();
      while (StackTopIsNot("td")
             && StackTopIsNot("th")
             && StackTopIsNot("tr")
             && StackTopIsNot("table")
             && StackTopIsNot("body")) {
        Elements.Pop();
      }
      if (StackTopIsNot("td") && StackTopIsNot("th")) {
        if (StackTopIsNot("tr")) {
          if (StackTopIsNot("table")) {
            ProcessOpenTag("table", null);
          }
          ProcessOpenTag("tr", null);
        }
        ProcessOpenTag("td", null);
      }
    }

    private void RequireDefinitionContext()
    {
      RequireBody();
      if (!StackContainsTypes(ElementType.DefinitionList)) {
        ProcessOpenTag("dl", null);
      }
      while (StackTopIsNot("dl")) {
        ProcessCloseTag(StackTop().Name);
      }
    }

    /// <summary>
    /// Process string content
    /// </summary>
    /// <param name="s"></param>
    private void ProcessContent(string s)
    {
      if(IsWhiteSpace(s)) {
        if(Elements.Count == 0) {
          return;
        }
        if(!StackTopTypeIs(ElementType.Block | ElementType.Inline | ElementType.Definition)) {
          return;
        }
      }
      if (StackContainsElement("head")) {
        if (StackTopTypeIs(ElementType.HeadContent | ElementType.HeadSpecial)) {
          StackTop().Contents.Add(new Cdata() { Content = s });
          return;
        }
      }
      RequireInlineContext();
      StackTop().Contents.Add(new Cdata() { Content = s });
    }

    /// <summary>
    /// Process an open tag
    /// </summary>
    /// <param name="name"></param>
    /// <param name="attributes"></param>
    private void ProcessOpenTag(string name, Dictionary<string, string> attributes)
    {
      if (attributes == null) {
        attributes = new Dictionary<string, string>();
      }
      switch (GetElementType(name)) {
        case ElementType.Other:
        case ElementType.Ignore:
          return;
        case ElementType.HTML:
          if (StackContainsElement("html")) {
            return;
          }
          break;
        case ElementType.Body:
        case ElementType.Head:
          RequireHtml();
          if (StackContainsElement(name)) {
            return;
          }
          while (Elements.Count > 1) {
            ProcessCloseTag(StackTop().Name);
          }
          break;
        case ElementType.ListElement:
          RequireListContext();
          break;
        case ElementType.ListContainer:
        case ElementType.Table:
        case ElementType.Block:
        case ElementType.BlockSingle:
        case ElementType.FlowContainer:
        case ElementType.DefinitionList:
          RequireBlockContext();
          break;
        case ElementType.Inline:
        case ElementType.InlineSingle:
          RequireInlineContext();
          break;
        case ElementType.TableRow:
        case ElementType.TableContent:
          RequireTableContext();
          break;
        case ElementType.TableCell:
          RequireTableRowContext();
          break;
        case ElementType.HeadContent:
        case ElementType.HeadSingle:
        case ElementType.HeadSpecial:
          RequireHead();
          break;
        case ElementType.Definition:
          RequireDefinitionContext();
          break;
      }
      Element newElement = new Element()
      {
        Name = name,
        Attributes = attributes,
        Contents = new List<Node>(),
      };
      if (Elements.Count > 0) {
        StackTop().Contents.Add(newElement);
      }
      else {
        Document.HTML = newElement;
      }
      switch (GetElementType(name)) {
        case ElementType.InlineSingle:
        case ElementType.BlockSingle:
        case ElementType.HeadSingle:
          break;
        default:
          Elements.Push(newElement);
          break;
      }
    }

    private void ProcessCloseTag(string name)
    {
      if (StackContainsElement(name)) {
        // Close inner containers
        while (StackTop().Name != name) {
          Elements.Pop();
        }
        if (StackTop().Name == name) { // should never be false
          Elements.Pop();
        }
      }
      else {
        switch (GetElementType(name)) {
          case ElementType.Block:
            // For instance, </pre> without a preceding <pre>
            if (StackContainsTypes(ElementType.FlowContainer | ElementType.Body)) {
              // e.g. <div>foobar</pre></div>
              while (!StackTopTypeIs(ElementType.FlowContainer | ElementType.Body)) {
                Elements.Pop();
              }
              HTML.Element block = new HTML.Element()
              {
                Name = name,
                Contents = StackTop().Contents,
              };
              StackTop().Contents = new List<HTML.Node>() { block };
            }
            break;
        }
        // If we can't cope we just give up on this one
        return;
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

    #region SGML Tokenization

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
      CON_special, // CON but inside <script> or <style> (HTML is *weird*)
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
      int ch = Peek();
      if (ch == -1) {
        return TokenType.eof;
      }
      // Special case to detect unterminated entity references
      if (ctx == Context.REF && !char.IsLetterOrDigit((char)ch)) {
        if (ch == (int)';') {
          Read();
          FakeRefc = false;
        }
        else {
          FakeRefc = true;
        }
        return TokenType.refc;
      }
      Read();
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
              if (Peek() == (int)'#') {
                Read();
                return TokenType.cro;
              }
              return TokenType.ero;
          }
          break;
        case '<':
          switch (ctx) {
            case Context.CON:
            case Context.DSM:
              if (Peek() == (char)'!') {
                Read();
                return TokenType.mdo;
              }
              if (Peek() == (char)'?') {
                Read();
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
              if (Peek() == (int)'/') {
                Read();
                return TokenType.etago;
              }
              return TokenType.stago;
            case Context.CON_special:
              if (Peek() == (int)'/') {
                Read();
                return TokenType.etago;
              }
              break;
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

    #endregion

    #region Character Input

    // StreamReader.Peek doesn't always work

    private Stack<int> PushedBack = new Stack<int>();

    private int Peek()
    {
      int ch = Read();
      if (ch != -1) {
        Pushback(ch);
      }
      return ch;
    }

    private int Read()
    {
      if (PushedBack.Count > 0) {
        return PushedBack.Pop();
      }
      else {
        return Input.Read();
      }
    }

    private void Pushback(int ch)
    {
      PushedBack.Push(ch);
    }

    #endregion

  }
}
