using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;

namespace Texxtoor.Portal.Core.Attributes {
  public class AuthorizeByRole : AuthorizeAttribute {
      public string RedirectActionName { get; set; }
      public string RedirectControllerName { get; set; }
      private  UserManager _userRepository;

      protected override bool AuthorizeCore(HttpContextBase httpContext) {
        var user = httpContext.User;
        _userRepository = UserManager.Instance;
        var accessAllowed = false;

        // Get the roles passed in with the (Roles = "...") on the attribute
        var allowedRoles = Roles.Split(',');

        if (!user.Identity.IsAuthenticated) {
          return false;
        }

        // Get roles for current user
        var roles = _userRepository.GetRolesForUser(user.Identity);

        accessAllowed = allowedRoles.Any(roles.Contains);

        return accessAllowed;
      }

      public override void OnAuthorization(AuthorizationContext filterContext) {
        base.OnAuthorization(filterContext);

        if (filterContext.HttpContext.User.Identity.IsAuthenticated && filterContext.Result is HttpUnauthorizedResult) {
          var values = new RouteValueDictionary(new {
            action = RedirectActionName == string.Empty ? "AccessDenied" : RedirectActionName,
            controller = RedirectControllerName == string.Empty ? "Home" : RedirectControllerName,
            area = ""
          });

          filterContext.Result = new RedirectToRouteResult(values);
        }
      }
    
  }
}