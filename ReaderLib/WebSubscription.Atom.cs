﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ReaderLib
{
  public partial class WebSubscription
  {
    static private XNamespace Atom = "http://www.w3.org/2005/Atom";

    private void UpdateFromAtom(XElement atom, Action<Action> dispatch, Action<Exception> error)
    {
      XElement title = atom.Element(Atom + "title");
      string publicUri = GetLinkAtom(atom);
      IEnumerable<XElement> items = atom.Elements(Atom + "entry").Reverse();
      dispatch(() =>
      {
        try {
          Title = title != null ? (string)title : "";
          PublicURI = publicUri;
          foreach (XElement item in items) {
            UpdateEntry(item, HashId((string)item.Element(Atom + "id")), UpdateFromAtom, error);
          }
          Type = "Atom";
          Error = null;
        }
        catch (SubscriptionException se) {
          if (se.Subscription == null) {
            se.Subscription = this;
          }
          error(se);
          Error = se;
        }
        catch (Exception e) {
          error(e);
          Error = e;
        }
      });
    }

    private void UpdateFromAtom(WebEntry entry, XElement element, Action<Exception> error)
    {
      entry.URI = GetLinkAtom(element);
      entry.Title = (string)element.Element(Atom + "title"); // TODO the title might be HTML.
      string updated = (string)GetMandatoryElement(element, Atom + "updated");
      try {
        entry.Date = Tools.RFC3339Date(updated);
      }
      catch (Exception e) {
        error(new SubscriptionParsingException(string.Format("Invalid date string “{0}”", updated), e) { Subscription = this });
      }
      XElement content = element.Element(Atom + "content");
      if (content != null) {
        XAttribute src = content.Attribute("src");
        if (src != null) {
          if (entry.Description == null) {
            entry.Description = "(awaiting remote content)";
            // TODO fetch content from somewhere else on demand
          }
        }
        else {
          // TODO content might not be HTML
          entry.Description = (string)content;
        }
      }
      else {
        XElement summary = element.Element(Atom + "summary");
        if (summary != null) {
          entry.Description = (string)summary;
        }
        else {
          entry.Description = null;
        }
      }
    }

    private string GetLinkAtom(XElement item)
    {
      foreach (XElement link in item.Elements(Atom + "link")) {
        if (link.Attribute("href") != null) {
          XAttribute attribute = link.Attribute("rel");
          string rel = (attribute != null ? attribute.Value : "");
          if (rel == "alternate" || rel == "") {
            return link.Attribute("href").Value;
          }
        }
      }
      return "";
    }
  }
}
