using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.Portal.App_Start;
using Texxtoor.Portal.Core.Configuration;
using Texxtoor.Portal.Core.License;
using Texxtoor.Portal.Core.UI;
using Texxtoor.ViewModels.Users.Demo;
using WebConfigurationManager = System.Web.Configuration.WebConfigurationManager;

namespace Texxtoor.Portal {

  public class MvcApplication : HttpApplication {

    private static readonly XNamespace Ns = "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0";

    protected void Application_Error(Object sender, EventArgs e) {
      // Code that runs when an unhandled error occurs

      // Get the exception object.
      var exc = Server.GetLastError();

      // Handle HTTP errors
      if (exc.GetType() == typeof(HttpException)) {
        // The Complete Error Handling Example generates
        // some errors using URLs with "NoCatch" in them;
        // ignore these here to simulate what would happen
        // if a global.asax handler were not implemented.
        if (exc.Message.Contains("NoCatch") || exc.Message.Contains("maxUrlLength"))
          return;

        //Redirect HTTP errors to HttpError page
        Server.Transfer("/Home/Error");
      }
      #region Error Page
      // For other kinds of errors give the user some information
      // but stay on the default page
      var rm = UserProfileManager.Instance.GetProfileByUser(HttpContext.Current.User.Identity.Name).RunControl;
      var logo = WebConfigurationManager.AppSettings["Logo-" + rm.RunMode.ToString().ToLowerInvariant()];
      Response.Write(@"<style>body { margin: 200px 300px; font-family: Helvetica; } background-color: silver; }</style>");
      Response.Write(String.Format(@"<img src='/Content/images/{0}' style='margin: 50px 0px' />", logo));
      Response.Write(@"<h1>Well, this is awkward.<h1>");
      Response.Write(@"<h2>Looks like this page is kaput. Or on vacation. Or just playing hard to get. At any rate...it's not here.<h2>");
      Response.Write(@"<h3>In the meantime, <a href='/'>Go Home</a> or <a href='#' onlick='window.history.back()'>Go Back</a>.<h3>");
      Response.Write(@"
      <h4>The page you have requested cannot be found!</h4>
      <p>We have recently updated our website and this page may have been moved or is no longer available. Please select from the menu above or select a different country site.

      <h4 dir=rtl>لم يتم العثور على الصفحة المطلوبة ! </h4>
      <p dir=rtl>لقد قمنا بتحديث موقعنا مؤخراً و من الممكن أنه قد تم تغيير عنوان هذه الصفحة أو أنها لم تعد متوفرة.<br/>
      يرجى اختيار صفحة من القائمة أعلاه أو الانتقال إلى الموقع التابع لبلد آخر. </p>

      <h4>La page demandée n’a pas été trouvée!</h4>
      <p>Nous avons récemment mis à jour notre site Internet et cette page a pu être déplacée ou supprimée. Pour accéder au site Internet de votre pays, merci de cliquer ici.</p>

      <h4>Diese Seite konnte leider nicht gefunden werden.</h4>
      <p>Wir haben vor kurzem ein Website-Update vorgenommen. Die angefragte Seite wurde eventuell verschoben oder entfernt. Bitte klicke hier, um zu der Website in Ihrer Sprache  zu gelangen.</p>

      <h4>La pagina solicitada no se encuentra!</h4>
      <p>Hemos actualizado nuestra web y esta página puede haber cambiado de ubicación o no estar disponible. Para visitar la página web de su país por favor haga click aquí.</p>

      <h4>La pagina selezionata non e’ stata trovata!</h4>
      <p>Recentemente abbiamo aggiornato il nostro sito e questa pagina potrebbe essere stata spostata o non è più disponibile. Per visitare il sito web nel tuo paese clicca qui.</p>

      <h4>A página procurada não pode ser encontrada!</h4>
      <p>Nós estamos atualizando nosso site e esta página pode ter sido removida ou alterada. Para visitar nosso site em seu país clique aqui.</p>

      <h4>Запрашиваемая вами страница не может быть найдена!</h4>
      <p>Мы проводим обновление нашего сайта и данная страница может быть удалена либо перемещена. Для посещения сайта в вашей стране пожалуйста нажмите здесь.</p>

      <h4>Aradığınız sayfa bulunamamıştır!</h4>
      <p>Kısa bir süre önce web sitemizi güncelledik ve bu sayfa taşınmamış ya da artık kullanım dışı kalmış olabilir. Kendi ülkenizden web sitesini ziyaret etmek için buraya tıklayın.</p>
      
      <h4>Η σελίδα που αναζητάτε δεν είναι διαθέσιμη!</h4>
      <p>Πρόσφατα ανανεώσαμε την ιστοσελίδα μας και η σελίδα που αναζητάτε ή έχει μετακινηθεί ή δεν είναι πλέον διαθέσιμη. Για να επισκεφθείτε την ιστοσελίδα της χώρας σας παρακαλώ κάντε κλικ εδώ.</p>

      <h4>Siden du søger kan ikke findes!</h4>
      <p>Vi har for nyligt opdateret vores hjemmeside og denne side kan være flyttet eller nedlagt. For at besøge den danske hjemmeside klik her.</p>

      <h4>Sidan kan inte hittas!</h4>
      <p>Vi har nyligen uppdaterat vår hemsida och denna sida kan ha flyttats eller finns inte längre tillgänglig. För att besöka den lokala hemsidan i ditt land, klicka här.</p>

      <h4>요청하신 내용을 찾을 수 없습니다.</h4>
      <p>최근 웹사이트를 업데이트하였으므로 요청하신 페이지가 삭제되었거나 경로가 바뀌었을 수 있습니다. 해당 국가의 웹사이트 방문은 여기를 클릭하십시오.</p>

      <h4>您搜索的页面不存在!</h4>
      <p>我们的网站已经更新,您搜索的页面也许更改或不存在。请点击这里浏览您所在地的网页。</p>

      <h4>ページが表示できません。</h4>
      <p>検索中のページは、削除された、名前が変更された、または現在利用できない可能性があります。ご希望言語のホームページはこちらより検索いただけます。</p>");

      Response.Write("<!-- ");
      Response.Write("Global Page Error\n");
      Response.Write("MSG: " + exc.Message + "\n");
      Response.Write("STR: " + exc.StackTrace + "\n");
      var cnt = 1;
      while ((exc = exc.InnerException) != null) {
        Response.Write("INN (" + cnt + "): " + exc.Message + "\n");
        Response.Write("STR (" + cnt + "): " + exc.StackTrace + "\n");
        cnt++;
      }
      Response.Write(" -->");
      Response.Write("Return to the <a href='/'>" + "Default Page</a>\n");
      #endregion
      // Log the exception and notify system operators

      // Clear the error from the server
      Server.ClearError();

    }

    protected void Session_Start() {
      // TODO: Implement and add to Runcontrol
      var tol = (LicenceType) Enum.Parse(typeof(LicenceType), ConfigurationManager.AppSettings["production:TypeOfLicence"], true);
      Application["TypeOfLicence"] = tol;
      // END TODO
      var culture = Request.UserLanguages == null ? "en" : Request.UserLanguages.FirstOrDefault();
      culture = culture ?? "en";
      Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
      Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
      // if localhost the config setting is used, otherwise it's texxtoor on the portal only
      // First we create the RunMode to manage the overall behavior
      var rm = new RunControl {
        UiLanguage = culture,
        RunMode = (RunMode)Application["RunMode"],
        Complexity = Complexity.Full
      };
      if (User.Identity.IsAuthenticated) {
        // maybe we enter an existing, valid session 
        var rep = Manager<UserProfileManager>.Instance;
        var profile = rep.GetProfileByUser(User.Identity.Name);
        if (profile != null) {
          rm = profile.RunControl;
          rm.RunMode = (RunMode)Application["RunMode"];
          rep.SaveChanges();
        }
      }
    }

    void Application_BeginRequest(Object source, EventArgs e) {
      var app = (HttpApplication)source;
      var host = FirstRequestInitialization.Initialize(app.Context);
      try {
        var tryExists = WebConfigurationManager.ConnectionStrings[FirstRequestInitialization.HostMapping[host]];
        if (tryExists == null)
          throw new ConfigurationErrorsException("Host mapping invalid. This host ist not allowed: " + host);
      } catch (Exception ex) {
        throw new ConfigurationErrorsException("Host mapping invalid. Exception: " + ex.Message + " Host: " + host);
      }
      try {
        Application["RunMode"] = FirstRequestInitialization.RunMapping[host];
      } catch (Exception ex) {
        throw new ConfigurationErrorsException("Run mapping invalid. Exception: " + ex.Message + " Host: " + host);
      }
      /*************** Database ***************/
      Application["ConnectionStringName"] = FirstRequestInitialization.HostMapping[host];

    }

    static class FirstRequestInitialization {
      private static readonly Dictionary<string, RunMode> runMapping = new Dictionary<string, RunMode>();
      private static readonly Dictionary<string, string> hostMapping = new Dictionary<string, string>();
      private static string _host = null;
      private static readonly Object SLock = new Object();

      // Initialise only on the first request
      public static string Initialize(HttpContext context) {
        if (!string.IsNullOrEmpty(_host) && runMapping.Any() && hostMapping.Any())
          return _host;
        lock (SLock) {
          if (!string.IsNullOrEmpty(_host))
            return _host;
          var uri = context.Request.Url;
          _host = uri.GetLeftPart(UriPartial.Authority);
          // read config
          var config = ConfigurationManager.GetSection("applicationSettings/Texxtoor.Portal.Core.Configuration") as TexxtoorConfiguration;
          if (config != null) {
            foreach (TexxtoorRunModeElement mode in config.RunModes) {
              runMapping.Add(mode.Url, mode.TargetRunMode);
              hostMapping.Add(mode.Url, mode.ConnectionStringName);
            }
          }
        }

        return _host;
      }

      public static Dictionary<string, RunMode> RunMapping
      {
        get
        {
          if (_host == null)
            throw new ApplicationException("FirstRequestInitialization failed (RunMapping)");
          return runMapping;
        }
      }

      public static Dictionary<string, string> HostMapping
      {
        get
        {
          if (_host == null)
            throw new ApplicationException("FirstRequestInitialization failed (HostMapping)");
          return hostMapping;
        }
      }

    }

    protected void Application_Start() {
      Database.SetInitializer<PortalContext>(null);
#if DEBUG
      // While debugging we re-create the sprite environment, 
      // at run time, we just take the results "as is" as there no need to have anything dynamic here
      //DynamicModuleUtility.RegisterModule(typeof(ImageOptimizationModule));
#endif
      AreaRegistration.RegisterAllAreas();

      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

      //This ist now in the assembly attribute
      //HubConfig.RegisterHubs();

      // We use "private" api calls in the areas, which is not supported by default, this Selector is aware of this      
      //GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), new AreaHttpControllerSelector(GlobalConfiguration.Configuration));

      // Web API routing config
      GlobalConfiguration.Configure(WebApiConfig.Register);
      // Standard Routing
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      // Global Bundles
      BundleConfig.RegisterBundles(BundleTable.Bundles);

      ModelMetadataProviders.Current = new PropertyUiHintModelMetadataProvider(); // OLD: ModelMetadataProviders.Current
      DefaultModelBinder.ResourceClassKey = typeof(ModelResources).Name;


      BundleTable.EnableOptimizations = false;
      // ReSharper disable PossibleNullReferenceException

      // Global Variables     

      /*************** Site Map ***************/
      var siteMap = XDocument.Load(Server.MapPath("~/Web.sitemap"));
      // localize Web sitemap statically to improve performance
      if (Boolean.Parse(siteMap.Root.Attribute("enableLocalization").Value)) {
        var cultures = WebConfigurationManager.AppSettings["ui:SupportedCultures"].Split(',');
        siteMap.Root.Descendants(Ns + "siteMapNode")
          .ToList()
          .ForEach(e => {
            if (e.Attribute("resourceKey") == null)
              return;
            foreach (var culture in cultures) {
              var c = new CultureInfo(culture);
              var res = SiteMapResources.ResourceManager.GetString(e.Attribute("resourceKey").Value, c);
              if (res != null) {
                e.Add(new XAttribute("title-" + culture, res));
              }
            }
          });
      }
      Application["WebSiteMap"] = siteMap;
      /*************** Demo Data *************/
      var wDoc = XDocument.Load(Server.MapPath("~/App_Data/DemoWizard.xml"));
      var w = wDoc.Root
        .Elements("wizard")
        .Select(e => new DemoWizard {
          Id = e.Attribute("id").Value,
          Language = e.Attribute("lang").Value,
          Title = e.Attribute("title").Value,
          Image = e.Element("img").Attribute("src").Value,
          Description = e.Element("desc").Value,
          DemoData = new Demo {
            UserName = e.Element("demo").Attribute("username").Value,
            Password = e.Element("demo").Attribute("password").Value,
            Pages = e.Element("demo").Element("pages").Elements("page").Select(p => new Page {
              Url = p.Attribute("url").Value + (p.Attribute("querystring") != null && p.Attribute("querystring").Value.Length > 0 ? "?" + p.Attribute("querystring").Value : ""),
              Steps = p.Element("steps")
                .Elements("step")
                .DescendantNodes()
                .Where(s => s.NodeType == XmlNodeType.CDATA)
                .Select(s => ((XCData)s).Value).ToList()
            }).ToList()
          }
        }).ToList();
      // ReSharper restore PossibleNullReferenceException
      Application["DemoWizard"] = w;
      /************** Calculation Production *************/
      var pDoc = XDocument.Load(Server.MapPath("~/App_Data/ProductionCalc.xml"));
      Application["ProductionCalc"] = pDoc;
    }

  }
}