using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.DataModels.ServiceModel;
using System.Collections.Generic;
using Texxtoor.Portal.ServiceApi.Services.ServiceDtos;
using Texxtoor.BaseLibrary.Core.Utilities.Imaging;

namespace Texxtoor.Portal.ServiceApi.Services {

  /// <summary>
  /// Upload image to editor, publish from word add in to editor.
  /// </summary>
  [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.PerSession)]
  [DataContract(Namespace = "http://www.texxtoor.de")]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class UploadService : IUploadService {

    # region Security

    private Texxtoor.DataModels.Models.Session _currentSession;
    private SessionData _sessionData;

    private bool GetCurrentSession(string ssid) {
      // read session 
      _currentSession = ServiceManager.Instance.GetSession(ssid);
      if (_currentSession != null) {
        // read session data
        _sessionData = SessionData.Deserialize(_currentSession.SessionData);
      }
      return (_currentSession != null);
    }

    # region Logon / Logoff Function

    public string SignIn(string uname, string password) {
      try {
        var result = UserManager.Instance.Usermanager.CheckPassword(new User { UserName = uname, Password = password }, password);
        if (result) {
          _currentSession = ServiceManager.Instance.SignIn(uname);
          return _currentSession.SessionId;
        } else {
          throw new Exception("User Not Found");
      }
      } catch (Exception ex) {
        throw new FaultException<SignFault>(new SignFault { Operation = "SignIn", Description = ex.Message });
      }
    }

    public string SignOut(string ssid) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      try {
        // remove session data from table (consider storing this later for data mining purpose)
        ServiceManager.Instance.CleanUpSession(ssid, _currentSession);
        return "OK";
      } catch (Exception ex) {
        throw new FaultException<SignFault>(new SignFault { Operation = "SignOut", Description = ex.Message });
      }
    }

    # endregion

    # endregion

    # region Image Handling

    public IList<int> GetServerImages(string ssid, int documentId) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      var userName = _currentSession.User.UserName;
      var projectId = ProjectManager.Instance.GetOpus(documentId, userName).Project.Id;
      var files = ResourceManager.Instance.GetAllFiles(projectId, TypeOfResource.Content, userName, "image/png", "image/jpg");
      return files.Select(f => f.Id).ToList();
    }

    public byte[] GetServerImage(string ssid, int id, bool asThumbnail, string thumbNailSize) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      var resId = ResourceManager.Instance.GetFile(id).ResourceId;
      using (var blob = BlobFactory.GetBlobStorage(resId, BlobFactory.Container.Resources)) {
        var bytes = blob.Content;
        if (!asThumbnail) return bytes;
        int w, h;
        thumbNailSize.GetIntPair(out w, out h);
        return ImageUtil.GetThumbnailImage(bytes, w, h);
      }
    }

    public string GetServerImageName(string ssid, int id) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      var resName = ResourceManager.Instance.GetFile(id).Name;
      return resName;
    }

    # endregion

    # region Add In Support

    /// <summary>
    /// Usage: 
    /// Provide ssid + html + projectId only creates new document and returns this document's id
    /// </summary>
    /// <param name="ssid"></param>
    /// <param name="xml"></param>
    /// <param name="opus"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    private void SaveDocument(string ssid, string xml, Opus opus, string userName) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xml))) {
        var xDoc = XDocument.Load(xmlStream);
        ProjectManager.Instance.RestoreOpusFromFile(opus.Id, xDoc, userName);
      }
    }

    public int PublishNewDocument(string ssid, int projectId, string name, string html) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      var userName = _currentSession.User.UserName;
      var opus = ProjectManager.Instance.CreateOpusForProject(projectId, name);
      SaveDocument(ssid, html, opus, userName);
      return opus.Id;
    }

    public int PublishDocument(string ssid, int documentId, string html) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      var userName = _currentSession.User.UserName;
      var opus = ProjectManager.Instance.GetOpus(documentId, userName);
      SaveDocument(ssid, html, opus, userName);
      return opus.Id;
    }

    public IList<ServiceElement> GetAllProjects(string ssid) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      var userName = _currentSession.User.UserName;
      return ProjectManager.Instance.GetProjectsWhereUserIsMember(userName)
        .Where(p => p.Active)
        .ToList()
        .Select(p => new ServiceElement {
          Id = p.Id,
          Name = p.Name,
          Children = p.Opuses.Select(o => new ServiceElement {
            Id = o.Id,
            Name = o.Name
          }).ToList()
        }).ToList();
    }

    public IEnumerable<Comment> LoadComments(string ssid, int id, int snippetId, string target) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      var comments = EditorManager.Instance.GetComments(snippetId, target)
        .Select(c => new Comment {
          Subject = c.Subject,
          Text = c.Content,
          Date = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(),
          UserName = c.Owner.UserName
        });
      var objSerializer = new JavaScriptSerializer();
      return comments;
    }

    public IEnumerable<Comment> SaveComment(string ssid, int id, int snippetId, string target, string subject, string comment, bool closed) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      var noTagWord = new List<string> { "the", "that", "if", "then", "where" };
      var currentComments = EditorManager.Instance.GetComments(snippetId, target).ToList();
      var user = Manager<UserManager>.Instance.GetUserByName(HttpContext.Current.User.Identity.Name);
      var item = new WorkitemChat {
        Content = comment,
        Owner = user, // TODO: Replace with User
        Closed = closed,
        Mood = 2,
        Private = target == "me",
        GroupOnly = target == "team",
        Snippet = (Snippet)EditorManager.Instance.GetElement(snippetId),
        Subject = subject,
        Name = target,
        Tags = String.Join(",", comment.Split(' ', '.', ',').Where(e => !noTagWord.Contains(e)).ToArray()),
        OrderNr = currentComments.Any() ? currentComments.Max(e => e.OrderNr) + 1 : 1
      };
      EditorManager.Instance.SaveComment(null, item);
      // avoid another DB call to simply get a fast response and refresh UI
      currentComments.Insert(0, item);
      // return the new set immediately to keep the display current
      var comments = currentComments.Select(c => new Comment {
        Subject = c.Subject,
        Text = c.Content,
        Date = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(),
        UserName = user.UserName
      });
      return comments;
    }

    public DocumentProperties GetDocumentSettings(string ssid, int id) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      var opus = ProjectManager.Instance.GetOpus(id, _currentSession.User.UserName);
      var dp = new DocumentProperties {
        AllowChapters = opus.AllowChapters,
        AllowMetaData = opus.AllowMetaData,
        ChapterDefault = opus.ChapterDefault,
        SectionDefault = opus.SectionDefault,
        TextSnippetDefault = opus.TextSnippetDefault,
        ListingSnippetDefault = opus.ListingSnippetDefault,
        ShowNaviPane = opus.ShowNaviPane,
        ShowFlowPane = opus.ShowFlowPane,
        ShowNumberChain = opus.ShowNumberChain,
        DocumentLanguage = opus.LocaleId
      };
      return dp;
    }

    public IEnumerable<KeyValuePair<int, string>> SemanticLists(string ssid, int id, TermType type) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      var sl = ProjectManager.Instance.GetSemantics(id, type);
      return sl;
    }


    # endregion Add In Support

  }


}
