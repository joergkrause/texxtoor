using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Functions;
using Texxtoor.DataModels.ServiceModel;
using Bookmark = Texxtoor.DataModels.Models.Reader.Functions.Bookmark;

namespace Texxtoor.Portal.ServiceApi.Services {

  /// <summary>
  /// All BLL functions for projects.
  /// </summary>
  public class ServiceManager : Manager<ServiceManager> {

    public Session GetSession(string ssid) {
      return Ctx.Sessions.FirstOrDefault(s => s.SessionId == ssid);
    }

    public Session SignIn(string userName) {
      var user = GetCurrentUser(userName);
      string currentSessionId = Guid.NewGuid().ToString();
      var currentSession = new Texxtoor.DataModels.Models.Session {
        User = user,
        SessionId = currentSessionId,
        SessionData = new SessionData { UserId = user.Id }.ToString()
      };
      Ctx.Sessions.Add(currentSession);
      Ctx.SaveChanges();
      return currentSession;
    }

    public void CleanUpSession(string ssid, Session currentSession) {
      var remove = Ctx.SearchTracking.Where(st => st.SessionId == ssid);
      foreach (var item in remove) {
        Ctx.SearchTracking.Remove(item);
      }
      if (currentSession != null) {
        Ctx.Sessions.Remove(currentSession);
      }
      Ctx.SaveChanges();
    }

    public BookMetadata GetNextSearchResult(SessionData sessionData, int bookId) {
      if (bookId == -1) {
        return null; // no results
      }
      // take the one with bookId
      var work = Ctx.Works
        .Include(w => w.Fragments)
        .Include(w => w.Fragments.Select(f=> f.FrozenFragment))
        .Include(w => w.Published)        
        .FirstOrDefault(w => w.Id == bookId);
      // Create Meta Data
      var book = SetBookMetaData(work);
      book.BookId = bookId;
      SaveChanges();
      return book;
    }

    public IQueryable<int> CreateSearchQuery(SessionData sessionData, string searchString, string searchBy, string category, string order) {
      IQueryable<int> query = null;
      switch (order) {
        case "asc":
          # region ASC
          switch (searchBy.ToLowerInvariant()) {
            case "title":
              query = Ctx.Works
                  .Where(p => p.Name.Contains(searchString))
                  .OrderBy(p => p.Name)
                  .Select(p => p.Id);
              break;
            case "note":
              query = Ctx.Works
                  .Where(p => p.Note.Contains(searchString))
                  .OrderBy(p => p.Name)
                  .Select(p => p.Id);
              break;
            case "content":
              var content = Ctx.Works
                .OrderBy(p => p.Name)
                .Select(p => p);
              var result = new List<int>();
              // TODO: Implement fast full text search
              query = result.AsQueryable();
              break;
          }
          # endregion
          break;
        case "desc":
          # region DESC
          switch (searchBy.ToLowerInvariant()) {
            case "title":
              query = Ctx.Works
                  .Where(p => p.Name.Contains(searchString))
                  .OrderByDescending(p => p.Name)
                  .Select(p => p.Id);
              break;
            case "note":
              query = Ctx.Works
                  .Where(p => p.Note.Contains(searchString))
                  .OrderByDescending(p => p.Name)
                  .Select(p => p.Id);
              break;
            case "content":
              var contentDesc = Ctx.Works
                .OrderBy(p => p.Name)
                .Select(p => p);
              var resultDesc = new List<int>();
              // TODO: Search
              break;
          }
          # endregion
          break;
      }
      return query;
    }

    public SearchResult CreateSearchResult(SessionData sessionData, string ssid, IQueryable<int> query) {
      SearchResult sresult;
      // Add result to search tracking for data mining
      var trackId = Ctx.SearchTracking
        .Where(s => s.SessionId == ssid)
        .Select(t => t.TrackId)
        .DefaultIfEmpty()
        .Max(i => i);
      foreach (var item in query) {
        Ctx.SearchTracking.Add(new Texxtoor.DataModels.Models.SearchTracking {
          TrackId = trackId,
          BookId = item,
          SessionId = ssid,
          Active = true
        });
      }
      SaveChanges();
      // save session
      sessionData.SearchCount = query.Count();
      SaveChanges();
      // create result
      if (!query.Any()) {
        sresult = new SearchResult {
          ErrCode = "EMPTY"
        };
      } else {
        sresult = new SearchResult {
          BookIds = Ctx.SearchTracking
            .Where(st => st.SessionId == ssid)
            .Select(st => st.BookId)
            .ToArray(),
          ErrCode = "OK"
        };
      }
      return sresult;
    }

    public BookMetadata[] GetBookMetadata(SessionData sessionData) {
      var user = Manager<UserManager>.Instance.GetUser(sessionData.UserId);
      // Custom Work only
      var query = Ctx
        .Works
        .Include(w => w.Fragments)
        .Include(w => w.Fragments.Select(f => f.FrozenFragment))
        .Include(w => w.Published)
        .Where(w => w.Owner.Id == user.Id);
      if (query.Any()) {
        var books = query.ToList().Select(w => {
          return SetBookMetaData(w);
        });
        return books.ToArray();
      }
      return null;
    }

    public BookMetadata[] GetBookMetadata(SessionData sessionData, int[] bookIds) {
      if (bookIds.Count() == 0) {
        return null; // no results
      }
      // take the one with bookId
      var works = Ctx.Works
        .Include(w => w.Fragments)
        .Include(w => w.Fragments.Select(f => f.FrozenFragment))
        .Include(w => w.Published)
        .Where(w => bookIds.Any(id => id == w.Id))
        .DefaultIfEmpty();
      var books = works.ToList().Select(SetBookMetaData);
      SaveChanges();
      return books.ToArray();
    }

    public string DeleteBookmark(SessionData sessionData, int bookmarkId) {
      var userName = Ctx.Users.Find(sessionData.UserId).UserName;
      var bookmark = Ctx.Bookmarks.FirstOrDefault(b => b.Owner.UserName == userName && b.Id == bookmarkId);
      if (bookmark != null) {
        Ctx.Bookmarks.Remove(bookmark);
        SaveChanges();
      }
      return sessionData.ToString();
    }

    public string CommitBookmark(SessionData sessionData, DataModels.ServiceModel.Bookmark bookmark) {
      var user = Ctx.Users.Find(sessionData.UserId);
      var newBookmark = new Bookmark {
        FragmentHref = bookmark.FragmentCfi,
        Work = Ctx.Works.Find(bookmark.BookId),
        Owner = user
      };
      Ctx.Bookmarks.Add(newBookmark);
      SaveChanges();
      return sessionData.ToString();
    }

    public DataModels.ServiceModel.Bookmark[] GetBookmarks(SessionData sessionData, int bookId) {
      var bookmarks = Ctx.Bookmarks
        .Where(b => b.Work.Id == bookId && b.Owner.Id == sessionData.UserId)
        .ToList()
        .Select(b => new DataModels.ServiceModel.Bookmark {
          Id = b.Id,
          BookId = bookId,
          FragmentCfi = b.FragmentHref
        });
      return bookmarks.ToArray();
    }

    public BookComment[] GetComments(SessionData sessionData, int bookId, bool withContent) {
      var comments = Ctx.Comments
        .Where(b => b.Work.Id == bookId && b.Parent == null);
      if (comments.Any()) {
        var commentList = comments.ToList().Select(b => new BookComment {
          BookId = bookId,
          FragmentCfi = b.CfiRef,
          Subject = b.Subject,
          Content = withContent ? b.Content : ""
        });
        return commentList.ToArray();
      } else {
        return new List<BookComment>().ToArray();
      }
    }

    public string AddComment(SessionData sessionData, BookComment comment) {
      var user = Ctx.Users.Find(sessionData.UserId);
      var userName = user.UserName;
      Comment parentComment = null;
      if (comment.ParentId.HasValue && comment.ParentId.Value > 0) {
        parentComment = Ctx.Comments.FirstOrDefault(c => c.Id == comment.ParentId);
      }
      Comment newComment = null;
      if (comment.Id > 0) {
        newComment = Ctx.Comments.FirstOrDefault(c => c.Owner.UserName == userName && c.Id == comment.Id);
      }
      if (newComment == null) {
        var subject = comment.Subject;
        var priv = subject.StartsWith("NOTE") || subject.Split(':')[1].ToLower() == "private";
        CommentsType type = CommentsType.Private;
        if (subject.StartsWith("COMM")) {
          type = (CommentsType)Enum.Parse(typeof(CommentsType), subject.Split(':')[1], true);
        }
        // ADD
        newComment = new Comment {
          Work = Ctx.Works.Find(comment.BookId),
          CfiRef = comment.FragmentCfi,
          Subject = comment.Subject, // internally only
          Content = comment.Content,
          Owner = user,
          Parent = parentComment,
          Name = comment.NavigationItem,
          Private = priv,
          //CommentType = type,
          Tags = ""
        };
        Ctx.Comments.Add(newComment);
      } else {
        // EDIT
        newComment.Subject = "COMM:Private";
        newComment.Content = comment.Content;
      }
      SaveChanges();
      return sessionData.ToString();
    }

    public List<BookComment> GetComments(SessionData sessionData, int bookId, string fragmentName) {
      // get all users' comments
      var userId = sessionData.UserId;
      var comments = Ctx.Comments
        .Include(c => c.Owner)
        .Include(c => c.Children)
        .Where(c => c.Work.Id == bookId 
          && c.Name == fragmentName
          // only root comment
          && c.Parent == null
          // private only for user
          //&& (
          //  (c.CommentType == CommentsType.Private && c.Owner.Id == userId) 
          //  // all other are always public
          //  // TODO: show group comments only to group members
          //  || c.CommentType != CommentsType.Private) 
            );
      if (comments.Any()) {
        var bookComments = new List<BookComment>();
        GetCommentsRecursively(bookId, comments, bookComments);
        return bookComments;
      }
      return null;
    }

    private void GetCommentsRecursively(int bookId, IEnumerable<Comment> comments, ICollection<BookComment> bookComments) {
      foreach (var comment in comments) {
        var bc = new BookComment {
          Id = comment.Id,
          BookId = bookId,
          Owner = comment.Owner.UserName,
          Content = comment.Content,
          NavigationItem = comment.Name,  // item
          FragmentCfi = comment.CfiRef,   // position in item
          Subject = comment.Subject,      // pseudo subject with more data
          ParentId = (comment.Parent != null) ? comment.Parent.Id : 0
        };
        bookComments.Add(bc);
        if (!comment.HasChildren()) {
          continue;
        }
        bc.Children = new List<BookComment>();
        GetCommentsRecursively(bookId, comment.Children, bc.Children);
      }
    }

    public FrozenFragment GetBookFragmentData(int bookId, string href) {
      var work = Ctx.Works
        .Include(w => w.Fragments)
        .Include(w => w.Fragments.Select(f => f.FrozenFragment))
        .Include(w => w.Published)
        .FirstOrDefault(w => w.Id == bookId);
      // get the working fragment and retrieve content from associated frozen fragment
      var fragment = work.Fragments.Traverse(i => i.FrozenFragment.ItemHref == href).First().FrozenFragment;
      return fragment;
    }

    public Stream GetBookResourceData(int bookId, string href, out string mime) {
      mime = "";
      var work = Ctx.Works
        .Include(w => w.Fragments)
        .Include(w => w.Fragments.Select(f => f.FrozenFragment))
        .Include(w => w.Published)
        .FirstOrDefault(w => w.Id == bookId);
      // get the working fragment and retrieve content from associated frozen fragment
      var fragment = work.Fragments.Traverse(i => i.FrozenFragment.ItemHref == href).FirstOrDefault();
      if (fragment == null) return null;
      // expect binary data in the first child of the frozen fragment
      var content = fragment.FrozenFragment.Content;
      var ms = new MemoryStream(content) { Position = 0 };
      string mime1, mime2;
      switch (fragment.FrozenFragment.TypeOfFragment) {
        case FragmentType.Image:
          var img = Image.FromStream(ms);
          ms = new MemoryStream();
          img.Save(ms, ImageFormat.Jpeg);
          mime1 = "image";
          mime2 = "jpg";
          break;
        case FragmentType.Audio:
          mime1 = "x-application";
          mime2 = "audio";
          break;
        case FragmentType.Video:
          mime1 = "x-application";
          mime2 = "video";
          break;
        default:
          mime1 = "text";
          mime2 = "html";
          break;
      }
      mime = String.Concat(mime1, "/", mime2);
      return ms;
    }

    private BookMetadata SetBookMetaData(Work work) {
      var book = new BookMetadata();
      // top element only, all children are resources
      var fragments = work.Fragments.Where(f => f.Parent == null).Select(f => f.FrozenFragment).ToList();
      switch (work.Extern) {
        case WorkType.Published:
          var publ = work.Published;
          book.Authors = String.Join(", ", publ.Authors.Select(u => String.Format("{0} {1}", u.Profile.FirstName, u.Profile.LastName)).ToArray());
          book.Title = work.Name;
          book.BookId = work.Id;
          book.Cover = publ.CoverImage.GetFinalCoverBytes(publ);
          break;
        case WorkType.Custom:
          book.Title = work.Name;
          var allAuthors = work.Fragments.SelectMany(f => f.FrozenFragment.Published.Authors).Distinct();
          book.Authors = String.Join(", ", allAuthors.Select(u => String.Format("{0} {1}", u.Profile.FirstName, u.Profile.LastName)).ToArray());
          book.Cover = null; // TODO: Fancy Auto Algorithm
          break;
        case WorkType.External:
          book.Title = work.ExternalBook.PackageData.MetaData.Title.Text;
          book.Authors = work.ExternalBook.Author;
          book.Cover = work.ExternalBook.CoverImage;
          break;
        default:
          throw new NotSupportedException();
      }
      // The fragments are already on chapter level without any depth if in frozen state
      book.ItemHref = fragments.Select(f => f.ItemHref).ToArray();
      book.ItemSize = fragments.Select(f => f.Content == null ? -1 : f.Content.Length).ToArray();
      book.Navigation = GetNavElementFromNavMap(fragments.Where(f => f.TypeOfFragment == FragmentType.Html).ToList());
      return book;
    }

    private IList<NavElement> GetNavElementFromNavMap(IEnumerable<FrozenFragment> fragments) {
      var navigation = new List<NavElement>();
      GetNavElementFromNavMapRecursively(fragments, navigation);
      return navigation;
    }

    private void GetNavElementFromNavMapRecursively(IEnumerable<FrozenFragment> source, IList<NavElement> target) {
      foreach (var item in source) {
        var navElement = new NavElement {
          Content = item.ItemHref,
          LabelText = item.Name,
          MetaId = item.ItemHref,
          Name = item.Name,
          PlayOrder = item.OrderNr,
          OrderNr = item.OrderNr
        };
        if (item.HasChildren()) {
          var children = new List<NavElement>();
          GetNavElementFromNavMapRecursively(item.Children, children);
          navElement.Children = children.ToArray();
        }
        target.Add(navElement);
      }
    }

  }
}