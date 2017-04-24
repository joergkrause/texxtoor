using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.Entity;
using System.Text;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.EPub;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.BusinessLayer {

  /// <summary>
  /// Reader / User functions with relation to products.
  /// </summary>
  public partial class ReaderManager {

    /// <summary>
    /// If the user tries to access a published book the very first time an associated work (private book instance) is being created.
    /// </summary>
    /// <remarks>
    /// User can store bookmarks, comments, and other custom information related to private copies only. Hence we need always a copy.
    /// This method retrieves an existing work (book) or, in case of first access, creates one.
    /// </remarks>
    /// <param name="publishedId"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public Work GetWorkForPublished(int publishedId, string userName) {
      var published = Ctx.Published.Find(publishedId);
      // first we try to get the default copy (that means a user's work which is derived from one published only)
      var work = Ctx.Works.FirstOrDefault(w => w.Owner.UserName == userName && w.Published.Id == publishedId);
      // if there is no one, create one
      if (work == null) {
        var newWork = new Work {
          Name = published.Title,
          Note = published.SubTitle,
          Owner = GetCurrentUser(userName),
          Fragments = CopyPublishedToWorkFragments(published.Id).ToList(),
          Published = published
        };
        Ctx.Works.Add(newWork);
        SaveChanges();
        work = newWork;
      }
      else {
        if (work.Fragments != null && work.Fragments.Any()) return work;
        work.Fragments = CopyPublishedToWorkFragments(published.Id).ToList();
        SaveChanges();
      }
      // get public work information, such as rating, from parent table
      return work;
    }

    public IQueryable<Work> GetAllWorkForPublished(int publishedId) {
      // all usages
      return Ctx.Works.Where(w => w.Published.Id == publishedId);
    }

    /// <summary>
    /// Assume the root element of the FrozenFragment collection is just a meta data item and will be stripped while copying.
    /// </summary>
    /// <param name="publishedId"></param>
    /// <returns></returns>
    public IEnumerable<WorkingFragment> CopyPublishedToWorkFragments(int publishedId) {
      var publ = Ctx.Published
        .Include(p => p.FrozenFragments)
        .Single(p => p.Id == publishedId);
      var fragments = new List<WorkingFragment>();
      // make working fragments from frozen fragments, assume chapter level fragments at root
      if (publ.FrozenFragments.Any(f => f.Parent == null)) {
        // Top level only, children remain as children
        var ff = publ.FrozenFragments.Where(f => f.Parent == null).OrderBy(f => f.OrderNr);
        fragments = CopyFrozenToWorkingFragments(ff);        
      }
      return fragments;
    }

    private List<WorkingFragment> CopyFrozenToWorkingFragments(IEnumerable<FrozenFragment> ff) {
      return ff.Select(f => new WorkingFragment {
        Children = f.HasChildren() ? CopyFrozenToWorkingFragments(f.Children) : null,
        Name = f.Name,
        OrderNr = f.OrderNr, 
        FrozenFragment = f
      }).ToList();
    }

    # region Work

    public Work AddWork(Work work, string userName) {
      var user = GetCurrentUser(userName);
      work.Owner = user;
      Ctx.Works.Add(work);
      SaveChanges();
      if (work.Public && work.Published != null){
        // sales statistics for authors, this is always a free service so he owns nothing
        // if we provide ads supported pages, this amount of clicks will pay out anyway
        var authors = work.Published.Authors;
        foreach (var author in authors){          
          Ctx.Sales.Add(new Sale{
            Day = work.CreatedAt,
            OrderCount = false,
            UserRevenues = 0,
            TotalRevenues = 0,
            OrderProduct = null,
            DownloadCount = false,
            ReaderCount = true,
            SubscriptionCount = false,
            SourceOpus = work.Published.SourceOpus,
            Owner = author
          });
        }
        SaveChanges();
      }
      return work;
    }

    # endregion

    public void AddWorkFromEPub(string userName, MemoryStream ms) {
      var user = GetCurrentUser(userName);
      var book = EBookFactory.Create(ms);
      Ctx.Books.Add(book);
      var work = new Work {
        Owner = user,
        ExternalBook = book,
        Name = book.PackageData.MetaData.Title.Text,
        Note = String.Format("External Book '{0}' ({1}) from {2}, published in {3}.",
          book.PackageData.MetaData.Title.Text,
          book.PackageData.MetaData.Publisher.Text,
          book.PackageData.MetaData.Creator.Text,
          book.PackageData.MetaData.Language.Text)
      };
      var fragments = new List<WorkingFragment>();
      // FrozenFragments are the ultimate storage
      Func<NavPoint, FrozenFragment> createFrozenFragment = null;
      createFrozenFragment = n => {
        var item = book.PackageData.Manifest.Items.FirstOrDefault(i => i.Href == n.Content);
        var f = new FrozenFragment {
          Name = n.LabelText,
          Title = n.LabelText,
          OrderNr = n.OrderNr,
          ItemHref = n.Identifier,
          Content = item == null ? null : item.Data
        };
        Ctx.FrozenFragments.Add(f);
        return f;
      };
      Func<NavPoint, WorkingFragment> convertNavToFragment = null;
      // workingFragments just handle reference points to have it handy without distributing the content again
      convertNavToFragment = n => new WorkingFragment {
        Name = n.LabelText,
        FrozenFragment = createFrozenFragment(n),
        OrderNr = n.OrderNr,
        Children = n.HasChildren() ? (n.Children.Select(child => convertNavToFragment(child)).ToList()) : null
      };
      book.NavigationData.NavMap.ForEach(n => fragments.Add(convertNavToFragment(n)));
      work.Fragments = fragments;
      Ctx.Works.Add(work);
      SaveChanges();
    }

    /// <summary>
    /// Retrieve a specific work including the attached products. Save for user.
    /// </summary>
    /// <param name="workId"></param>
    /// <param name="userName"> </param>
    /// <returns>The work or <c>null</c> if nothing found.</returns>
    public Work GetWorkWithProducts(int workId, string userName) {
      var result = Ctx.Works
        .Include(w => w.Products)
        .FirstOrDefault(w => w.Id == workId && w.Owner.UserName == userName);
      return result;
    }

    /// <summary>
    /// The published work this work is derived from, if any. 
    /// </summary>
    /// <remarks>
    /// If the user combines content from several sources, these all appear here at a list.
    /// Even private content, pulled from uploaded EPubs, is stored as "published" work but kept private to protect intellectual property.
    /// </remarks>
    public IList<Published> GetDerivedFromPublished(int workId) {
      var w = GetWork(workId);
      var associatedWithPublished = new List<Published>();
      foreach (var f in w.Fragments) {
        Ctx.LoadProperty(f, e => e.FrozenFragment);
        Ctx.LoadProperty(f.FrozenFragment, e => e.Published);
        var p = f.FrozenFragment.Published;
        // p might be null if the work was not published (e.g. imported by user or generic work)
        if (p != null && !associatedWithPublished.Contains(p)) {
          associatedWithPublished.Add(p);
        }
      }
      return associatedWithPublished;
    }


    public Work GetWork(int id) {
      return Ctx.Works.Find(id);
    }

    public Work GetWork(int id, bool includeExternal) {
      if (!includeExternal) return GetWork(id);
      return Ctx.Works
        .Include(w => w.ExternalBook)
        .FirstOrDefault(w => w.Id == id);
    }

    public Work GetWork(int id, string userName) {
      return Ctx.Works
        .Include(w => w.ExternalBook)
        .Include(w => w.Fragments)
        .Include(w => w.Published)
        .FirstOrDefault(w => w.Id == id && w.Owner.UserName == userName);
    }

    public Work EditWork(Work newWork, string userName) {
      var work = GetWork(newWork.Id, userName);
      newWork.CopyProperties<Work>(work,
          w => w.Name,
          w => w.Note,
          w => w.Shared,
          w => w.Public
      );
      // we assume that the user can edit private, custom made works only, hence we treat the work's former content as private
      CopyFragments(work.Fragments, newWork);
      SaveChanges();
      return work;
    }



  }
}