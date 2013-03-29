using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReaderLib;
using HTML = ReaderLib.HTML;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace RssReader
{
  public class RenderHTML
  {
    static readonly FontFamily Monospace = new FontFamily("Global Monospace");

    static public FrameworkElement Render(HTML.Document document,
                                          ScrollViewer scrollviewer)
    {
      RenderHTML renderer = new RenderHTML();
      return renderer.RenderBlocks(document.HTML.Follow("body"), scrollviewer);
    }

    private FrameworkElement RenderBlocks(HTML.Element root,
                                          ScrollViewer scrollviewer)
    {
      /*
       * TextBlock container = new TextBlock()
              {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(1),
              };
            }
       */
      StackPanel container = new StackPanel();
      UIElementCollection children = container.Children;
      for (int i = 0; i < root.Contents.Count; ++i) {
        HTML.Element e = root.Contents[i] as HTML.Element;
        //Console.WriteLine("<{0}>", e.Name);
        switch (e.Name) {
          case "ul":
            children.Add(RenderList(e,
                                    y => "•",
                                    scrollviewer));
            //children.Add(new LineBreak());
            break;
          case "ol":
            children.Add(RenderList(e,
                                    y => string.Format("{0}.", y + 1),
                                    scrollviewer));
            //children.Add(new LineBreak());
            break;
          case "h1":
          case "h2":
          case "h3":
          case "h4":
          case "h5":
          case "h6":
            children.Add(RenderParagraph(e, Math.Pow(1.125, '7' - e.Name[1]), FontWeights.Bold, null, scrollviewer));
            if (i + 1 != root.Contents.Count) {
              //children.Add(new LineBreak());
            }
            break;
          case "pre":
            children.Add(RenderParagraph(e, 1, FontWeights.Normal, Monospace, scrollviewer));
            if (i + 1 != root.Contents.Count) {
              //children.Add(new LineBreak());
            }
            break;
          case "table":
            children.Add(RenderTable(e, scrollviewer));
            //children.Add(new LineBreak());
            break;
          default:
            children.Add(RenderParagraph(e, 1, FontWeights.Normal, null, scrollviewer));
            if (i + 1 != root.Contents.Count) {
              //children.Add(new LineBreak());
            }
            break;
        }
        //Console.WriteLine("</{0}>", e.Name);
      }
      return container;
    }

    private struct TablePosition
    {
      public TablePosition(int row, int col)
      {
        this.row = row;
        this.col = col;
      }
      public int row;
      public int col;
    }

    private static bool Usable(ISet<TablePosition> occupied,
                               int row, int col, int colspan)
    {
      while (colspan > 0) {
        if (occupied.Contains(new TablePosition(row, col))) {
          return false;
        }
        ++col;
        --colspan;
      }
      return true;
    }

    UIElement RenderTable(HTML.Element e, ScrollViewer scrollviewer)
    {
      Grid g = new Grid();
      if (scrollviewer != null) {
        g.SetBinding(Grid.MaxWidthProperty,
                     new Binding()
                     {
                       Source = scrollviewer,
                       Path = new PropertyPath("ViewportWidth"),
                     });
      }
      HashSet<TablePosition> occupied = new HashSet<TablePosition>();
      int maxrow = 0;
      int maxcol = 0;
      int row = 0;
      int col;
      Console.WriteLine("Render table: {0}", e);
      foreach (HTML.Element r in e.Contents) {
        col = 0;
        foreach (HTML.Element c in e.Contents) {
          int colspan = GetSpan(c, "colspan");
          int rowspan = GetSpan(c, "rowspan");
          while (!Usable(occupied, row, col, colspan)) {
            ++col;
          }
          if (c.Contents.Count != 0) {
            FrameworkElement content = RenderBlocks(c, scrollviewer);
            Grid.SetColumn(content, col);
            Grid.SetColumnSpan(content, colspan);
            Grid.SetRow(content, row);
            Grid.SetRowSpan(content, rowspan);
            g.Children.Add(content);
          }
          for (int cn = 0; cn < colspan; ++cn) {
            for (int rn = 0; rn < rowspan; ++rn) {
              occupied.Add(new TablePosition(row + rn, col + cn));
            }
          }
          if (row + rowspan - 1 > maxrow) {
            maxrow = row + rowspan - 1;
          }
          col += colspan;
        }
        if (col - 1 > maxcol) {
          maxcol = col - 1;
        }
        ++row;
      }
      for (row = 0; row <= maxrow; ++row) {
        g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
      }
      for (col = 0; col <= maxcol; ++col) {
        g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
      }
      return g;
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

    private UIElement RenderList(HTML.Element e, Func<int, string> makeBullet, ScrollViewer scrollviewer)
    {
      Grid g = new Grid();
      if (scrollviewer != null) {
        g.SetBinding(Grid.MaxWidthProperty,
                     new Binding()
                     {
                       Source = scrollviewer,
                       Path = new PropertyPath("ViewportWidth"),
                     });
      }
      g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
      g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
      int y = 0;
      foreach (HTML.Element ee in e.Contents) {
        //Console.WriteLine("<{0}>", ee.Name);
        g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        TextBlock bullet = new TextBlock()
        {
          Text = makeBullet(y),
          Padding = new Thickness(2),
          HorizontalAlignment = HorizontalAlignment.Right,
        };
        Grid.SetColumn(bullet, 0);
        Grid.SetRow(bullet, y);
        g.Children.Add(bullet);
        FrameworkElement content = RenderBlocks(ee, scrollviewer);
        Grid.SetColumn(content, 1);
        Grid.SetRow(content, y);
        g.Children.Add(content);
        ++y;
        //Console.WriteLine("</{0}>", ee.Name);
      }
      return g;
    }

    private UIElement RenderParagraph(HTML.Element e,
                                      double fontScale,
                                      FontWeight fontWeight,
                                      FontFamily fontFamily,
                                      ScrollViewer scrollviewer)
    {
      TextBlock t = new TextBlock()
      {
        TextWrapping = TextWrapping.Wrap,
        Padding = new Thickness(2),
        HorizontalAlignment = HorizontalAlignment.Left,
      };
      ParagraphSizeTracker pst = new ParagraphSizeTracker() {
        Container = scrollviewer
      };
      t.SetBinding(TextBlock.MaxWidthProperty,
                   new Binding()
                   {
                     Source = pst,
                     Path = new PropertyPath("Width")
                   });
      LastCharacter = ' ';
      foreach (HTML.Node node in e.Contents) {
        Inline i = RenderInlines(node, fontScale, fontWeight, fontFamily, pst);
        if (i != null) {
          t.Inlines.Add(i);
        }
      }
      return t;
    }

    private char LastCharacter = ' ';

    private Inline RenderInlines(HTML.Node n, double fontScale, FontWeight fontWeight, FontFamily fontFamily, ParagraphSizeTracker pst)
    {
      HTML.Element e = n as HTML.Element;
      if (e != null) {
        Inline newInline;
        //Console.WriteLine("<{0}>", e.Name);
        switch (e.Name) {
          case "b":
          case "strong": newInline = new Span(); fontWeight = FontWeights.Bold; break;
          case "i":
          case "em": newInline = new Italic(); break;
          case "u": newInline = new Underline(); break;
          case "big": newInline = new Span(); fontScale *= 1.125; break;
          case "small": newInline = new Span(); fontScale /= 1.125; break;
          case "code":
          case "tt": newInline = new Span(); fontFamily = Monospace; break;
          case "sub": newInline = new Span(); newInline.Typography.Variants = FontVariants.Subscript; break;
          case "sup": newInline = new Span(); newInline.Typography.Variants = FontVariants.Superscript; break;
          case "a":
            newInline = RenderHyperlink(e);
            break;
          case "img":
            newInline = RenderImage(e, pst);
            break;
          // TODO map, anything else?
          default: newInline = new Span(); break;
        }
        if (newInline is Span) {
          foreach (HTML.Node node in e.Contents) {
            Inline i = RenderInlines(node, fontScale, fontWeight, fontFamily, pst);
            if (i != null) {
              (newInline as Span).Inlines.Add(i);
            }
          }
        }
        //Console.WriteLine("</{0}>", e.Name);
        return newInline;
      }
      else {
        //Console.WriteLine("text: [{0}]", ((HTML.Cdata)n).Content);
        StringBuilder sb = new StringBuilder();
        foreach (char c in ((HTML.Cdata)n).Content) {
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
        if (sb.Length > 0) {
          Run r = new Run(sb.ToString());
          r.FontSize *= fontScale;
          if (fontFamily != null) {
            r.FontFamily = fontFamily;
          }
          r.FontWeight = fontWeight;
          return r;
        }
        else {
          return null;
        }
      }
    }

    private Inline RenderHyperlink(HTML.Element e)
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

    private Inline RenderImage(HTML.Element e, ParagraphSizeTracker pst)
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

    private class ParagraphSizeTracker : INotifyPropertyChanged
    {
      public double Width
      {
        get {
          double m = 96; // minimum
          foreach (Image image in Images) {
            if (!double.IsNaN(image.Width) && image.Width > m) {
              m = image.Width;
            }
          }
          if (Container != null) {
            double cw = Container.ViewportWidth - 4;
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

      public ScrollViewer Container
      {
        get
        {
          return _Container;
        }
        set
        {
          if (_Container != null) {
            _Container.ScrollChanged -= ContainerScrollChanged;
          }
          if (value != null) {
            value.ScrollChanged += ContainerScrollChanged;
          }
          _Container = value;
        }
      }

      private ScrollViewer _Container = null;

      private void ContainerScrollChanged(object sender, ScrollChangedEventArgs e) {
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
