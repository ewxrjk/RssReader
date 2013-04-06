using System;

namespace RssReader
{
  public class Performable
  {
    public string Description;

    public Action Undo;

    public Action Redo;
  }
}
