using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using System.Threading.Tasks;
using Texxtoor.DataModels.ServiceModel;

namespace Texxtoor.Portal.ServiceApi.Services {

  [ServiceContract]
  public interface IReaderService {

    [OperationContract]
    [WebGet]
    String ClientTest(string test);

    [FaultContract(typeof(SignFault))]
    [OperationContract]
    [WebGet]
    Task<string> SignIn(String uname, String password);

    [FaultContract(typeof(SignFault))]
    [OperationContract]
    [WebGet]
    String SignOut(String ssid);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebGet]
    SearchResult SearchBook(String ssid, String searchString, String searchBy, String category, String order);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebGet]
    BookMetadata GetWork(String ssid, int bookId);

    [FaultContract(typeof (DataFault))]
    [OperationContract]
    [WebGet]
    Stream GetCover(string ssid, int bookId);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped)]
    BookMetadata[] GetNextSearchResults(String ssid, int[] bookIds);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebGet]
    int CountSearchResults(String ssid);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebGet]
    string GetBookFragment(String ssid, int bookId, String fragmentHref);

    [WebGet(BodyStyle=WebMessageBodyStyle.Bare)]
    Stream GetBookResource(String ssid, int bookId, String fragmentHref);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebGet]
    List<BookComment> GetComments(String ssid, int bookId, String fragmentHref);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebGet]
    BookComment[] GetParentComments(String ssid, int bookId, bool withContent);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "AddComment?ssid={ssid}", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    String AddComment(String ssid, BookComment comment);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebGet]
    Bookmark[] GetBookmarks(String ssid, int bookId);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "AddBookmark?ssid={ssid}", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    String AddBookmark(String ssid, Bookmark bookmark);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebGet]
    String DeleteBookmark(string ssid, int bookmarkId);

    [FaultContract(typeof(DataFault))]
    [OperationContract]
    [WebGet]
    BookMetadata[] GetBookList(String ssid);

  }

}
