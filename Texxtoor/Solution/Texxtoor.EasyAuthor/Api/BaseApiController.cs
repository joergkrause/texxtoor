using System.Globalization;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using Texxtoor.BaseLibrary.Core.Notifications;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.EasyAuthor.Core.Extensions;

namespace Texxtoor.EasyAuthor.Api
{

    /// <summary>
    /// This class is a base controller for all Web API controllers.
    /// Functionality is adopted from  Texxtoor.Portal.Core.Extensions.ControllerExt
    /// <see cref="ControllerExt"/>
    /// </summary>
    public abstract class BaseApiController : ApiController
    {
        protected string UserName
        {
            get
            {
                return this.User == null ? string.Empty : this.User.Identity.Name;
            }
        }

        protected string CurrentCulture
        {
            get
            {
                var cult = Thread.CurrentThread.CurrentUICulture;
                return cult.Parent.Equals(CultureInfo.InvariantCulture)
                         ? cult.TwoLetterISOLanguageName
                         : cult.Parent.TwoLetterISOLanguageName;
            }
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            var userProfile = string.IsNullOrWhiteSpace(this.UserName) ? null : this.UnitOfWork<UserProfileManager>().GetProfileByUser(this.UserName);
            if (userProfile != null && userProfile.RunControl != null)
            {
                // RESTful services shouldn't use any session information
                // That's why everything is being saved on per request basis
                controllerContext.Request.Properties.Add("RunControl", userProfile.RunControl);
                Thread.CurrentThread.CurrentUICulture =
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(userProfile.RunControl.UiLanguage);
            }

            base.Initialize(controllerContext);
        }

        protected bool GetAc2()
        {
            return this.ControllerContext.Request.Properties.ContainsKey("RunControl")
                   && ((RunControl)this.ControllerContext.Request.Properties["RunControl"]).RunMode == RunMode.Business;
        }

        protected T UnitOfWork<T>() where T : class, IManager, new()
        {
            return this.UnitOfWork<T>(null);
        }

        protected T UnitOfWork<T>(INotificationService notificationService) where T : class, IManager, new()
        {
            var rep = Manager<T>.Instance;
            rep.Notification = notificationService;
            return rep;
        }
    }
}
