using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RssReader
{
  public class Performable
  {
    public string Description;

    public Action Undo;

    public Action Redo;
  }
}
