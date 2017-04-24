using System.Linq;
using System.Globalization;
using System.Web.Http;
using System.Web.Mvc;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Context;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.BaseLibrary;

namespace Texxtoor.Portal.Core.Extensions {

  public class ApiControllerExt : ApiController {

    public string UserName { get { return User.Identity.Name; } }

    public string CurrentCulture {
      get {
        var cult = System.Threading.Thread.CurrentThread.CurrentUICulture;
        return Equals(cult.Parent, CultureInfo.InvariantCulture)
                 ? cult.TwoLetterISOLanguageName : cult.Parent.TwoLetterISOLanguageName;
      }
    }

  }
}