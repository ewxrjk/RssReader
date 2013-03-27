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

namespace RssReader
{
  static public class RenderHTML
  {
    static readonly FontFamily Monospace = new FontFamily("Global Monospace");

    static public TextBlock Render(HTML.Document document)
    {
      return HTMLToTextBlock(document.HTML.Follow("body"));
    }

    static private TextBlock HTMLToTextBlock(HTML.Element root,
                                             Binding widthBinding = null)
    {
      TextBlock t = new TextBlock()
      {
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(1),
      };
      for (int i = 0; i < root.Contents.Count; ++i) {
        HTML.Element e = root.Contents[i] as HTML.Element;
        //Console.WriteLine("<{0}>", e.Name);
        switch (e.Name) {
          case "ul":
            ListContainer(e, t.Inlines, y => "•");
            t.Inlines.Add(new LineBreak());
            break;
          case "ol":
            ListContainer(e, t.Inlines, y => string.Format("{0}.", y + 1));
            t.Inlines.Add(new LineBreak());
            break;
          case "h1":
          case "h2":
          case "h3":
          case "h4":
          case "h5":
          case "h6":
            TextContainer(e, t.Inlines, Math.Pow(1.125, '7' - e.Name[1]), FontWeights.Bold, null);
            if (i + 1 != root.Contents.Count) {
              t.Inlines.Add(new LineBreak());
            }
            break;
          case "pre":
            TextContainer(e, t.Inlines, 1, FontWeights.Normal, Monospace);
            if (i + 1 != root.Contents.Count) {
              t.Inlines.Add(new LineBreak());
            }
            break;
          case "table":
            Table(e, t.Inlines);
            t.Inlines.Add(new LineBreak());
            break;
          default:
            TextContainer(e, t.Inlines, 1, FontWeights.Normal, null);
            if (i + 1 != root.Contents.Count) {
              t.Inlines.Add(new LineBreak());
            }
            break;
        }
        //Console.WriteLine("</{0}>", e.Name);
      }
      return t;
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

    static private void Table(HTML.Element e, InlineCollection inlines)
    {
      Grid g = new Grid();
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
            TextBlock content = HTMLToTextBlock(c);
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
      inlines.Add(g);
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

    static private void ListContainer(HTML.Element e, InlineCollection inlines, Func<int,string> makeBullet)
    {
      Grid g = new Grid();
      g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
      g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
      int y = 0;
      foreach (HTML.Element ee in e.Contents) {
        //Console.WriteLine("<{0}>", ee.Name);
        g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        TextBlock bullet = new TextBlock()
        {
          Text = makeBullet(y),
          Margin = new Thickness(1),
          HorizontalAlignment = HorizontalAlignment.Right,
        };
        Grid.SetColumn(bullet, 0);
        Grid.SetRow(bullet, y);
        g.Children.Add(bullet);
        TextBlock content = HTMLToTextBlock(ee);
        Grid.SetColumn(content, 1);
        Grid.SetRow(content, y);
        g.Children.Add(content);
        ++y;
        //Console.WriteLine("</{0}>", ee.Name);
      }
      inlines.Add(g);
    }

    static private void TextContainer(HTML.Element e, InlineCollection inlines, double fontScale, FontWeight fontWeight, FontFamily fontFamily)
    {
      foreach (HTML.Node node in e.Contents) {
        inlines.Add(ConvertToInline(node, fontScale, fontWeight, fontFamily));
      }
    }

    static private Inline ConvertToInline(HTML.Node n, double fontScale, FontWeight fontWeight, FontFamily fontFamily)
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
            Hyperlink h = new Hyperlink();
            if (e.Attributes.ContainsKey("href")) {
              Uri TargetURI;
              if (Uri.TryCreate(e.Attributes["href"], UriKind.RelativeOrAbsolute, out TargetURI)) {
                h.NavigateUri = TargetURI;
                h.RequestNavigate += RequestedNavigate;
              }
              h.ToolTip = e.Attributes["href"]; // tooltip contains URI even if it's bad
            }
            newInline = h;
            break;
          case "img":
            Uri ImageURI;
            if (e.Attributes.ContainsKey("src")
                && Uri.TryCreate(e.Attributes["src"], UriKind.RelativeOrAbsolute, out ImageURI)) {
              Image image = new Image() { Stretch = Stretch.Fill };
              BitmapImage bitmapImage = new BitmapImage();
              bitmapImage.BeginInit();
              bitmapImage.UriSource = ImageURI;
              bitmapImage.EndInit();
              image.Source = bitmapImage;
              // This hack forces image size to match pixel size,
              // which undermines WPF's preference for honoring the
              // embedded DPI value which leads to upscaled and
              // mildly blurry images where that doesn't match
              // the local display.  Very high resolution displays
              // might need another approach.
              //
              // It seems to be necessary to have Source=image and
              // use a two-component path, rather than having
              // Source=bitmapImage.  TODO understand why.
              image.SetBinding(Image.WidthProperty, new Binding()
              {
                Path = new PropertyPath("Source.PixelWidth"),
                Source = image,
              });
              image.SetBinding(Image.HeightProperty, new Binding()
              {
                Path = new PropertyPath("Source.PixelHeight"),
                Source = image,
              });
              if (e.Attributes.ContainsKey("title")) {
                image.ToolTip = e.Attributes["title"];
              }
              else if (e.Attributes.ContainsKey("alt")) {
                image.ToolTip = e.Attributes["alt"];
              }
              // TODO arrange to display alt value if image can't be loaded
              // (or before it has loaded)
              newInline = new InlineUIContainer() { Child = image };
            }
            else {
              newInline = new Span(); // bit of a hack
            }
            break;
          // TODO map, anything else?
          default: newInline = new Span(); break;
        }
        if (newInline is Span) {
          foreach (HTML.Node node in e.Contents) {
            (newInline as Span).Inlines.Add(ConvertToInline(node, fontScale, fontWeight, fontFamily));
          }
        }
        //Console.WriteLine("</{0}>", e.Name);
        return newInline;
      }
      else {
        //Console.WriteLine("text: [{0}]", ((HTML.Cdata)n).Content);
        Run r = new Run(((HTML.Cdata)n).Content);
        r.FontSize *= fontScale;
        if(fontFamily != null) {
          r.FontFamily = fontFamily;
        }
        r.FontWeight = fontWeight;
        return r;
      }
    }

    static private void RequestedNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Uri.ToString());
      e.Handled = true;
    }

  }
}
