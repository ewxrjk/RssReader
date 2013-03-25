﻿using System;
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

namespace RssReader
{
  static public class RenderHTML
  {
    static public TextBlock Render(HTML.Document document)
    {
      return HTMLToTextBlock(document.HTML.Follow("body"));
    }

    static private TextBlock HTMLToTextBlock(HTML.Element root)
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
          case "ol":
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            int y = 0;
            foreach (HTML.Element ee in e.Contents) {
              //Console.WriteLine("<{0}>", ee.Name);
              g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
              TextBlock bullet = new TextBlock()
              {
                Text = (e.Name == "ul" ? "•" : string.Format("{0}.", y + 1)),
                Margin = new Thickness(1),
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
            t.Inlines.Add(g);
            t.Inlines.Add(new LineBreak());
            break;
          case "h1":
          case "h2":
          case "h3":
          case "h4":
          case "h5":
          case "h6":
            double fontScale = Math.Pow(1.125, '7' - e.Name[1]);
            Span s = new Span() { FontWeight = FontWeights.Bold };
            foreach (HTML.Node node in e.Contents) {
              s.Inlines.Add(ConvertToInline(node, fontScale));
            }
            t.Inlines.Add(s);
            if (i + 1 != root.Contents.Count) {
              t.Inlines.Add(new LineBreak());
            }
            break;
          default:
            // TODO <pre> should be monospaced
            // TODO <hN> should be emphasized somehow
            foreach (HTML.Node node in e.Contents) {
              t.Inlines.Add(ConvertToInline(node, 1));
            }
            if (i + 1 != root.Contents.Count) {
              t.Inlines.Add(new LineBreak());
            }
            break;
        }
        //Console.WriteLine("</{0}>", e.Name);
      }
      return t;
    }

    static private Inline ConvertToInline(HTML.Node n, double fontScale)
    {
      HTML.Element e = n as HTML.Element;
      if (e != null) {
        Inline newInline;
        //Console.WriteLine("<{0}>", e.Name);
        switch (e.Name) {
          case "b":
          case "strong": newInline = new Bold(); break;
          case "i":
          case "em": newInline = new Italic(); break;
          case "u": newInline = new Underline(); break;
          case "big": newInline = new Span(); fontScale *= 1.125; break;
          case "small": newInline = new Span(); fontScale /= 1.125; break;
          case "tt": newInline = new Span(); newInline.FontFamily = new System.Windows.Media.FontFamily("monospace"); break; // TODO and subelements...
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
              Image image = new Image() { Stretch = System.Windows.Media.Stretch.Fill };
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
            (newInline as Span).Inlines.Add(ConvertToInline(node, fontScale));
          }
        }
        //Console.WriteLine("</{0}>", e.Name);
        return newInline;
      }
      else {
        //Console.WriteLine("text: [{0}]", ((HTML.Cdata)n).Content);
        Run r = new Run(((HTML.Cdata)n).Content);
        r.FontSize *= fontScale;
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
