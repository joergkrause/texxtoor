using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Functions;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.ViewModels.Common;

namespace Texxtoor.BusinessLayer {

  /// <summary>
  /// Reader / User functions with relation to products.
  /// </summary>
  public partial class ReaderManager : Manager<ReaderManager> {

    private byte[] Personalizer(Work work) {
      // TODO: Create a media specific work
      return null;
    }

    public List<Product> GetAllProducts(string userName) {
        if (string.IsNullOrEmpty(userName))
        {
            return new List<Product>();
        }
      var result = Ctx.Products
        .Include(p => p.Work)
        .Where(p => p.Owner.UserName == userName && p.Work != null)
        .OrderByDescending(p => p.CreatedAt)
        .ToList();
      return result;
    }

    public Product GetOrAddProductForWork(int id, string userName) {
      var work = Ctx.Works.Find(id);
      var user = GetCurrentUser(userName);
      var product = GetAllProducts(userName).FirstOrDefault(p => p.Work.Id == id);
      if (product == null) {
        product = new Product {
          Name = ControllerResources.ReaderManager_GatOrAddProductForWork_Product_ + work.Name,
          Title = work.Name,
          SubTitle = work.Note,
          Proprietor = UserProfileManager.Instance.GetProfile(user.Profile.Id).FullName,
          Issue = work.Published != null ? work.Published.SourceOpus.Version.ToString("{0}.0") : "1.0",
          Work = work,
          Owner = user
        };
        Ctx.Products.Add(product);
        SaveChanges();
      } else {
        product.Work = work;
        SaveChanges();
      }
      return product;
    }

    /// <summary>
    /// Add the product to database and establishes a relation to specified work.
    /// </summary>
    /// <param name="product">Product</param>
    /// <param name="workId">Id of related work</param>
    /// <param name="userName">Name of user the product is assigned to</param>
    public void AddProductForWorkId(Product product, int workId, string userName) {
      // TODO: Check whether the product exists and just change properties
      product.Owner = GetCurrentUser(userName);
      var work = Ctx.Works.Find(workId);
      product.Work = work;
      product.Content = Personalizer(work);
      Ctx.Products.Add(product);
      SaveChanges();
    }

    /// <summary>
    /// Save the product to database, accepts a detached object.
    /// </summary>
    /// <param name="newProduct"></param>
    /// <returns>The attached product object</returns>
    public Product SaveProduct(Product newProduct) {
      var pr = Ctx.Products.Find(newProduct.Id);
      newProduct.CopyProperties<Product>(pr,
        p => p.Name,
        p => p.Colored,
        p => p.Dedication,
        p => p.Issue,
        p => p.Proprietor,
        p => p.SubTitle,
        p => p.Title
        );
      Ctx.SaveChanges();
      return pr;
    }

    public Product DeleteProduct(int id, string userName) {
      var product = Ctx.Products.FirstOrDefault(p => p.Id == id && p.Owner.UserName == userName);
      if (product != null) {
        Ctx.Products.Remove(product);
        Ctx.SaveChanges();
      }
      return product;
    }

    /// <summary>
    /// Returns the groups where the user is owner, admin, or member.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public IQueryable<ReaderGroup> GetMyGroups(string userName) {
      var user = GetCurrentUser(userName);
      var owner = GetGroups(userName);
      var membr = Ctx.ReaderGroups
        .Where(g => g.Members.Contains(user) || g.Admins.Contains(user));
      return owner.Union(membr);
    }

    public IQueryable<ReaderGroup> GetGroups(string userName, bool @public = false) {
      var query = Ctx.ReaderGroups
        .Include(p => p.Members)
        .Where(g => (@public == false && g.Owner.UserName == userName) || (@public == true && g.Public == true));
      return query;
    }

    public IQueryable<ReaderGroup> GetGroups(int groupId) {
      var query = Ctx.ReaderGroups
          .Include(g => g.Members)
          .Where(g => g.Id == groupId);
      return query;
    }

    public void AddGroupForUser(ReaderGroup group, string userName) {
      var user = GetCurrentUser(userName);
      var newGroup = new ReaderGroup();
      group.CopyProperties<ReaderGroup>(newGroup,
        g => g.Public,
        g => g.Name,
        g => g.Themes,
        g => g.Description,
        g => g.Closed);
      newGroup.Admins = new List<User>(new User[] { user });
      newGroup.Members = new List<User>(new User[] { user });
      newGroup.Owner = user;
      Ctx.ReaderGroups.Add(newGroup);
      SaveChanges();
    }

    public ReaderGroup GetGroup(int id, params Expression<Func<ReaderGroup, object>>[] properties) {
      var group = Ctx.ReaderGroups
        .Include(g => g.Members)
        .Include(g => g.Members.Select(m => m.Profile))
        .FirstOrDefault(g => g.Id == id);
      if (@group == null || properties == null) return @group;
      foreach (var property in properties) {
        Ctx.LoadProperty(@group, property);
      }
      return group;
    }

    public void EditGroup(int id, ReaderGroup newGroup) {
      var group = GetGroup(id);
      newGroup.CopyProperties<ReaderGroup>(group,
          g => g.Name,
          g => g.Description,
          g => g.Public,
          g => g.Closed);
      SaveChanges();
    }

    public void DeleteGroup(int id, string userName) {
      var group = Ctx.ReaderGroups.FirstOrDefault(g => g.Id == id && g.Owner.UserName == userName);
      if (group != null) {
        Ctx.ReaderGroups.Remove(group);
        SaveChanges();
      }
    }

    public IEnumerable<QuestionsAnswers> GetAllQuestions(int top = 50) {
      return Ctx.QuestionsAnswers
        .Include(p => p.Work)
        .OrderByDescending(qa => qa.ModifiedAt)
        .Take(top);
    }

    /// <summary>
    /// Return all top level questions a user has created and those the user is the author of related works.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="top"></param>
    /// <returns></returns>
    public IDictionary<string, QuestionsAnswers> GetOwnOrAuthorQuestions(string userName, int top = 30) {
      var user = Manager<UserManager>.Instance.GetUserByName(userName);
      var q = Ctx.QuestionsAnswers
        .Include(p => p.Theme)
        .Include(p => p.Work)
        .Include(p => p.Work.Published)
        .Include(p => p.Work.Published.Authors)
        .Where(qa => qa.Owner.Id == user.Id
          || (qa.Work.Published != null && qa.Work.Published.Authors.Any(u => u.Id == user.Id)))
        .OrderByDescending(qa => qa.ModifiedAt)
        .Take(top)
        .ToDictionary(qa => qa.Work.Published.Authors.Any(u => u.Id == user.Id) ? "User" : "Author", qa => qa);
      return q;
    }

    public IEnumerable<QuestionsAnswers> GetOwnQuestions(string userName, int top = 30) {
      return Ctx.QuestionsAnswers
        .Include(p => p.Theme)
        .Include(p => p.Work)
        .Where(qa => qa.Owner.UserName == userName)
        .OrderByDescending(qa => qa.ModifiedAt)
        .Take(top);
    }

    /// <summary>
    /// Returns the themes for selectlist
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Theme> GetGroupThemes() {
      var themes = Ctx.Themes.ToList();
      return themes;
    }

    public bool RemoveMemberFromGroup(int id, int userId, string userName) {
      var group = Ctx.ReaderGroups
        .Include(g => g.Members)
        .FirstOrDefault(g => g.Id == id && g.Owner.UserName == userName);
      if (group == null) return false;
      var member = group.Members.FirstOrDefault(m => m.Id == userId);
      if (member == null) return false;
      group.Members.Remove(member);
      SaveChanges();
      return true;
    }

    public bool AddMemberToGroup(int id, int userId, string userName) {
      var group = Ctx.ReaderGroups
        .Include(g => g.Members)
        .FirstOrDefault(g => g.Id == id && g.Owner.UserName == userName);
      if (group == null) return false;
      var member = group.Members.FirstOrDefault(m => m.Id == userId);
      if (member != null) return false; // exists
      member = Ctx.Users.Find(userId);
      group.Members.Add(member);
      SaveChanges();
      return true;
    }

    public bool AddMemberToGroup(int id, int[] userIds, string userName) {
      var group = Ctx.ReaderGroups
        .Include(g => g.Members)
        .FirstOrDefault(g => g.Id == id && g.Owner.UserName == userName);
      if (group == null) return false;
      var existing = group.Members.Where(m => userIds.Any(u => u == m.Id)).Select(u => u.Id);
      var missing = userIds.Except(existing);
      foreach (var member in Ctx.Users.Where(m => userIds.Any(u => u == m.Id))) {
        @group.Members.Add(member);
      }
      SaveChanges();
      return true;
    }

    public void AddQuestion(QuestionsAnswers qanda, int workId, string userName) {
      var user = GetCurrentUser(userName);
      var work = GetWork(workId);
      // default after question is created
      qanda.Type = QandAType.Question;
      qanda.Owner = user;
      qanda.OrderNr = 0;
      qanda.Work = work;
      Ctx.QuestionsAnswers.Add(qanda);
      SaveChanges();
    }

    /// <summary>
    /// Change an existing question.
    /// </summary>
    /// <param name="newQandA"></param>
    public void EditQuestion(QuestionsAnswers newQandA) {
      var oldQandA = GetQuestion(newQandA.Id);
      newQandA.CopyProperties<QuestionsAnswers>(oldQandA,
        q => q.Subject, q => q.Content, q => q.Mood, q => q.Type);
      Ctx.SaveChanges();
    }

    /// <summary>
    /// Return all published work from catalog, retrieve all children if a parent category is used.
    /// </summary>
    /// <param name="catId"></param>
    /// <returns></returns>
    public IEnumerable<Published> GetPublished(int catId) {
      var parent = Ctx.Catalog.FirstOrDefault(c => c.Id == catId);
      IEnumerable<Published> query;
      if (parent != null && parent.HasChildren()) {
        var catHierarchy = new List<int>();
        Action<Catalog> getCat = null;
        getCat = c => {
          if (c.HasChildren()) {
            c.Children.ForEach(getCat);
          }
          catHierarchy.Add(c.Id);
        };
        getCat(parent);
        // parent cat, search all children
        query = Ctx.Catalog
          .Include(p => p.Published)
          .Include(p => p.Published.Select(m => m.Marketing))
          .Include(p => p.Published.Select(c => c.Catalogs)) //back ref for counting          
          .Where(c => catHierarchy.Contains(c.Id))
          .SelectMany(c => c.Published)
          .Where(p => p.FrozenFragments.Any())
          .OrderBy(c => c.Title);
      } else {
        query = Ctx.Catalog
          .Include(p => p.Published)
          .Include(p => p.Published.Select(m => m.Marketing))
          .Where(c => c.Id == catId || catId == -1)
          .SelectMany(c => c.Published)
          .Where(p => p.FrozenFragments.Any())
          .OrderBy(p => p.Title);
      }
      query.ForEach(p => p.IsRecommendation = false);
      query.ForEach(p => Ctx.LoadProperty(p, m => m.Marketing));
      return query;
    }

    /// <summary>
    /// Return list of published work using distinct search criteria.
    /// </summary>
    /// <param name="profiSearch">Is extended search, using more parameters.</param>
    /// <param name="auditing">Books with auditing (peer review) only.</param>
    /// <param name="auditinglevel">Minimum review level</param>
    /// <param name="rating">Minimum rating.</param>
    /// <returns></returns>
    public List<Published> GetPublished(bool profiSearch, string search, bool? auditing, int? auditinglevel, int? rating) {
      var auditingHasValue = auditing.HasValue;
      var auditinglevelHasValue = auditinglevel.HasValue;
      var ratingHasValue = rating.HasValue;

      var auditingValue = auditingHasValue && auditing.Value;
      var auditinglevelValue = !auditinglevelHasValue ? 0 : auditinglevel.Value;
      var ratingValue = !ratingHasValue ? 0 : rating.Value;
      var query = Ctx.Published
          .Include(p => p.Catalogs)
          .ToList()
          .Where(p => p.Title.ToLower().Contains(search.ToLower()) || (p.SubTitle != null && p.SubTitle.ToLower().Contains(search.ToLower()))
            && !profiSearch
            || (
                (profiSearch && (!auditingHasValue) || p.Reviews.OfType<PeerReview>().Any())
                && (profiSearch && (!auditinglevelHasValue || !auditingHasValue) || p.Reviews.OfType<PeerReview>().Any(r => r.Level >= auditinglevelValue))
                && (profiSearch && (!ratingHasValue) || p.Rating >= ratingValue || p.Rating == 0)
                )
            )
          .Select(p => p).ToList();
      return query;
    }

    /// <summary>
    /// Removes a book from the user's library and returns this books name or <c>null</c> if nothing found.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public string DeleteBookFromLibrary(int id, string userName) {
      var work = Ctx.Works.FirstOrDefault(w => w.Id == id && w.Owner.UserName == userName);
      if (work != null) {
        // check constraints
        var refProduct = Ctx.Products.FirstOrDefault(op => op.Work.Id == work.Id);
        if (refProduct == null) {
          var name = work.Name;
          Ctx.Works.Remove(work);
          SaveChanges();
          return name;
        }
      }
      return null;
    }

    /// <summary>
    /// Delete all comments with the given id list.
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="userName"></param>
    public int DeleteComments(List<int> ids, string userName) {
      var comments = Ctx.Comments.ToList().Where(c => ids.Any(i => i == c.Id) && c.Owner.UserName == userName).ToList();
      var result = comments.Count;
      comments.ForEach(c => Ctx.Comments.Remove(c));
      SaveChanges();
      return result;
    }

    /// <summary>
    /// Check whether a user's book has comments in it.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public bool BookHasComments(int id, string userName) {
      var result = Ctx.Comments
        .Include(p => p.Work)
        // only own comments, those with tags ':', and only the parent entries
        .Any(b => b.Work.Id == id && b.Owner.UserName == userName && b.Subject.Contains(":") && b.Parent == null);
      return result;
    }

    public IDictionary<string, List<Comment>> GetAllComments(int id, string userName) {
      var comments = Ctx.Comments
        .Include(p => p.Work)
        // only own comments, those with tags ':', and only the parent entries
        .Where(b => b.Work.Id == id && b.Owner.UserName == userName && b.Subject.Contains(":") && b.Parent == null)
        .ToList()
        .GroupBy(c => c.Subject.Split(':')[0]).ToDictionary(c => c.Key, c => c.ToList());
      return comments;
    }

    /// <summary>
    /// Removes a fragment from a collection, including all child fragments.
    /// </summary>
    /// <param name="workId"></param>
    /// <param name="fragmentId"></param>
    public void RemoveFragmentIfExists(int workId, int fragmentId) {
      // all fragments
      var fragment = Ctx.WorkingFragments
          .Include(p => p.Work)
          .FirstOrDefault(f => f.Id == fragmentId && f.Work.Id == workId);
      if (fragment != null) {
        if (fragment.HasChildren()) {
          var fs = new List<WorkingFragment>();
          FlatFragmentsForDeletion(fs, fragment.Children);
          fs.Reverse();
          foreach (var f in fs) {
            Ctx.WorkingFragments.Remove(f);
          }
        }
        Ctx.WorkingFragments.Remove(fragment);
        SaveChanges();
      }
    }

    // get a flat copy of the fragment tree for delete purpose
    private void FlatFragmentsForDeletion(IList<WorkingFragment> fs, IEnumerable<WorkingFragment> children) {
      foreach (var fragment in children) {
        fs.Add(fragment);
        if (fragment.Children != null && fragment.Children.Count > 0) {
          FlatFragmentsForDeletion(fs, fragment.Children);
        }
      }
    }

    // get complete fragment tree
    private JsTreeModel[] GetFragmentTreeModel(IList<WorkingFragment> fragments) {
      if (fragments == null || !fragments.Any()) return null;
      var ls = new List<JsTreeModel>();
      foreach (var n in fragments) {
        ls.Add(new JsTreeModel {
          data = n.Name.Ellipsis(50).ToHtmlString(),
          attr = new JsTreeAttribute {
            id = n.Id.ToString(),
            rel = (n.Children != null && n.Children.Any()) ? "folder" : "file"
          },
          children = GetFragmentTreeModel(n.Children)
        });
      }
      return ls.ToArray();
    }

    private JsTreeModel[] GetManifestTreeModel(IEnumerable<FrozenFragment> fragments, int limitLevel) {
      if (fragments == null || !fragments.Any()) return null;
      var ls = new List<JsTreeModel>();
      foreach (var n in fragments) {
        // limit the level of TOC
        if (n.Level < limitLevel) {
          ls.Add(new JsTreeModel {
            data = n.Name,
            attr = new JsTreeAttribute {
              id = n.Id.ToString(CultureInfo.InvariantCulture),
              rel = (n.HasChildren()) ? "folder" : "file"
            },
            children = GetManifestTreeModel(n.Children, limitLevel)
          });
        }
      }
      return ls.ToArray();
    }

    public JsTreeModel[] GetFragmentTreeForPublished(int id, int limitLevel) {
      JsTreeModel[] tree;
      var publ = Ctx.Published.Find(id);
      if (publ != null) {
        var query = publ.FrozenFragments
          .Where(f => f.Parent == null)
          .OrderBy(f => f.OrderNr);
        tree = GetManifestTreeModel(query, limitLevel);
        if (tree == null || tree.Length == 0) {
          return new[] { new JsTreeModel { attr = new JsTreeAttribute { id = "0", rel = "folder" }, children = null, data = publ.Title + " (empty)" } };
        }
        tree = new[] { new JsTreeModel { attr = new JsTreeAttribute { id = "0", rel = "folder" }, children = tree, data = publ.Title } };
      } else {
        return new[] { new JsTreeModel { attr = new JsTreeAttribute { id = "0", rel = "folder" }, children = null, data = ControllerResources.ReaderManager_GetFragmentTree_Empty_Book } };
      }
      return tree;
    }

    public JsTreeModel[] GetFragmentTreeForWork(int id, int limitLevel) {
      JsTreeModel[] tree;
      var work = Ctx.Works.Find(id);
      if (work != null) {
        var query = work.Fragments.OrderBy(f => f.OrderNr).Select(f => f.FrozenFragment);
        tree = GetManifestTreeModel(query, limitLevel);
        if (tree == null || tree.Length == 0) {
          return new[] { new JsTreeModel { attr = new JsTreeAttribute { id = "0", rel = "folder" }, children = null, data = work.Name + " (empty)" } };
        }
        tree = new[] { new JsTreeModel { attr = new JsTreeAttribute { id = "0", rel = "folder" }, children = tree, data = work.Name } };
      } else {
        return new[] { new JsTreeModel { attr = new JsTreeAttribute { id = "0", rel = "folder" }, children = null, data = ControllerResources.ReaderManager_GetFragmentTree_Empty_Book } };
      }
      return tree;
    }

    public Product GetProduct(int productId) {
      return Ctx.Products.Find(productId);
    }

    public void CreateMessage(WorkChat msg, int publishedId, int? parentId, string userName) {
      msg.Owner = Manager<UserManager>.Instance.GetCurrentUser(userName);
      msg.Work = Ctx.Works.Find(publishedId);
      if (parentId.HasValue) {
        msg.Parent = Ctx.WorkChats.Find(parentId.Value);
      }
      Ctx.WorkChats.Add(msg);
      SaveChanges();
    }

    public IEnumerable<WorkChat> GetTopMessages(int workId) {
      var msg = Ctx.WorkChats
        .Include(w => w.Owner)
        .Where(w => w.Work.Id == workId && w.Parent == null)
        .OrderByDescending(w => w.CreatedAt);
      return msg;
    }

    public IEnumerable<Bookmark> GetBookmarksForWork(int id, string userName) {
      return Ctx.Bookmarks
          .Include(b => b.Work)
          .Where(b => b.Work.Id == id && b.Owner.UserName == userName)
          .ToList();
    }

    public List<Catalog> GetCatalog(bool parentOnly, string filter, string lang = null) {
      var cl = new CultureInfo(lang ?? CurrentCulture);
      var cp = cl.Parent.TwoLetterISOLanguageName == "iv" ? cl.TwoLetterISOLanguageName : cl.Parent.TwoLetterISOLanguageName;
      return Ctx.Catalog
        .Where(c => c.LocaleId == cp || lang == null)
        .Where(c => c.Name.Contains(filter) || (parentOnly && c.Parent == null))
        .OrderBy(c => c.OrderNr)
        .ToList();
    }

    public IEnumerable<Published> GetRecommendations(string userName, params IEnumerable<ConsumerMatrix>[] matrix) {
      IEnumerable<ConsumerMatrix> resultMatrix = new List<ConsumerMatrix>();
      resultMatrix = matrix.Where(m => m != null).Aggregate(resultMatrix, (current, m) => current.Union(m));
      // We're looking for matches in the Element table
      var matchElements = Ctx.ElementMatrix
        .Include(em => em.Element)
        .ToList()
        .Where(em => resultMatrix.Any(r => r.Keyword.ToLower().Contains(em.Keyword.ToLower())))
        .Where(em => resultMatrix.Any(r => r.Stage == em.Stage))
        .Where(em => resultMatrix.Any(r => r.Target == em.Target))
        .Distinct()
        .Select(em => em.Element.Id);
      if (!matchElements.Any()) {
        matchElements = Ctx.ElementMatrix
        .Include(em => em.Element)
        .ToList()
        .Where(em => resultMatrix.Any(r => r.Keyword.ToLower().Contains(em.Keyword.ToLower())))
        .Where(em => resultMatrix.Any(r => r.Target == em.Target))
        .Distinct()
        .Select(em => em.Element.Id);
      }
      if (!matchElements.Any()) {
        matchElements = Ctx.ElementMatrix
        .Include(em => em.Element)
        .ToList()
        .Where(em => resultMatrix.Any(r => r.Keyword.ToLower().Contains(em.Keyword.ToLower())))
        .Distinct()
        .Select(em => em.Element.Id);
      }
      // check whether the matching elements have been published earlier
      // TODO: by taking the ElementMatrix approach directly we can give hints for "work in progress"
      // catalogs any assures that work not filed in catalogies, such as RSS and articles, do not appear here
      var p = Ctx.Published
        .Include(publ => publ.Catalogs)
        .Include(publ => publ.Marketing)
        .Where(publ => matchElements.Any(me => me == publ.SourceOpus.Id) && publ.Catalogs.Any());
      //if (!p.Any()) {
      //  p = Ctx.Published
      //    .Include(publ => publ.Catalogs)
      //    .Include(publ => publ.Marketing)
      //    .Where(publ => publ.Catalogs.Any() && publ.FrozenFragments.Any()) // FrozenFragments counts zero for accidentially published content
      //    .Take(5);
      //}
      p.ForEach(r => r.IsRecommendation = true);
      return p;
    }

    public Published GetPublishedById(int id, params Expression<Func<Published, object>>[] expressions) {
      var model = Ctx.Published.Find(id);
      if (expressions != null) {
        Ctx.LoadProperty(model, expressions);
      }
      return model;
    }

    public Catalog GetCatalog(int? category) {
      return Ctx.Catalog.FirstOrDefault(c => (c.Id == category));
    }


    public void AddComment(Comment newComment, int? parentCommentGroup, string userName) {
      if (parentCommentGroup.HasValue) {
        var parentComment = Ctx.Comments.Find(parentCommentGroup.Value);
        newComment.Parent = parentComment;
      }
      newComment.Owner = GetCurrentUser(userName);
      Ctx.Comments.Add(newComment);
      SaveChanges();
    }

    public void EditComment(Comment comment, string userName)
    {
      var comm = Ctx.Comments.SingleOrDefault(c => c.Id == comment.Id && c.Owner.UserName == userName);
      if (comm != null)
      {
        comment.CopyProperties<Comment>(comm, 
          c => c.Level,
          c => c.Name,
          c => c.Private,
          c => c.Subject,
          c => c.Tags
          );
        SaveChanges();
      }
    }

    public IEnumerable<Comment> GetComments(string fragmentName, string userName) {
      return Ctx.Comments
        .Include(p => p.Owner)
        .Where(c => c.CfiRef == fragmentName                   // one fragment only
          && c.Parent == null                                  // root only, treeview is pulling the rest
          && (!c.Private || c.Owner.UserName == userName));    // either public or current user's private stuff
    }


    public string CreateReaderServiceSession(string userName) {
      string currentSessionId = Guid.NewGuid().ToString();
      User user = GetCurrentUser(userName);
      var currentSession = new Session {
        User = user,
        SessionId = currentSessionId,
        SessionData = new Texxtoor.DataModels.ServiceModel.SessionData { UserId = (user == null ? -1 : user.Id) }.ToString()
      };
      Ctx.Sessions.Add(currentSession);
      SaveChanges();
      return currentSessionId;
    }

  }
}