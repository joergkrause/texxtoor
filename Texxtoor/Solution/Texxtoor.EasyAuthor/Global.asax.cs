using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Xml.Linq;
using Texxtoor.BaseLibrary;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.EasyAuthor;
using Texxtoor.Portal.Core.UI;
using WebConfigurationManager = System.Web.Configuration.WebConfigurationManager;

namespace Texxtoor.EasyAuthor {

    public class MvcApplication : HttpApplication {

    public static void RegisterViewEngine(ViewEngineCollection viewEngines) {
    }

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
        Server.Transfer("HttpErrorPage.aspx");
      }

      // For other kinds of errors give the user some information
      // but stay on the default page
      Response.Write(@"<style>body { margin: 200px 300px; font-family: Helvetica; } background-color: silver; }</style>");
      Response.Write(@"<img src='/Content/images/Texxtoor-160x56.png' style='margin: 50px 0px' />");
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

      // Log the exception and notify system operators

      // Clear the error from the server
      Server.ClearError();
    }

    protected void Session_Start() {
      var culture = Request.UserLanguages == null ? "en" : Request.UserLanguages.FirstOrDefault();
      culture = culture ?? "en";
      Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
      Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
      // if localhost the config setting is used, otherwise it's texxtoor on the portal only
# if !DEBUG
      var isAC2 = !IsTexxtoor || Boolean.Parse(WebConfigurationManager.AppSettings["texxtoor:ac2"]);
# else
      var isAC2 = false;
# endif
      // First we create the RunMode to manage the overall behavior
      var rm = new RunControl {
        UiLanguage = culture,
        RunMode = isAC2 ? RunMode.Business : RunMode.Texxtoor,
        Complexity = Complexity.Simple
      };
      if (User.Identity.IsAuthenticated) {
        // maybe we enter an existing, valid session 
        var profile = Manager<UserProfileManager>.Instance.GetProfileByUser(User.Identity.Name);
        if (profile != null) {
          rm = profile.RunControl;
        }
      }
      Session["RunControl"] = rm;
    }

    private static bool IsLocalHost {
      get {
        return HttpContext.Current == null || HttpContext.Current.Request.Url.Authority.Contains("localhost");
      }
    }

    private static bool IsTexxtoor {
      get {
        return HttpContext.Current == null || HttpContext.Current.Request.Url.Authority.Contains("texxtoor");
      }
    }

    protected void Application_Start() {
      Database.SetInitializer<PortalContext>(null);
      AreaRegistration.RegisterAllAreas();

      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      
      // Web API routing config
      GlobalConfiguration.Configure(WebApiConfig.Register);
      // Standard Routing
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      // Global Bundles
      BundleConfig.RegisterBundles(BundleTable.Bundles);

      ModelMetadataProviders.Current = new PropertyUiHintModelMetadataProvider(); // OLD: ModelMetadataProviders.Current
      DefaultModelBinder.ResourceClassKey = typeof(ModelResources).Name;
      //BundleTable.EnableOptimizations = true;
      // ReSharper disable PossibleNullReferenceException

      // Global Variables     
      var pDoc = XDocument.Load(Server.MapPath("~/App_Data/ProductionCalc.xml"));
      Application["ProductionCalc"] = pDoc;
    }

  }
}