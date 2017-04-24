using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.ServiceModel;
using Texxtoor.Portal.ServiceApi.Services.ServiceDtos;

namespace Texxtoor.Portal.ServiceApi.Services {
  

  [ServiceContract]
  public interface IUploadService {

    [OperationContract, WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    int PublishDocument(string ssid, int documentId, string html);

    [OperationContract, WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    int PublishNewDocument(string ssid, int projectId, string name, string html);

    [OperationContract, WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    IList<ServiceElement> GetAllProjects(string ssid);

    [OperationContract, WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    string SignOut(string ssid);

    [OperationContract, WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    [FaultContract(typeof(SignFault))]
    string SignIn(string uname, string password);

    [OperationContract, WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    DocumentProperties GetDocumentSettings(string ssid, int id);

    [OperationContract, WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    IEnumerable<Comment> SaveComment(string ssid, int id, int snippetId, string target, string subject, string comment, bool closed);

    [OperationContract, WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    IEnumerable<Comment> LoadComments(string ssid, int id, int snippetId, string target);

    [OperationContract, WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    IEnumerable<KeyValuePair<int, string>> SemanticLists(string ssid, int id, TermType type);

    [OperationContract, WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    IList<int> GetServerImages(string ssid, int documentId);

    [OperationContract, WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    byte[] GetServerImage(string ssid, int id, bool asThumbnail, string thumbNailSize);

    [OperationContract, WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
    string GetServerImageName(string ssid, int id);

  }
}
