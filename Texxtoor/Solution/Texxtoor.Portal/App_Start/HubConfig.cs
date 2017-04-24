using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;


namespace Texxtoor.Portal {

  public class HubConfig {

    public static void RegisterHubs() {
      var hubConfiguration = new HubConfiguration {
        EnableDetailedErrors = true,
        EnableJavaScriptProxies = true
      };
    }

  }
}