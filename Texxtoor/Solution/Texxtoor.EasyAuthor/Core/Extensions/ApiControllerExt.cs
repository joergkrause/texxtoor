using System.Globalization;
using System.Web.Http;

namespace Texxtoor.EasyAuthor.Core.Extensions {

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