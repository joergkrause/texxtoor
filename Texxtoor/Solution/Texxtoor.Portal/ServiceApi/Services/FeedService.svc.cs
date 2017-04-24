using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Web;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Helper;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.Portal.ServiceApi.Services {

  /// <summary>
  /// The Feed Service for preview and perma link.
  /// </summary>
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [DataContract(Namespace = "http://www.texxtoor.de")]
  public class FeedService : IFeedService {

    private T UnitOfWork<T>() where T : class, IManager, new() {
      return Manager<T>.Instance as T;
    }

    public Rss20FeedFormatter RssPreview(string pid, string uid) {
      var path = HttpContext.Current.Server.MapPath("~/Download/");
      int userId = Int32.Parse(uid);
      int id = Int32.Parse(pid);
      var user = Manager<UserManager>.Instance.GetUser(userId);
      var opus = ProjectManager.Instance.GetOpus(id, HttpContext.Current.User.Identity.Name);
      var feed = new SyndicationFeed(opus.Name, "Test feed from project " + opus.Project.Name, HttpContext.Current.Request.Url);
      feed.Authors.Add(new SyndicationPerson(user.Email));
      feed.Categories.Add(new SyndicationCategory("Texxtoor Work Preview"));
      feed.Description =
        new TextSyndicationContent("This is a preview feed that shows the RSS feed of the current work.");
      var items = opus.Children.OfType<Section>().Select(chapter => new SyndicationItem(
                                                                    chapter.RawContent,
                                                                    ProductionManager.Instance.CreateChapterHtml(opus,
                                                                                                                chapter,
                                                                                                                createImagePreview,
                                                                                                                null,
                                                                                                                path,
                                                                                                                // Could use .Rss for different output?
                                                                                                                GroupKind.Html,
                                                                                                                false),
                                                                    HttpContext.Current.Request.Url,
                                                                    "txxt" + chapter.Id,
                                                                    DateTime.Now)).ToList();
      feed.Items = items;
      return new Rss20FeedFormatter(feed);
    }

    public Rss20FeedFormatter Rss(string pid) {
      var path = HttpContext.Current.Server.MapPath("~/Download/");
      int id = Int32.Parse(pid);
      var publ = ProjectManager.Instance.GetPublished(id);
      var feed = new SyndicationFeed(publ.Title, "Article " + publ.Title, HttpContext.Current.Request.Url);
      feed.Authors.Add(new SyndicationPerson(String.Join(", ", publ.Authors.Select(u => String.Format("{0} {1}", u.Profile.FirstName, u.Profile.LastName)).ToArray())));
      feed.Categories.Add(new SyndicationCategory("texxtoor" +
                                                  ""));
      feed.Description = new TextSyndicationContent(publ.SubTitle);
      UnitOfWork<ProjectManager>().AsUnitOfWork().LoadProperty(publ, p => p.FrozenFragments);
      var items = publ.FrozenFragments
                  .Where(f => f.TypeOfFragment == FragmentType.Html)
                  .Select(ff => new SyndicationItem(
                          publ.Title,
                          ProductionManager.Instance.CreateFragmentHtml(ff, createImageFinal, null, false),
                          HttpContext.Current.Request.Url,
                          "txxt" + ff.Id,
                          DateTime.Now)).ToList();
      feed.Items = items;
      return new Rss20FeedFormatter(feed);
    }

    public Rss20FeedFormatter UserRss(string orderId, string passwordHash) {
      // this is the final product for users who order feeds (free or not or hold valid subscription), orderId is Order.Id
      // TODO: Check subscription or order against user account data
      // TODO: Create Feed or error feed
      var feed = new SyndicationFeed("Error", "No Feed", HttpContext.Current.Request.Url);
      return new Rss20FeedFormatter(feed);
    }

    public Stream GetFeedImagePreview(string id) {
      var iid = Int32.Parse(id);
      var element = UnitOfWork<ProjectManager>().Ctx.Elements.SingleOrDefault(f => f.Id == iid);
      return element == null ? null : new MemoryStream(element.Content) { Position = 0 };
    }

    public Stream GetFeedImage(string id) {
      var frozenFragment = UnitOfWork<ProjectManager>().Ctx.FrozenFragments.SingleOrDefault(f => f.ItemHref == id);
      return frozenFragment == null ? null : new MemoryStream(frozenFragment.Content) { Position = 0 };
    }

    private string createImagePreview(object sender, CreateImageArguments e) {
      // take image and process a calling path, use elements from current opus
      return String.Format("/ServiceApi/Services/FeedService.svc/GetFeedImagePreview/{0}", ((Snippet)sender).Id);
    }

    private string createImageFinal(object sender, CreateImageArguments e) {
      // take image and process a calling path, use content in frozen fragments here
      return String.Format("/ServiceApi/Services/FeedService.svc/GetFeedImage/{0}", e.FileName);
    }

    //private string scaleImage(object sender, ScaleImageEventArgs e) {
    //  // take image and process a calling path
    //}

  }
}
