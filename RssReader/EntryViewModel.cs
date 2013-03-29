using System;
using ReaderLib;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using HTML = ReaderLib.HTML;
using System.Windows.Documents;
using System.Windows.Data;
using System.Windows.Media;

namespace RssReader
{
  /// <summary>
  /// View model for an entry
  /// </summary>
  public class EntryViewModel : INotifyPropertyChanged
  {
    public EntryViewModel(Entry entry)
    {
      _Entry = entry;
      _Entry.PropertyChanged += ModelPropertyChanged;
    }

    private Entry _Entry;

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName) {
        case "Date":
        case "Title":
        case "URI":
          OnPropertyChanged(e.PropertyName);
          break;
        case "Description":
          OnPropertyChanged("HtmlDescription");
          OnPropertyChanged(e.PropertyName);
          break;
        case "Read":
          OnPropertyChanged("TitleWeight");
          OnPropertyChanged(e.PropertyName);
          break;
      }
    }

    #region Public properties

    public string Title
    {
      get
      {
        return _Entry.Title;
      }
    }

    public string Description
    {
      get
      {
        return _Entry.Description;
      }
    }

    public HTML.Document HtmlDescription
    {
      get
      {
        return HTML.Document.Parse(Description);
      }
    }

    public bool Read
    {
      get
      {
        return _Entry.Read;
      }
      set
      {
        _Entry.Read = value;
      }
    }

    public FontWeight TitleWeight
    {
      get
      {
        return _Entry.Read ? FontWeights.Normal : FontWeights.Bold;
      }
    }

    public Uri URI
    {
      get
      {
        WebEntry e = _Entry as WebEntry;
        Uri result;
        return e != null && Uri.TryCreate(e.URI, UriKind.Absolute, out result) ? result : null;
      }
    }

    public string Date
    {
      get
      {
        WebEntry e = _Entry as WebEntry;
        if (e != null && e.Date != null) {
          DateTime now = DateTime.Now;
          if (now.Date == e.Date.Date) {
            return e.Date.ToString("HH:mm:ss: ");
          }
          else {
            return e.Date.ToString("yyyy-MM-dd: ");
          }
        }
        else {
          return "";
        }
      }
    }

    #endregion

    static readonly FontFamily HeadingFontFamily = new FontFamily("Global Sans Serif");

    private Inline HeadingDate()
    {
      Run r = new Run();
      r.SetBinding(Run.TextProperty, new Binding()
      {
        Source = this,
        Path = new PropertyPath("Date"),
        Mode = BindingMode.OneWay,
      });
      return r;

    }

    private Inline HeadingTitle()
    {
      Run r = new Run();
      r.SetBinding(Run.TextProperty, new Binding()
      {
        Source = this,
        Path = new PropertyPath("Title"),
        Mode = BindingMode.OneWay,
      });
      return r;
    }

    private Hyperlink HeadingHyperlink()
    {
      // TODO ugh.  can't we use xaml for this?
      Hyperlink l = new Hyperlink()
      {
        FontFamily = HeadingFontFamily,
        FontWeight = FontWeights.Bold,
      };
      l.FontSize *= 1.5;
      l.SetBinding(Hyperlink.NavigateUriProperty, new Binding()
      {
        Source = this,
        Path = new PropertyPath("URI"),
        Mode = BindingMode.OneWay,
      });
      l.SetBinding(Hyperlink.ToolTipProperty, new Binding()
      {
        Source = this,
        Path = new PropertyPath("URI"),
        Mode = BindingMode.OneWay,
      });
      l.RequestNavigate += RequestedNavigate;
      l.Inlines.Add(HeadingDate());
      l.Inlines.Add(HeadingTitle());
      return l;
    }

    public FrameworkElement Rendered(ScrollViewer scrollViewer)
    {
      StackPanel sp = new StackPanel();
      TextBlock header = new TextBlock();
      header.Inlines.Add(HeadingHyperlink());
      sp.Children.Add(header);
      sp.Children.Add(RenderHTML.Render(HtmlDescription, scrollViewer));
      return sp;
    }
    
    // TODO some duplication here...
    private void RequestedNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Uri.ToString());
      e.Handled = true;
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
