using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReaderLib;
using HTML = ReaderLib.HTML;

namespace Tests
{
  [TestClass]
  public class TestHTML
  {
    [TestMethod]
    public void TestHTMLEmpty()
    {
      HTML.Document d = HTML.Document.Parse("");
      Assert.AreEqual("html", d.HTML.Name);
      Assert.AreEqual(1, d.HTML.Contents.Count);
      HTML.Element body = d.HTML.Contents[0] as HTML.Element;
      Assert.AreNotEqual(null, body);
      Assert.AreEqual("body", body.Name);
      Assert.AreEqual(0, body.Contents.Count);
    }

    [TestMethod]
    public void TestHTMLSimple()
    {
      HTML.Document d = HTML.Document.Parse("<p>wibble</p>");
      Assert.AreEqual("html", d.HTML.Name);
      Assert.AreEqual(1, d.HTML.Contents.Count);
      HTML.Element body = d.HTML.Contents[0] as HTML.Element;
      Assert.AreNotEqual(null, body);
      Assert.AreEqual("body", body.Name);
      Assert.AreEqual(1, body.Contents.Count);
      HTML.Element p = body.Contents[0] as HTML.Element;
      Assert.AreEqual("p", p.Name);
      Assert.AreEqual(1, p.Contents.Count);
      HTML.Cdata cdata = p.Contents[0] as HTML.Cdata;
      Assert.AreEqual("wibble", cdata.Content);
    }

    [TestMethod]
    public void TestHTMLSimpleNoClose()
    {
      HTML.Document d = HTML.Document.Parse("<p>wibble");
      HTML.Element p = d.HTML.Follow("body.p");
      Assert.AreEqual(1, p.Contents.Count);
      HTML.Cdata cdata = p.Contents[0] as HTML.Cdata;
      Assert.AreEqual("wibble", cdata.Content);
    }

    [TestMethod]
    public void TestHTMLSimpleNoOpen()
    {
      HTML.Document d = HTML.Document.Parse("wibble");
      HTML.Element p = d.HTML.Follow("body.p");
      Assert.AreEqual(1, p.Contents.Count);
      HTML.Cdata cdata = p.Contents[0] as HTML.Cdata;
      Assert.AreEqual("wibble", cdata.Content);
    }

    [TestMethod]
    public void TestHTMLTwoPara()
    {
      HTML.Document d = HTML.Document.Parse("<p>first</p> <p>second</p>");
      Assert.AreEqual(2, d.HTML.Follow("body").Contents.Count);
      HTML.Element p1 = d.HTML.Follow("body.p");
      Assert.AreEqual(1, p1.Contents.Count);
      HTML.Cdata cdata1 = p1.Contents[0] as HTML.Cdata;
      Assert.AreEqual("first", cdata1.Content);
      HTML.Element p2 = (HTML.Element)d.HTML.Follow("body").Contents[1];
      HTML.Cdata cdata2 = p2.Contents[0] as HTML.Cdata;
      Assert.AreEqual("second", cdata2.Content);
    }

    [TestMethod]
    public void TestHTMLInline()
    {
      HTML.Document d = HTML.Document.Parse("<p>one <i>two</i> three</p>");
      HTML.Element p = d.HTML.Follow("body.p");
      Assert.AreEqual(3, p.Contents.Count);
      HTML.Cdata cdata1 = p.Contents[0] as HTML.Cdata;
      Assert.AreEqual("one ", cdata1.Content);
      HTML.Element italic = p.Contents[1] as HTML.Element;
      Assert.AreEqual("i", italic.Name);
      Assert.AreEqual(1, italic.Contents.Count);
      HTML.Cdata cdata2 = italic.Contents[0] as HTML.Cdata;
      Assert.AreEqual("two", cdata2.Content);
      HTML.Cdata cdata3 = p.Contents[2] as HTML.Cdata;
      Assert.AreEqual(" three", cdata3.Content);
    }

    [TestMethod]
    public void TestHTMLInlineNesting()
    {
      HTML.Document d = HTML.Document.Parse("<p>one <i>two <b>three</i> four</p>");
      HTML.Element p = d.HTML.Follow("body.p");
      Assert.AreEqual(3, p.Contents.Count);
      HTML.Cdata cdata1 = p.Contents[0] as HTML.Cdata;
      Assert.AreEqual("one ", cdata1.Content);
      HTML.Element italic = p.Contents[1] as HTML.Element;
      Assert.AreEqual("i", italic.Name);
      Assert.AreEqual(2, italic.Contents.Count);
      HTML.Cdata cdata2 = italic.Contents[0] as HTML.Cdata;
      Assert.AreEqual("two ", cdata2.Content);
      HTML.Element bold = italic.Contents[1] as HTML.Element;
      Assert.AreEqual(1, bold.Contents.Count);
      HTML.Cdata cdata3 = bold.Contents[0] as HTML.Cdata;
      Assert.AreEqual("three", cdata3.Content);
      HTML.Cdata cdata4 = p.Contents[2] as HTML.Cdata;
      Assert.AreEqual(" four", cdata4.Content);
    }

    [TestMethod]
    public void TestHTMLLists()
    {
      HTML.Document d = HTML.Document.Parse("<ul><li>one</li><li>two</li></ul>");
      HTML.Element ul = d.HTML.Follow("body.ul");
      Assert.AreEqual(2, ul.Contents.Count);
      HTML.Element li1 = ul.Contents[0] as HTML.Element;
      Assert.AreEqual("li", li1.Name);
      Assert.AreEqual(1, li1.Contents.Count);
      HTML.Element p1 = li1.Contents[0] as HTML.Element;
      Assert.AreEqual("one", (p1.Contents[0] as HTML.Cdata).Content);
      HTML.Element li2 = ul.Contents[1] as HTML.Element;
      Assert.AreEqual("li", li2.Name);
      Assert.AreEqual(1, li2.Contents.Count);
      HTML.Element p2 = li2.Contents[0] as HTML.Element;
      Assert.AreEqual("two", (p2.Contents[0] as HTML.Cdata).Content);

      d = HTML.Document.Parse("<ul><li><p>one<p>two</ul>");
      ul = d.HTML.Follow("body.ul");
      Assert.AreEqual(1, ul.Contents.Count);
      li1 = ul.Contents[0] as HTML.Element;
      Assert.AreEqual("li", li1.Name);
      Assert.AreEqual(2, li1.Contents.Count);
      Assert.AreEqual("one", ((li1.Contents[0] as HTML.Element).Contents[0] as HTML.Cdata).Content);
      Assert.AreEqual("two", ((li1.Contents[1] as HTML.Element).Contents[0] as HTML.Cdata).Content);
    }

    [TestMethod]
    public void TestHTMLEntities()
    {
      HTML.Document d = HTML.Document.Parse("<p>&ldquo;&amp;&#255;&#xA1;&rdquo;</p>");
      Assert.AreEqual("“&\xFF\xA1”", (d.HTML.Follow("body.p").Contents[0] as HTML.Cdata).Content);

      d = HTML.Document.Parse("<p>&ldquo &amp&#255&#xA1&rdquo</p>");
      Assert.AreEqual("“ &\xFF\xA1”", (d.HTML.Follow("body.p").Contents[0] as HTML.Cdata).Content);

      d = HTML.Document.Parse("<p>&#x10000;</p>");
      Assert.AreEqual("\U00010000", (d.HTML.Follow("body.p").Contents[0] as HTML.Cdata).Content);

      d = HTML.Document.Parse("<p>&#xD800;&#xDFFF;</p>"); // surrogates
      Assert.AreEqual("&#xD800;&#xDFFF;", (d.HTML.Follow("body.p").Contents[0] as HTML.Cdata).Content);

      d = HTML.Document.Parse("<p>&#x110000;</p>"); // out of range
      Assert.AreEqual("&#x110000;", (d.HTML.Follow("body.p").Contents[0] as HTML.Cdata).Content);
    }

    [TestMethod]
    public void TestHTMLAttributes()
    {
      HTML.Document d = HTML.Document.Parse("<p><a href=\"http://www.example.com/\">target</a></p>");
      HTML.Element a = d.HTML.Follow("body.p.a");
      Assert.AreEqual(1, a.Attributes.Count);
      Assert.IsTrue(a.Attributes.ContainsKey("href"));
      Assert.AreEqual("http://www.example.com/", a.Attributes["href"]);

      d = HTML.Document.Parse("<p><a href='http://www.example.com/&amp;' >target</a></p>");
      a = d.HTML.Follow("body.p.a");
      Assert.AreEqual(1, a.Attributes.Count);
      Assert.IsTrue(a.Attributes.ContainsKey("href"));
      Assert.AreEqual("http://www.example.com/&", a.Attributes["href"]);

      d = HTML.Document.Parse("<p><a href=\"http://www.example.com/\" Target=_blank>target</a></p>");
      a = d.HTML.Follow("body.p.a");
      Assert.AreEqual(2, a.Attributes.Count);
      Assert.IsTrue(a.Attributes.ContainsKey("href"));
      Assert.AreEqual("http://www.example.com/", a.Attributes["href"]);
      Assert.IsTrue(a.Attributes.ContainsKey("target"));
      Assert.AreEqual("_blank", a.Attributes["target"]);
    }

    [TestMethod]
    public void TestHTMLExtraSlash()
    {
      HTML.Document d = HTML.Document.Parse("<img src=\"http://imgs.xkcd.com/comics/voyager_1.png\" title=\"what'ever'\" />");
      HTML.Element img = d.HTML.Follow("body.p.img");
      Assert.AreEqual(2, img.Attributes.Count);
      Assert.IsTrue(img.Attributes.ContainsKey("src"));
      Assert.AreEqual("http://imgs.xkcd.com/comics/voyager_1.png", img.Attributes["src"]);
      Assert.IsTrue(img.Attributes.ContainsKey("title"));
      Assert.AreEqual("what'ever'", img.Attributes["title"]);
      Assert.AreEqual("<html><body><p><img src=\"http://imgs.xkcd.com/comics/voyager_1.png\" title=\"what'ever'\"></p></body></html>", d.ToString());
    }
  }
}
