using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HTML = ReaderLib.HTML;

namespace RssReader
{
  /// <summary>
  /// Convert an HTML document to a FlowDocument
  /// </summary>
  public class RenderHTML
  {
    /// <summary>
    /// Monospace font
    /// </summary>
    public FontFamily MonospaceFont = new FontFamily("Global Monospace");

    /// <summary>
    /// Text font
    /// </summary>
    public FontFamily TextFont = new FontFamily("Global Serif");

    /// <summary>
    /// Heading font
    /// </summary>
    public FontFamily HeadingFont = new FontFamily("Global Sans Serif");

    /// <summary>
    /// Containing scrollviewer
    /// </summary>
    public ScrollViewer ParentScrollViewer = null;

    /// <summary>
    /// Render an HTML document
    /// </summary>
    /// <param name="document"></param>
    /// <param name="scrollviewer">Containing viewer (for width bound)</param>
    /// <returns></returns>
    public FrameworkElement Render(HTML.Document document)
    {
      FlowDocument fd = new FlowDocument();
      fd.SetBinding(FlowDocument.PageWidthProperty,
                    new Binding()
                    {
                      Source = ParentScrollViewer,
                      Path = new PropertyPath("ViewportWidth"),
                    });
      fd.Blocks.AddRange(ConvertFlowsOrBlocks(document.HTML.Follow("body")));
      return new RichTextBox() { 
        Document = fd,
        IsDocumentEnabled = true,
        IsReadOnly = true,
        BorderThickness = new Thickness(0),
      };
    }

    private IEnumerable<Block> ConvertFlowsOrBlocks(HTML.Element root)
    {
      List<Block> blocks = new List<Block>();
      foreach (HTML.Node n in root.Contents) {
        blocks.AddRange(ConvertFlowOrBlock(n as HTML.Element));
      }
      return blocks;
    }

    private IEnumerable<Block> ConvertFlowOrBlock(HTML.Element e)
    {
      if (e.IsBlockElement()
          || e.IsListElement()
          || e.IsTableElement() ) {
        Block block;
        switch (e.Name) {
          case "ul":
            block = ConvertList(e);
            break;
          case "ol":
            List l = ConvertList(e);
            l.StartIndex = 1;
            block = l;
            break;
          case "h1":
          case "h2":
          case "h3":
          case "h4":
          case "h5":
          case "h6":
            block = ConvertParagraph(e, HeadingFont, FontWeights.Bold, Math.Pow(1.125, '7' - e.Name[1]), true);
            break;
          case "pre":
            block = ConvertParagraph(e, MonospaceFont, FontWeights.Normal, 1.0, false);
            break;
          case "table":
            block = ConvertTable(e);
            break;
          // TODO <hr>?
          default:
            block = ConvertParagraph(e, TextFont, FontWeights.Normal, 1.0, true);
            break;
        }
        if (block != null) {
          return new List<Block>() { block };
        }
        else {
          return new List<Block>();
        }
      }
      else {
        IEnumerable<Block> content;
        if (e.Contents.Count > 0
            && (e.Contents[0] is HTML.Cdata
                || (e.Contents[0] as HTML.Element).IsInlineElement())) {
          content = new List<Block>() { ConvertParagraph(e, TextFont, FontWeights.Normal, 1.0, true) };
        }
        else {
          content = ConvertFlowsOrBlocks(e);
        }
        switch (e.Name) {
          case "blockquote":
            foreach (Block b in content) {
              Thickness t = b.Margin;
              double left = double.IsNaN(t.Left) ? 0 : t.Left;
              double right = double.IsNaN(t.Right) ? 0 : t.Right;
              b.Margin = new Thickness(left + 12, t.Top, right + 12, t.Bottom);
              break;
            }
            break;
            // TODO other kinds of flow container
        }
        return content;
      }
    }

    #region Table Rendering

    private Table ConvertTable(HTML.Element e)
    {
      Table t = new Table();
      TableRowGroup trg = new TableRowGroup();
      t.RowGroups.Add(trg);
      int columns = 0;
      foreach(HTML.Element r in e.Contents) {
        TableRow tr = new TableRow();
        trg.Rows.Add(tr);
        int columnNumber = 0;
        foreach(HTML.Element c in r.Contents) {
          TableCell tc = new TableCell()
          {
            ColumnSpan = GetSpan(c, "colspan"),
            RowSpan = GetSpan(c, "rowspan"),
          };
          tc.Blocks.AddRange(ConvertFlowOrBlock(c));
          tr.Cells.Add(tc);
          columnNumber += tc.ColumnSpan;
        }
        if (columnNumber > columns) {
          columns = columnNumber;
        }
      }
      for (int i = 0; i < columns; ++i) {
        t.Columns.Add(new TableColumn());
      }
      return t;
    }

    static private int GetSpan(HTML.Element e, string attribute)
    {
      string attributeValue;
      if (e.Attributes.TryGetValue(attribute, out attributeValue)) {
        int value;
        if (int.TryParse(attributeValue, out value)) {
          if (value < 1) {
            value = 1;
          }
          return value;
        }
      }
      return 1;
    }

    #endregion

    #region Lists

    private List ConvertList(HTML.Element e)
    {
      List l = new List();
      foreach (HTML.Element ee in e.Contents) {
        ListItem li = new ListItem();
        li.Blocks.AddRange(ConvertFlowOrBlock(ee));
        l.ListItems.Add(li);
      }
      return l;
    }

    #endregion

    #region Paragraphs

    private char LastCharacter;

    private Paragraph ConvertParagraph(HTML.Element e, FontFamily fontFamily, FontWeight fontWeight, double fontScale, bool collapseSpace)
    {
      LastCharacter = ' ';
      Paragraph p = new Paragraph();
      p.Inlines.AddRange(ConvertInlines(e, fontFamily, fontWeight, fontScale, collapseSpace,
                                        new ParagraphSizeTracker() { ParentScrollViewer = ParentScrollViewer }));
      return p.Inlines.Count > 0 ? p : null;
    }

    private IEnumerable<Inline> ConvertInlines(HTML.Element e, FontFamily fontFamily, FontWeight fontWeight, double fontScale, bool collapseSpace, ParagraphSizeTracker pst)
    {
      return from i in
               (from n in e.Contents select ConvertInline(n, fontFamily, fontWeight, fontScale, collapseSpace, pst)) 
             where i != null
             select i;
    }

    #endregion

    #region Inlines

    private Inline ConvertInline(HTML.Node n, FontFamily fontFamily, FontWeight fontWeight, double fontScale, bool collapseSpace, ParagraphSizeTracker pst)
    {
      if (n is HTML.Element) {
        return ConvertInlineElement((HTML.Element)n, fontFamily, fontWeight, fontScale, collapseSpace, pst);
      }
      else {
        Inline i = collapseSpace ? ConvertText(((HTML.Cdata)n).Content)
                                 : ConvertFixedText(((HTML.Cdata)n).Content);
        if (i != null) {
          i.FontSize *= fontScale;
          i.FontFamily = fontFamily;
          i.FontWeight = fontWeight;
        }
        return i;
      }
    }

    private Inline ConvertInlineElement(HTML.Element e, FontFamily fontFamily, FontWeight fontWeight, double fontScale, bool collapseSpace, ParagraphSizeTracker pst)
    {
      Span s;
      switch (e.Name) {
        case "b":
        case "strong": s = new Span(); fontWeight = FontWeights.Bold; break;
        case "i":
        case "em": s = new Italic(); break;
        case "u": s = new Underline(); break;
        case "big": s = new Span(); fontScale *= 1.125; break;
        case "small": s = new Span(); fontScale /= 1.125; break;
        case "code":
        case "tt": s = new Span(); fontFamily = MonospaceFont; break;
        case "sub": s = new Span(); s.Typography.Variants = FontVariants.Subscript; break;
        case "sup": s = new Span(); s.Typography.Variants = FontVariants.Superscript; break;
        case "a":
          s = RenderHyperlink(e);
          break;
        case "img":
          return RenderImage(e, pst);
        case "br":
          return new LineBreak();
        // TODO map, anything else?
        default: s = new Span(); break;
      }
      s.Inlines.AddRange(ConvertInlines(e, fontFamily, fontWeight, fontScale, collapseSpace, pst));
      return s;
    }

    private Inline ConvertText(string text)
    {
      StringBuilder sb = new StringBuilder();
      foreach (char c in text) {
        char ch;
        switch (c) {
          case ' ':
          case '\t':
          case '\n':
          case '\r':
            if (LastCharacter == ' ') {
              continue;
            }
            ch = ' ';
            break;
          default:
            ch = c;
            break;
        }
        sb.Append(ch);
        LastCharacter = ch;
      }
      string s = sb.ToString();
      return s.Length > 0 ? new Run(s) : null;
    }

    private Inline ConvertFixedText(string text)
    {
      Span span = new Span();
      StringBuilder sb = null;
      foreach (char c in text) {
        switch (c) {
          case '\n':
            if (sb != null) {
              if (sb.Length > 0) {
                span.Inlines.Add(new Run(sb.ToString()));
              }
              sb = null;
            }
            span.Inlines.Add(new LineBreak());
            break;
          default:
            if (sb == null) {
              sb = new StringBuilder();
            }
            sb.Append(c);
            break;
        }
      }
      if (sb != null && sb.Length > 0) {
        span.Inlines.Add(new Run(sb.ToString()));
      }
      return span;
    }

    private Hyperlink RenderHyperlink(HTML.Element e)
    {
      Hyperlink h = new Hyperlink();
      if (e.Attributes.ContainsKey("href")) {
        Uri TargetURI;
        if (Uri.TryCreate(e.Attributes["href"], UriKind.RelativeOrAbsolute, out TargetURI)) {
          h.NavigateUri = TargetURI;
          h.RequestNavigate += RequestedNavigate;
        }
        h.ToolTip = e.Attributes["href"]; // tooltip contains URI even if it's bad
      }
      return h;
    }

    static private void RequestedNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Uri.ToString());
      e.Handled = true;
    }

    private InlineUIContainer RenderImage(HTML.Element e, ParagraphSizeTracker pst)
    {
      Uri ImageURI;
      if (e.Attributes.ContainsKey("src")
          && Uri.TryCreate(e.Attributes["src"], UriKind.RelativeOrAbsolute, out ImageURI)) {
        Image image = new Image() { Stretch = Stretch.Fill };
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.UriSource = ImageURI;
        bitmapImage.EndInit();
        image.Source = bitmapImage;
        if (ImageSizeCache.ContainsKey(ImageURI)) {
          Tuple<int, int> size = ImageSizeCache[ImageURI];
          image.Width = size.Item1;
          image.Height = size.Item2;
        }
        // This hack forces image size to match pixel size,
        // which undermines WPF's preference for honoring the
        // embedded DPI value which leads to upscaled and
        // mildly blurry images where that doesn't match
        // the local display.  Very high resolution displays
        // might need another approach.
        bitmapImage.DownloadCompleted += (s, ee) =>
        {
          ImageSizeCache[ImageURI] = new Tuple<int, int>(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
          image.Width = bitmapImage.PixelWidth;
          image.Height = bitmapImage.PixelHeight;
        };
        // Communicate image size info back up to the container
        if (pst != null) {
          pst.Add(image);
        }
        if (e.Attributes.ContainsKey("title")) {
          image.ToolTip = e.Attributes["title"];
        }
        else if (e.Attributes.ContainsKey("alt")) {
          image.ToolTip = e.Attributes["alt"];
        }
        // TODO arrange to display alt value if image can't be loaded
        // (or before it has loaded)
        return new InlineUIContainer() { Child = image };
      }
      else {
        return null;
      }
    }

    static private Dictionary<Uri, Tuple<int, int>> ImageSizeCache = new Dictionary<Uri, Tuple<int, int>>();

    #endregion

    private class ParagraphSizeTracker : INotifyPropertyChanged
    {
      public double Width
      {
        get
        {
          double m = 96; // minimum
          foreach (Image image in Images) {
            if (!double.IsNaN(image.Width) && image.Width > m) {
              m = image.Width;
            }
          }
          if (ParentScrollViewer != null) {
            double cw = ParentScrollViewer.ViewportWidth - 4;
            if (cw > m) {
              m = cw;
            }
          }
          return m;
        }
      }

      public void Add(Image i)
      {
        Images.Add(i);
        i.SizeChanged += ChildSizeChanged;
      }

      private Collection<Image> Images = new Collection<Image>();

      private void ChildSizeChanged(object sender, SizeChangedEventArgs e)
      {
        OnPropertyChanged("Width");
      }

      public ScrollViewer ParentScrollViewer
      {
        get
        {
          return _ParentScrollViewer;
        }
        set
        {
          if (_ParentScrollViewer != null) {
            _ParentScrollViewer.ScrollChanged -= ParentScrollViewerChanged;
          }
          if (value != null) {
            value.ScrollChanged += ParentScrollViewerChanged;
          }
          _ParentScrollViewer = value;
        }
      }

      private ScrollViewer _ParentScrollViewer = null;

      private void ParentScrollViewerChanged(object sender, ScrollChangedEventArgs e)
      {
        if (e.ViewportWidthChange != 0) {
          OnPropertyChanged("Width");
        }
      }

      #region INotifyPropertyChanged

      public event PropertyChangedEventHandler PropertyChanged;

      public void OnPropertyChanged(string propertyName)
      {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null) {
          handler(this, new PropertyChangedEventArgs(propertyName));
        }
      }

      #endregion

    }

  }
}
