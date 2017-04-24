using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Marketing;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Common;
using System.Web.Configuration;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Web.Script.Serialization;

namespace Texxtoor.Portal.Controllers {

  public class CommunityController : ControllerExt {

    [Authorize]
    public ActionResult EditProfile() {
      var model = Manager<UserProfileManager>.Instance.GetProfileByUser(UserName);
      return View(model);
    }

    [Authorize]
    [NavigationPathFilter("Show Profile")]
    public ActionResult ShowProfile(int id) {
      var model = Manager<UserProfileManager>.Instance.GetProfile(id);
      return View(model);
    }

    [HttpPost]
    public JsonResult SetPicture(int id, HttpPostedFileBase file) {
      if (file == null) {
        var g = UnitOfWork<UserProfileManager>().RemoveProfileImage(id, UserName);
        // Return JSON    
        return Json( new { msg = "Removed", src = String.Format("/Tools/GetImg/{0}?c=Profile&res=125x155&nc=true&t=", 0, DateTime.Now.Ticks) } );
      }
      else {
        UnitOfWork<UserProfileManager>().SaveProfileImage(id, file, UserName);
        // Return JSON             
        return Json(new { msg = "Picture Saved", src = String.Format("/Tools/GetImg/{0}?c=Profile&res=125x155&nc=true&t=", id, DateTime.Now.Ticks) } );
      }
    }


    [ValidateInput(false)]
    [HttpPost]
    public JsonResult SaveProfile(int id, FormCollection form) {
      try {
        var content = form["content"];
        UnitOfWork<UserProfileManager>().SaveProfileWalltext(id, Server.HtmlDecode(content), UserName);
        return Json(new { msg = ControllerResources.CommunityController_SaveProfile_Profil_gespeichert });
      } catch (Exception ex) {
        return Json(new { msg = ControllerResources.CommunityController_SaveProfile_Fehler_ + ex.Message });
      }
    }

    public ActionResult Users() {
      return View();
    }

    public ActionResult ListUsers(PaginationFormModel p) {
      var authors = UnitOfWork<UserManager>().GetUsersByRole(UserRole.Author);
      var contrib = UnitOfWork<UserManager>().GetUsersByRole(UserRole.Contributor);
      var users = authors
        .Union(contrib)
        .Distinct()
        .Where(u => !u.IsLockedOut)
        .AsQueryable();
      return PartialView("Users/_ListUsers", users.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult AddMessageToUser(int id) {
      ViewBag.Receivers = id;
      var msg = new Message {
        Sender = Manager<UserManager>.Instance.GetUserByName(UserName),
        Subject = "",
        Body = ""
      };
      return PartialView("Users/_AddMessage", msg);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="receivers">Comma separated list of id</param>
    /// <param name="msg"></param>
    /// <returns></returns>
    [HttpPost, ValidateInput(false)]
    public JsonResult SendMessage(int[] receivers, Message msg) {
      UnitOfWork<UserManager>().AddMessage(receivers, msg, UserName);
      return Json(new { msg = ControllerResources.MessagingController_AddMessage_Successfully_send__ });
    }

    public ContentResult GetTwitter() {
      // You need to set your own keys and screen name
      var oAuthConsumerKey = WebConfigurationManager.AppSettings["social:TwitterConsumerKey"];
      var oAuthConsumerSecret = WebConfigurationManager.AppSettings["social:TwitterConsumerSecret"];
      var oAuthUrl = "https://api.twitter.com/oauth2/token";
      var screenname = WebConfigurationManager.AppSettings["social:TwitterName"];

      // Do the Authenticate
      var authHeaderFormat = "Basic {0}";

      var authHeader = String.Format(authHeaderFormat,
          Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(oAuthConsumerKey) + ":" +
          Uri.EscapeDataString((oAuthConsumerSecret)))
      ));

      var postBody = "grant_type=client_credentials";

      var authRequest = (HttpWebRequest)WebRequest.Create(oAuthUrl);
      authRequest.Headers.Add("Authorization", authHeader);
      authRequest.Method = "POST";
      authRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
      authRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

      using (Stream stream = authRequest.GetRequestStream()) {
        byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
        stream.Write(content, 0, content.Length);
      }

      authRequest.Headers.Add("Accept-Encoding", "gzip");

      WebResponse authResponse = authRequest.GetResponse();
      // deserialize into an object
      dynamic twitAuthResponse;
      using (authResponse) {
        using (var reader = new StreamReader(authResponse.GetResponseStream())) {
          var objectText = reader.ReadToEnd();
          twitAuthResponse = JsonConvert.DeserializeObject(objectText);
        }
      }

      // Do the timeline
      var timelineFormat = "https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name={0}&include_rts=1&exclude_replies=1&count=5";
      var timelineUrl = string.Format(timelineFormat, screenname);
      HttpWebRequest timeLineRequest = (HttpWebRequest)WebRequest.Create(timelineUrl);
      var timelineHeaderFormat = "{0} {1}";
      timeLineRequest.Headers.Add("Authorization", string.Format(timelineHeaderFormat, twitAuthResponse.token_type, twitAuthResponse.access_token));
      timeLineRequest.Method = "Get";
      WebResponse timeLineResponse = timeLineRequest.GetResponse();
      var timeLineJson = String.Empty;
      using (timeLineResponse) {
        using (var reader = new StreamReader(timeLineResponse.GetResponseStream())) {
          timeLineJson = reader.ReadToEnd();
        }
      }
      return Content(timeLineJson, "application/json");
    }

  }
}
