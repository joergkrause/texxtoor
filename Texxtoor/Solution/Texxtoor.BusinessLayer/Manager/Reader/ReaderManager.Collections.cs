using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Functions;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.BusinessLayer {

  /// <summary>
  /// Reader / User functions with relation to products.
  /// </summary>
  public partial class ReaderManager {

    # region Works and Fragment Collections

    /// <summary>
    /// Retrieve all books in the users private library.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public IQueryable<Work> GetWorksForUser(string userName) {
      var works = Ctx.Works
        .Include(w => w.ExternalBook)
        .Include(w => w.Products)
        .Include(w => w.Published)
        .Include(w => w.Fragments)
        .Where(c => c.Owner.UserName == userName)
        .OrderByDescending(x => x.CreatedAt)
        .ThenBy(x => x.Name.ToLower());
      return works;
    }

    private static List<WorkingFragment> CreateFlatReverseTree(IEnumerable<WorkingFragment> fragments) {
      var reverse = new List<WorkingFragment>();
      foreach (var item in fragments) {
        if (item.HasChildren()) {
          var children = CreateFlatReverseTree(item.Children);
          reverse.AddRange(children);
        } else {
          reverse.Add(item);
        }
      }
      return reverse;
    }

    public WorkingFragment AddFragmentToWork(int workId, int frozenFragmentId, string userName) {
      var frozenFragment = Ctx.FrozenFragments.Find(frozenFragmentId);
      var work = Ctx.Works
        .Include(p => p.Fragments)
        .Single(c => c.Id == workId);
      // adding always at the end
      var lastOrder = (work.Fragments.Any()) ? work.Fragments.Max(f => f.OrderNr) : 0;

      // create new fragment as a clone of the existing one
      var workingFragment = new WorkingFragment {
        FrozenFragment = frozenFragment,
        Name = frozenFragment.Name,
        OrderNr = lastOrder,
        Work = work
      };
      // look for children, e.g. images or other resources
      if (frozenFragment.HasChildren()) {
        BuildChildFragments(frozenFragment, workingFragment);
      }
      Ctx.WorkingFragments.Add(workingFragment);
      work.Fragments.Add(workingFragment);
      SaveChanges();
      return workingFragment;
    }

    /// <summary>
    /// This functions takes an orderd list of frozenfragment id's and assigns the values as new workingfragments to the work.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="fragmentId"></param>
    /// <param name="userName"></param>
    public void AssignFragmentsToWork(int id, List<int> fragmentId, string userName) {
      using (var scope = Ctx.BeginTransaction()) {
        var targetWork = Ctx.Works
          .Include(w => w.Fragments)
          .FirstOrDefault(c => c.Id == id && c.Owner.UserName == userName);
        if (targetWork == null)
          throw new ArgumentNullException("id", "Work does not exists");
        if (targetWork.Extern != WorkType.Custom)
          throw new ArgumentOutOfRangeException("id", "Only custom work can be changed");
        // take the current fragment list's frozen fragments
        var current = targetWork.Fragments.Select(f => f.FrozenFragment);
        // clear the working fragments, we're going to rebuild the whole list
        foreach (var i in targetWork.Fragments.Select(f => f.Id).ToList()) {
          var wf = Ctx.WorkingFragments.Find(i);
          Ctx.WorkingFragments.Remove(wf);
        }
        SaveChanges();
        // 2. Add all as new fragments and keep order
        var orderNr = 1;
        foreach (var i in fragmentId) {
          var wf = AddFragmentToWork(targetWork.Id, i, userName);
          wf.OrderNr = orderNr++;
        }
        SaveChanges();
        scope.Commit();
      }
    }


    // build a flat list of children from the navigation hierarchy
    private void BuildChildFragments(FrozenFragment sourceFragment, WorkingFragment targetFragment) {
      if (!sourceFragment.HasChildren()) return;
      targetFragment.Children = new List<WorkingFragment>();
      var lastOrder = 1;
      foreach (var frozenFragment in sourceFragment.Children) {
        var newFragment = new WorkingFragment {
          FrozenFragment = frozenFragment,
          Name = frozenFragment.Name,
          OrderNr  = lastOrder++,
          Work = targetFragment.Work,
          Parent = targetFragment          
        };
        targetFragment.Children.Add(newFragment);
        // loop recursively through all levels and add them
        if (frozenFragment.HasChildren()) {
          BuildChildFragments(frozenFragment, newFragment);
        }
      }
    }

    # endregion

    /// <summary>
    /// Copy a work into a new one.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userName"></param>
    public Work CopyWork(int id, string userName) {
      using (var scope = Ctx.BeginTransaction()) {
        var fromWork = Ctx.Works
          .Include(w => w.ExternalBook)
          .Include(w => w.Published)
          .FirstOrDefault(w => w.Id == id && w.Owner.UserName == userName);
        // Do not copy external books
        if (fromWork == null || fromWork.Extern == WorkType.External) return fromWork;
        var newWork = new Work {
          Name = String.Format(ControllerResources.ReaderManager_CopyWork_Copy_from, fromWork.Name),
          Note = fromWork.Note,
          Public = fromWork.Public,
          Shared = fromWork.Shared,
          Owner = fromWork.Owner,
        };
        CopyFragments(fromWork.Fragments, newWork);
        Ctx.Works.Add(newWork);
        Ctx.SaveChanges();
        scope.Commit();
        return newWork;
      }
    }

    private void CopyFragments(IEnumerable<WorkingFragment> source, Work target) {
      if (source == null || !source.Any()) return;
      if (target.Fragments == null) {
        target.Fragments = new List<WorkingFragment>();
      }
      target.Fragments.Clear();
      // copy recursively
      Func<IEnumerable<WorkingFragment>, IEnumerable<WorkingFragment>> copyChildFragments = null;
      copyChildFragments = (children) => {
        var currentFragmentList = new List<WorkingFragment>();
        foreach (var workingFragment in children) {
          var child = new WorkingFragment {
            FrozenFragment = workingFragment.FrozenFragment,
            Name = workingFragment.Name,
            Work = target,
            OrderNr = workingFragment.OrderNr,
          };
          if (workingFragment.HasChildren()) {
            child.Children = copyChildFragments(workingFragment.Children).ToList();
          }
          currentFragmentList.Add(child);
        }
        return currentFragmentList;
      };
      var topFragments = copyChildFragments(source).ToList();
      topFragments.ForEach(f => target.Fragments.Add(f));
    }

    public IEnumerable<ConsumerMatrix> GetConsumerMatrix(string userName, bool temporary = true) {
      var userProfile = Ctx.UserProfiles
        .Include(p => p.ConsumerMatrix)
        .FirstOrDefault(p => p.User.UserName == userName);
      return userProfile != null ? userProfile.ConsumerMatrix.Where(m => m.Temporary == temporary) : null;
    }

    public void RemoveConsumerMatrix(int matrixId, string userName) {
      var matrix =
        Ctx.ConsumerMatrix.SingleOrDefault(m => m.Id == matrixId && m.Temporary && m.Profile.User.UserName == userName);
      if (matrix != null) {
        Ctx.ConsumerMatrix.Remove(matrix);
        SaveChanges();
      }
    }

    public int AddConsumerMatrix(ConsumerMatrix matrix, string userName) {
      var userProfile = Ctx.UserProfiles
        .Include(p => p.ConsumerMatrix)
        .FirstOrDefault(p => p.User.UserName == userName);
      if (userProfile == null) return 0;
      matrix.Profile = userProfile;
      matrix.Temporary = true;
      if (
        !userProfile.ConsumerMatrix.Any(
          cm => cm.Keyword == matrix.Keyword && cm.Stage == matrix.Stage && cm.Target == matrix.Target)) {
        Ctx.ConsumerMatrix.Add(matrix);
      }
      SaveChanges();
      return matrix.Id;
    }

    public QuestionsAnswers GetQuestion(int id) {
      return Ctx.QuestionsAnswers.Include(q => q.Work).FirstOrDefault(q => q.Id == id);
    }

    public void SetRating(int id, int v, string userName) {
      var published = Ctx.Published.Find(id);
      published.Starred = published.Starred + v / 2;
      published.Rating = published.Rating + (v * 10) / 2;
      SaveChanges();
    }

    internal IEnumerable<Published> GetAllPublished(bool quick = false) {
      return quick ? Ctx.Published.Where(p => !p.Catalogs.Any()) : Ctx.Published.Where(p => p.Catalogs.Any());
    }

    /// <summary>
    /// Returns all works for users, which is useful to manage shared works within groups.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="shared"></param>
    /// <returns></returns>
    public IEnumerable<Work> GetWorksForUsers(IList<User> list, bool shared) {
      if (list == null || !list.Any()) return null;
      var works = Ctx.Works
        .Where(w => w.Fragments.Any() && w.Shared == shared)
        .OrderBy(w => w.Owner.UserName)
        .ThenByDescending(w => w.CreatedAt)
        .ToList()
        .Where(w => (w.Extern == WorkType.Custom || w.Extern == WorkType.Published) && list.Any(l => l.UserName == w.Owner.UserName))
        .AsEnumerable();
      return works;
    }

    public FrozenFragment GetFrozenFragment(int id) {
      return Ctx.FrozenFragments.Find(id);
    }

    public byte[] GetFrozenFragmentHref(string href) {
      var fragment = Ctx.FrozenFragments.SingleOrDefault(f => f.ItemHref == href);
      return fragment != null ? fragment.Content : null;
    }

    public void RegisterSales(int id){
      var work = GetWork(id);
      if (work.Public && work.Published != null) {
        // sales statistics for authors, this is always a free service so he owns nothing
        // if we provide ads supported pages, this amount of clicks will pay out anyway
        var authors = work.Published.Authors;
        foreach (var author in authors) {
          Ctx.Sales.Add(new Sale {
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
    }

    
  }
}