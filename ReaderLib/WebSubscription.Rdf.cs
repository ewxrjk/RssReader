using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;

namespace ReaderLib
{
  public partial class WebSubscription
  {

    private void UpdateFromRdf(XElement rdf, Action<Action> dispatch, Action<Exception> error)
    {
      // TODO
    }

  }
}
