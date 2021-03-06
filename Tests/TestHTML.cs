﻿// This file is part of HalfMoon RSS reader
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
      Assert.AreEqual(2, d.HTML.Contents.Count);
      HTML.Element body = d.HTML.Contents[1] as HTML.Element;
      Assert.IsNotNull(body);
      Assert.AreEqual("body", body.Name);
      Assert.AreEqual(0, body.Contents.Count);
    }

    [TestMethod]
    public void TestHTMLSimple()
    {
      HTML.Document d = HTML.Document.Parse("<p>wibble</p>");
      Assert.AreEqual("html", d.HTML.Name);
      Assert.AreEqual(2, d.HTML.Contents.Count);
      HTML.Element body = d.HTML.Contents[1] as HTML.Element;
      Assert.IsNotNull(body);
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
      Assert.AreEqual("one", (li1.Contents[0] as HTML.Cdata).Content);
      HTML.Element li2 = ul.Contents[1] as HTML.Element;
      Assert.AreEqual("li", li2.Name);
      Assert.AreEqual(1, li2.Contents.Count);
      Assert.AreEqual("two", (li2.Contents[0] as HTML.Cdata).Content);

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
      Assert.AreEqual("<html><head></head><body><p><img src=\"http://imgs.xkcd.com/comics/voyager_1.png\" title=\"what'ever'\"></p></body></html>", d.ToString());
    }

    [TestMethod]
    public void TestHTMLTable()
    {
      HTML.Document d = HTML.Document.Parse("<table><tr><td>a</td><td>b</td><tr><td><p>c</td>");
      Assert.AreEqual("<html><head></head><body><table><tr><td>a</td><td>b</td></tr><tr><td><p>c</p></td></tr></table></body></html>", d.ToString());
      d = HTML.Document.Parse("<table><tr><td>a<caption>c</table>");
      Assert.AreEqual("<html><head></head><body><table><tr><td>a</td></tr><caption>c</caption></table></body></html>", d.ToString());
    }

    [TestMethod]
    public void TestHTMLSpaceTable()
    {
      HTML.Document d = HTML.Document.Parse("<table>\n <tr>\n  <td>a</td>\n  <td>b</td>\n <tr>\n  <td>c</td>\n</table>\n");
      Assert.AreEqual("<html><head></head><body><table><tr><td>a</td><td>b</td></tr><tr><td>c</td></tr></table></body></html>", d.ToString());
    }

    [TestMethod]
    public void TestHTMLComment()
    {
      HTML.Document d = HTML.Document.Parse("<!--comment-->spong");
      Assert.AreEqual("<html><head></head><body><p>spong</p></body></html>", d.ToString());
      d = HTML.Document.Parse("this and <!--comment--> that");
      Assert.AreEqual("<html><head></head><body><p>this and  that</p></body></html>", d.ToString());
    }

    [TestMethod]
    public void TestHTMLHead()
    {
      HTML.Document d = HTML.Document.Parse("<head><title>T<body>B");
      Assert.AreEqual("<html><head><title>T</title></head><body><p>B</p></body></html>", d.ToString());
      d = HTML.Document.Parse("<head><link rel='foo' href='bar'>");
      Assert.AreEqual("<html><head><link href=\"bar\" rel=\"foo\"></head><body></body></html>", d.ToString());
    }

    [TestMethod]
    public void TestHTMLBOM()
    {
      HTML.Document d = HTML.Document.Parse("\uFEFFB");
      Assert.AreEqual("<html><head></head><body><p>B</p></body></html>", d.ToString());
    }

    [TestMethod]
    public void TestHTMLScript()
    {
      HTML.Document d = HTML.Document.Parse("<head><script><T</script>");
      Assert.AreEqual("<html><head><script><T</script></head><body></body></html>", d.ToString());
      d = HTML.Document.Parse("<head><style><T</style>");
      Assert.AreEqual("<html><head><style><T</style></head><body></body></html>", d.ToString());
    }

    [TestMethod]
    public void TestHTMLXml()
    {
      HTML.Document d = HTML.Document.Parse("<meta http-equiv=\"something\" /><link rel=whatever /><link rel=alternate />");
      Assert.AreEqual("<html><head><meta http-equiv=\"something\"><link rel=\"whatever\"><link rel=\"alternate\"></head><body></body></html>", d.ToString());
      d = HTML.Document.Parse("<html a=a xml:lang=\"en-US\" z=z>T");
      Assert.AreEqual("<html a=\"a\" xml:lang=\"en-US\" z=\"z\"><head></head><body><p>T</p></body></html>", d.ToString());
    }

    [TestMethod]
    public void TestHTMLExplicitClose()
    {
      HTML.Document d = HTML.Document.Parse("<html><head><title>T</title></head><body>B</body></html>");
      Assert.AreEqual("<html><head><title>T</title></head><body><p>B</p></body></html>", d.ToString());
    }

    [TestMethod]
    public void TestHTMLBlockQuote()
    {
      HTML.Document d = HTML.Document.Parse("<blockquote><p>T</blockquote>");
      Assert.AreEqual("<html><head></head><body><blockquote><p>T</p></blockquote></body></html>", d.ToString());
      d = HTML.Document.Parse("<blockquote>T</blockquote>");
      Assert.AreEqual("<html><head></head><body><blockquote>T</blockquote></body></html>", d.ToString());
    }

    [TestMethod]
    public void TestHTMLFlowContext()
    {
      HTML.Document d = HTML.Document.Parse("<blockquote>A<p>B</blockquote>");
      Assert.AreEqual("<html><head></head><body><blockquote><p>A</p><p>B</p></blockquote></body></html>", d.ToString());
      d = HTML.Document.Parse("<blockquote>A</pre><p>B</blockquote>");
      Assert.AreEqual("<html><head></head><body><blockquote><pre>A</pre><p>B</p></blockquote></body></html>", d.ToString());
    }

    [TestMethod]
    public void TestHTMLDefinitionList()
    {
      HTML.Document d = HTML.Document.Parse("<dl><dd>a<dt>b</dl>");
      Assert.AreEqual("<html><head></head><body><dl><dd>a</dd><dt>b</dt></dl></body></html>", d.ToString());
      d = HTML.Document.Parse("<dl><dd>a<dt><p>b</dl>");
      Assert.AreEqual("<html><head></head><body><dl><dd>a</dd><dt><p>b</p></dt></dl></body></html>", d.ToString());
    }

  }
}
