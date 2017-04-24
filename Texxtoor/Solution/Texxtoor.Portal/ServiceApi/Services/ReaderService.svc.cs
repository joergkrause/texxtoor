using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.DataModels.ServiceModel;
using Texxtoor.DataModels.Context;
using System.Data.Entity.Validation;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Texxtoor.Portal.ServiceApi.Services {

  // per session as we store results in the session temporarily
  //[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
  [DataContract(Namespace = "http://www.texxtoor.de")]
  public class ReaderService : IReaderService {

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

    # region Test Functions

    public String ClientTest(string test) {
      return test;
    }

    # endregion

    # region Logon / Logoff Function

    public async Task<string> SignIn(string uname, string password) {
      try {
        var result = await UserManager.Instance.Usermanager.UserValidator.ValidateAsync(new User { UserName = uname, Password = password });
        if (result.Succeeded) {
          _currentSession = ServiceManager.Instance.SignIn(uname);
          return _currentSession.SessionId;
        } else {
          throw new FaultException<SignFault>(new SignFault { Operation = "SignIn", Description = "User Not Found" }, new FaultReason("Argument"));
        }
      } catch (Exception ex) {
        throw new FaultException<SignFault>(new SignFault { Operation = "SignIn", Description = ex.Message }, new FaultReason("Exception"));
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
        throw new FaultException<SignFault>(new SignFault { Operation = "SignOut", Description = ex.Message }, new FaultReason("Exception"));
      }
    }

    # endregion

    # region Search and Download Functions

    /// <summary>
    /// Search for published books
    /// </summary>
    /// <param name="ssid"></param>
    /// <param name="searchString"></param>
    /// <param name="searchBy"></param>
    /// <param name="category"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public SearchResult SearchBook(string ssid, string searchString, string searchBy, string category, string order) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      if (String.IsNullOrEmpty(searchBy)) {
        throw new ArgumentNullException("searchBy", "searchBy must have at least one character");
      }
      if (String.IsNullOrEmpty(order)) {
        order = "asc";
      }
      order = order.ToLowerInvariant();
      if (!order.Equals("asc") && !order.Equals("desc")) {
        throw new FaultException<DataFault>(new DataFault {
          Data = String.Format("SearchString {0}, SearchBy {1}, Category {2}, Order {3}", searchString, searchBy, category, order),
          Description = "order must provide 'asc', 'desc', or null.",
          Operation = "Search"
        });
      }
      IQueryable<int> query = ServiceManager.Instance.CreateSearchQuery(_sessionData, searchString, searchBy, category, order);
      if (query == null) {
        throw new FaultException<DataFault>(new DataFault {
          Data = String.Format("SearchString {0}, SearchBy {1}, Category {2}, Order {3}", searchString, searchBy, category, order),
          Description = "query returned no results.",
          Operation = "Search"
        });
      }
      SearchResult sresult = ServiceManager.Instance.CreateSearchResult(_sessionData, ssid, query);
      _currentSession.SessionData = _sessionData.ToString();
      return sresult;
    }

    public int CountSearchResults(string ssid) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      return _sessionData.SearchCount;
    }

    public BookMetadata GetWork(string ssid, int bookId) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      return ServiceManager.Instance.GetNextSearchResult(_sessionData, bookId);
    }

    public Stream GetCover(string ssid, int bookId) {
      return new MemoryStream(ServiceManager.Instance.GetNextSearchResult(_sessionData, bookId).Cover);
    }

    public BookMetadata[] GetNextSearchResults(string ssid, int[] bookIds) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      return ServiceManager.Instance.GetBookMetadata(_sessionData, bookIds);
    }

    // We load only those books already copied into the users private work store
    public string GetBookFragment(string ssid, int bookId, string href) {
      var c= ServiceManager.Instance.GetBookFragmentData(bookId, href).Content;
      if (c != null) {
        return Encoding.UTF8.GetString(c);
      }
      return String.Empty;
    }

    public Stream GetBookResource(string ssid, int bookId, string href) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      string mime;
      var ms = ServiceManager.Instance.GetBookResourceData(bookId, href, out mime);
      if (ms != null) {
        WebOperationContext.Current.OutgoingResponse.ContentType = mime;
        ms.Position = 0;
        return ms;
      }
      throw new FaultException<DataFault>(new DataFault {
        Data = String.Format("SSID {0}; BookId {1}; HRef {2}", ssid, bookId, href),
        Description = "Resource Not Found",
        Operation = "Read"
      });
    }

    # endregion

    # region Comments

    public List<BookComment> GetComments(string ssid, int bookId, string fragmentHref) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      return ServiceManager.Instance.GetComments(_sessionData, bookId, fragmentHref);
    }

    public string AddComment(string ssid, BookComment comment) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      if (comment == null || comment.Content == null || comment.Content.Length < 1) {
        throw new ArgumentNullException("comment cannot be empty");
      }
      var bookId = comment.BookId;
      try {
        _currentSession.SessionData = ServiceManager.Instance.AddComment(_sessionData, comment);
        return "OK";
      } catch (DbEntityValidationException vex) {
        throw new FaultException<DataFault>(new DataFault {
          Data = String.Format("SSID {0}; BookId {1}; Cfi {2}", ssid, bookId, comment.FragmentCfi),
          Description = "ERR " + vex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage,
          Operation = "CommitComment"
        });
      } catch (Exception ex) {
        throw new FaultException<DataFault>(new DataFault {
          Data = String.Format("SSID {0}; BookId {1}; Cfi {2}", ssid, bookId, comment.FragmentCfi),
          Description = ex.Message,
          Operation = "CommitComment"
        });
      }
    }

    public BookComment[] GetParentComments(string ssid, int bookId, bool withContent) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      return ServiceManager.Instance.GetComments(_sessionData, bookId, withContent);
    }

    # endregion Comment Functions

    # region Bookmark Functions

    public Texxtoor.DataModels.ServiceModel.Bookmark[] GetBookmarks(string ssid, int bookId) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      return ServiceManager.Instance.GetBookmarks(_sessionData, bookId);
    }

    public string AddBookmark(string ssid, Texxtoor.DataModels.ServiceModel.Bookmark bookmark) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      _currentSession.SessionData = ServiceManager.Instance.CommitBookmark(_sessionData, bookmark);
      return "OK";
    }

    public string DeleteBookmark(string ssid, int bookmarkId) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      _currentSession.SessionData = ServiceManager.Instance.DeleteBookmark(_sessionData, bookmarkId);
      return "OK";
    }

    # endregion

    # region Customer Functions

    public BookMetadata[] GetBookList(String ssid) {
      if (!GetCurrentSession(ssid)) {
        throw new InvalidOperationException("invalid ssid");
      }
      return ServiceManager.Instance.GetBookMetadata(_sessionData);
    }

    # endregion Customer Functions

    
  }
}