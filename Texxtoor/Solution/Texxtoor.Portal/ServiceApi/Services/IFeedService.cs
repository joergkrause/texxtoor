using System.IO;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;

namespace Texxtoor.Portal.ServiceApi.Services {
  
  [ServiceContract]
  [ServiceKnownType(typeof(Rss20FeedFormatter))]
  [ServiceKnownType(typeof(Atom10FeedFormatter))]
  public interface IFeedService {

    [OperationContract]
    [WebGet(UriTemplate = "RssPreview/{uid}/{pid}")]
    Rss20FeedFormatter RssPreview(string pid, string uid);

    [OperationContract]
    [WebGet(UriTemplate = "Rss/{pid}")]
    Rss20FeedFormatter Rss(string pid);

    [OperationContract]
    [WebGet(UriTemplate = "UserRss/{orderId}/{passwordHash}")]
    Rss20FeedFormatter UserRss(string orderId, string passwordHash);

    [OperationContract]
    [WebGet(UriTemplate = "GetFeedImagePreview/{id}")]
    Stream GetFeedImagePreview(string id);

    [OperationContract]
    [WebGet(UriTemplate = "GetFeedImage/{id}")]
    Stream GetFeedImage(string id);
  }
}
