using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Texxtoor.Portal.App_Start;

[assembly: OwinStartup(typeof(Texxtoor.Portal.Startup))]

namespace Texxtoor.Portal {
  public class Startup {
    public void Configuration(IAppBuilder app) {
      //app.MapSignalR();
      AuthConfig.RegisterAuth(app);
    }
  }
}
