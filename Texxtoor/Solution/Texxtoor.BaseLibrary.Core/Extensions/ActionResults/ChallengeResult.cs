using System.Web;
using Microsoft.Owin.Security;
using System.Web.Mvc;

namespace Texxtoor.BaseLibrary.Core.Extensions.ActionResults {
  public class ChallengeResult : HttpUnauthorizedResult {

    public const string XsrfKey = "XsrfId";

    public ChallengeResult(string provider, string redirectUri)
      : this(provider, redirectUri, null) {
    }

    public ChallengeResult(string provider, string redirectUri, string userId) {
      LoginProvider = provider;
      RedirectUri = redirectUri;
      UserId = userId;
    }

    public string LoginProvider { get; set; }
    public string RedirectUri { get; set; }
    public string UserId { get; set; }

    public override void ExecuteResult(ControllerContext context) {
      var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
      if (UserId != null) {
        properties.Dictionary[XsrfKey] = UserId;
      }
      context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
    }
  }
}
