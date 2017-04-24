using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using Texxtoor.Portal.Services;

namespace Texxtoor.Portal.ServiceApi.Services {
  // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEditorService" in both code and config file together.

  [ServiceContract]
  public interface IEditorService {

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic GetSnippet(int id);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic SaveReorganizedTree(int id, int source, int target, string position);

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
    dynamic InsertSnippet(int documentId, int chapterId, int id, string type, string variation, string data);

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
    dynamic SaveDialogData(string id, string dialog, Dictionary<string, string> form);
    #region -= Tree Helper =-

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic GetTreeData(int documentId);

    #endregion

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic InsertOrphanedSnippet(int documentId, int chapterId, int id, int afterSnippet);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic GetContentStructure(int id, int chapterId);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic GetContentStructureForNewChapter(int id, int chapterId);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic SearchSnippetId(int id, int chapterId, int snippetId, string value, int direction);

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
    dynamic Move(int id, int chapterId, int sectionId, int dropId, string move, bool withChildren);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic OrphanSnippet(int id, bool withChildren);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic DeleteSnippet(int id, bool delChildren);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic OrphanedSnippets();

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
    dynamic SaveContent(int id, int documentId, string content, string form);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic GetSidebarType();

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic GetThumbnails(int id, string w, string h);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic SemanticLists(int id, string type);

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
    List<int> GetAllChapterIds(int id);

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
    dynamic GetNextChapter(int id, int chapterId, string dir);

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
    dynamic LoadChapter(int id, int chapterId);

    #region Partial View Methods

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic Toc(int id);

    #endregion

    [OperationContract]
    [WebGet(RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare)]
    Stream GetImage(int id, bool c, string m);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic GetDialogData(string id, string dialog, string additionalData);

    //[OperationContract, WebInvoke(UriTemplate = "UploadImage/{id}/{title}", Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
    //[OperationContract, WebInvoke(UriTemplate = "UploadImage/{id}/{title}", Method = "POST")]
    //dynamic UploadImage(string id, string title, Stream fileContents);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
    dynamic LoadComments(int id, int snippetId, string target);

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
    dynamic SaveComment(int id, int snippetId, string target, string subject, string comment, bool closed);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic GetDocumentProperties(int id);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    string GetTranslation(int id, Translators engine, string fromLanguage, string toLanguage);

    # region SVG Edit

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle=WebMessageBodyStyle.Wrapped)]
    dynamic SaveSvg(int id, int imgId, int projectId, string svg, string filename);

    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
    dynamic SaveImage(int id, int svgId, int projectId, string img, string mimeType, string filename);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    dynamic ProjectLibrary(int id, string type);

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    EditorService.SvgData LoadSvg(int resourceId);

    # endregion


  }
}
