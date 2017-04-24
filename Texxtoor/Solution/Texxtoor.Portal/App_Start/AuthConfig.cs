using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
 
namespace Texxtoor.Portal.App_Start {
  public static class AuthConfig {
    public static void RegisterAuth(IAppBuilder app) { 
      // Enable the application to use a cookie to store information for the signed in user
      app.UseCookieAuthentication(new CookieAuthenticationOptions {
        AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
        LoginPath = new PathString("/Account/Logon")
      });
      // Use a cookie to temporarily store information about a user logging in with a third party login provider
      app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

      app.UseMicrosoftAccountAuthentication(
          clientId: WebConfigurationManager.AppSettings["social:LiveClientId"],
          clientSecret: WebConfigurationManager.AppSettings["social:LiveClientSecret"]);

      app.UseTwitterAuthentication(
          consumerKey: WebConfigurationManager.AppSettings["social:TwitterConsumerKey"],
          consumerSecret: WebConfigurationManager.AppSettings["social:TwitterConsumerSecret"]);

      app.UseFacebookAuthentication(
          appId: WebConfigurationManager.AppSettings["social:FaceBookAppId"],
          appSecret: WebConfigurationManager.AppSettings["social:FacebookAppSecret"]);
      
      //TODO: Deactivated due to Google no longer support OAuth2
      //app.UseGoogleAuthentication();
    }
  }
}