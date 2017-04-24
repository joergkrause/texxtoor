using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Logging;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels;

namespace Texxtoor.BusinessLayer {

  /// <summary>
  /// All BLL functions for projects.
  /// </summary>
  public partial class ProjectManager : Manager<ProjectManager> {

    # region Opus Functions

    public IQueryable<Opus> GetAllOpusForUser(string userName, bool closed) {
      var opus = Ctx.Projects
        .Include(p => p.Team)
        .Include(p => p.Team.Members)
        .Include(p => p.Team.Members.Select(t => t.Member))
        .Where(p => p.Active != closed) // only active        
        .Where(p => p.Team.Members.Any(m => m.Member.UserName == userName)) // only where user is member
        .SelectMany(p => p.Opuses) // all opus from all projects      
        .Where(o => o.Active != closed)
        .OrderByDescending(o => o.ModifiedAt)
        .AsQueryable();
      return opus;
    }

    public Opus GetAndActivateOpus(int opusId, bool activate) {
      try {
        var opus = Ctx.Opuses.Find(opusId);
        var prj = Ctx.Projects.Find(opus.Project.Id);
        opus.Active = activate;
        opus.Project = prj;
        SaveChanges();
        return opus;
      } catch (Exception ex) {
        Logger.Error(ex, "ProjectManager(GetAndActivateOpus)", opusId, activate);
        throw;
      }
    }

    public Opus CreateOpusFromExisting(int opusId, bool copyContent, bool withMilestones = true) {
      var proxy = Ctx.Configuration.ProxyCreationEnabled;
      using (var scope = Ctx.BeginTransaction()) {
        try {
          Ctx.Configuration.ProxyCreationEnabled = true;
          var opus = Ctx.Opuses.Find(opusId);
          Opus newOpus;
          if (copyContent) {
            newOpus = CopyElementTree<Opus>(opus);
          } else {
            newOpus  =new Opus();
            opus.CopyProperties<Opus>(newOpus,
              o => o.LocaleId,
              o => o.Name,
              o => o.Properties,
              o => o.Project);
            // add at least one chapter to make a valid document
            opus.Children = new List<Element>
            {
              new Section
              {
                Name = ControllerResources.ProjectManager_CreateFromTemplate_First_Chapter,
                LocaleId = opus.LocaleId,
                OrderNr = 1,
                Content = Encoding.UTF8.GetBytes(ControllerResources.ProjectManager_CreateFromTemplate_First_Chapter),
                Parent = opus
              }
            };
          }
          newOpus.Version = opus.Version + 1;
          newOpus.OrderNr = opus.OrderNr + 1;
          newOpus.PreviousVersion = opus;
          newOpus.Variation = VariationType.Continuation;
          var mstn = opus.Milestones.ToList();
          Ctx.Opuses.Add(newOpus);
          if (withMilestones) {
            foreach (var milestone in mstn) {
              var ms = new Milestone {
                Name = milestone.Name,
                Description = milestone.Description,
                Opus = newOpus,
                Owner = milestone.Owner,
                DateAssigned = DateTime.Now,
                DateDue = DateTime.Now.Add(milestone.DateDue.Subtract(milestone.DateAssigned))
              };
              Ctx.Milestones.Add(ms);
            }
          }
          SaveChanges();
          scope.Commit();
          return newOpus;
        } catch (Exception ex) {
          scope.Rollback();
          Logger.Error(ex, "ProjectManager(CreateOpusFromExisting)", opusId);
          throw;
        } finally {
          Ctx.Configuration.ProxyCreationEnabled = proxy;
        }
      }
    }

    /// <summary>
    /// Copy thet startElement with all children into the target. If target is <c>null</c> we expect the start is an opus and copy the whole opus.
    /// </summary>
    /// <param name="startElement"></param>
    /// <param name="targetElement"></param>
    private T CopyElementTree<T>(T startElement, Element targetElement = null) where T : Element {
      Trace.WriteLine("Create new Opus by copying one");
      if (targetElement == null && !(startElement is Opus))
      {
        throw new ArgumentOutOfRangeException("startElement", "targetElement is null and startElement");
      }
      try
      {
        T clone = null;
        using (var scope = Ctx.BeginTransaction() ) {
          // get copies to have it in new context
          var element = Ctx.Elements.Find(startElement.Id);
          var target = targetElement != null ? Ctx.Elements.Find(targetElement.Id) : null;
          // only if success
          if (element == null) return null;
          var notTracked = Ctx.Elements.AsNoTracking();
          clone = Ctx.Elements.AsNoTracking().OfType<T>().Single(o => o.Id == element.Id);
          // re-apply all dependend objects
          var opus = clone as Opus;
          if (opus != null) {
            opus.Project = Ctx.Projects.Find(((Opus)element).Project.Id);
          }
          Func<IEnumerable<Element>, List<Element>> cloneChildren = null;
          // recursively loop through 
          cloneChildren = sourceChildren => {
            var sourceChildIds = sourceChildren.Select(c => c.Id);
            var clonedChildren = notTracked.Where(e => sourceChildIds.Any(c => c == e.Id)).ToList();
            foreach (var clonedChild in clonedChildren) {
              if (clonedChild.HasChildren()) {
                clonedChild.Children = cloneChildren(clonedChild.Children);
              }
            }
            return clonedChildren;
          };
          // deep copy of all children
          clone.Children = cloneChildren(element.Children);
          // store a reference to the predecessor
          var snippet = clone as Snippet;
          if (snippet != null)
          {
            snippet.Predecessor = element as Snippet;
          }
          // register predecessor and copy at root level
          if (opus != null) {
            Ctx.Elements.Add(opus);
            opus.PreviousVersion = (Opus)element;
          } else {
            // if not at root we need a target
            if (target != null) {
              if (target.Children == null) {
                target.Children = new List<Element>();
              }
              // add to tracker and create graph
              Ctx.Elements.Add(clone);
              target.Children.Add(clone);
            }
          }
          SaveChanges();
          scope.Commit();
        }
        return clone;
      } catch (DbEntityValidationException ex)
      {
        Logger.Error(ex, "ProjectManager");
        throw;
      }
    }


    private void DeepCloneRecursively(IEnumerable<Element> children, IDictionary<Element, int> newParents, bool keepRelation = false) {
      foreach (var element in children) {
        if (element.HasChildren()) {
          DeepCloneRecursively(element.Children.ToList(), newParents, keepRelation);
        }
        // we start with the deepest element        
        var oldParentId = (element.Parent == null) ? 0 : element.Parent.Id;
        // as we want to clone we opt tracking out
        var sourceElement = Ctx.Elements.OfType<Snippet>().AsNoTracking().Single(e => e.Id == element.Id);
        // re-parent
        newParents.Add(element, oldParentId);
        // create clone
        var targetElement = Ctx.Elements.Add(sourceElement);
        // if relation has to be keep
        if (keepRelation && element is Snippet) {
          ((Snippet)targetElement).Predecessor = sourceElement;
        }
      }
    }

    private void FlatElementList(IEnumerable<Element> children, IList<Element> flatList) {
      foreach (var element in children) {
        flatList.Add(element);
        if (element.HasChildren()) {
          FlatElementList(element.Children.ToList(), flatList);
        }        
      }
    }

    public SelectList GetSemanticListForOpus(int opusId, string typeOfList) {
      var ops = Ctx.Opuses.Find(opusId);
      IEnumerable<KeyValuePair<int, string>> lst = null;
      var select = new Func<Term, KeyValuePair<int, string>>(t => new KeyValuePair<int, string>(t.Id, t.Content));
      // if project is NOT SET for termset it applies for ALL projects
      var where = new Func<Term, bool>(t => t.TermSets.Any(ts => ts.Project == null || ts.Project.Id == ops.Project.Id) && t.LocaleId == ops.LocaleId || ops.LocaleId == null);
      switch (typeOfList.ToLowerInvariant()) {
        case "abbreviation":
          lst = Ctx.AbbreviationTerms.Where(where).Select(select);
          break;
        case "cite":
          lst = Ctx.CiteTerms.Where(where).Select(select);
          break;
        case "idiom":
          lst = Ctx.IdiomTerms.Where(where).Select(select);
          break;
        case "variable":
          lst = Ctx.VariableTerms.Where(where).Select(select);
          break;
        case "definition":
          lst = Ctx.DefinitionTerms.Where(where).Select(select);
          break;
        case "link":
          lst = Ctx.DefinitionTerms.Where(where).Select(select);
          break;
      }
      var sl = new SelectList(lst, "Key", "Value");
      return sl;
    }

    public void ChangeMileStone(int mileStoneId, string comment, int progress) {
      try {
        progress = (progress > 100) ? 100 : progress;
        progress = Math.Abs(progress);
        var mstn = Ctx.Milestones
          .Include(p => p.Opus)
          .Include(p => p.Owner)
          .First(m => m.Id == mileStoneId);
        var comments = mstn.Comment == null ? new List<string>() : mstn.Comment.Split('|').ToList();
        comment = comment == null ? String.Format("Set by {0}", GetCurrentUserName()) : comment.Replace('|', ',').Replace('^', ',');
        comments.Add(String.Format("{0}^{1}^{2}^{3}", comment, mstn.Owner.Member.UserName, DateTime.Now, progress));
        mstn.Comment = String.Join("|", comments.ToArray());
        mstn.Progress = progress;
        SaveChanges();
      } catch (Exception ex) {
        Logger.Error(ex, "ProjectManager(ChangeMileStone)", mileStoneId, comment, progress);
        throw;
      }
    }

    public Milestone GetMilestone(int milestoneId) {
      return Ctx.Milestones
        .Include(m => m.NextMilestone)
        .SingleOrDefault(m => m.Id == milestoneId);
    }

    public void MoveMileStoneOrder(int id, string d) {
      var ms = Ctx.Milestones.Find(id);
      var mstn = ms.Opus.Milestones;
      // 'd' ==> move down, 'u' ==> move up
      // TODO: Implement this
    }

    public void AssignMilestone(int id, int member) {
      var ms = Ctx.Milestones.Find(id);
      ms.Owner = Ctx.TeamMembers.Find(member);
      SaveChanges();
    }

    public IQueryable<Milestone> GetMileStonesOfOpus(int opusId) {
      var mstn = Ctx.Milestones
        .Include(p => p.Owner)
        .Include(p => p.Owner.Member)
        .Where(m => m.Opus.Id == opusId);
      return mstn;
    }

    public Opus CreateOpusForProject(int projectId, string name, int? copyId = null, bool? withMilestones = null) {
      var prj = Ctx.Projects
        .Include(p => p.Team)
        .Include(p => p.Team.Members)
        .Include("Team.Members.Role")
        .First(p => p.Id == projectId);
      Opus opus = null;
      if (copyId.HasValue) {
        opus = CreateOpusFromExisting(copyId.Value, true, withMilestones.GetValueOrDefault());
      } else {
        opus = new Opus {
          Name = name,
          Version = 1,
          Project = prj
        };
      }
      var lead = GetTeamLeaderAsMember(prj.Team.Id);
      if (withMilestones.GetValueOrDefault()) {
        var mstn = CreateDefaultMileStones(lead, opus);
        opus.Milestones = mstn;
      }
      Ctx.Opuses.Add(opus);
      SaveChanges();
      return opus;
    }

    public List<Element> GetOpusElementsForPreview(int projectId, int opusId) {
      var prj = Ctx.Projects.Find(projectId);
      Ctx.LoadProperty(prj, "Opuses");
      var opus = prj.Opuses.FirstOrDefault(o => o.Id == opusId);
      // get all fragments in the specified opus on page 
      //Func<Element, List<Element>> flatFunction = e => e.HasChildren() ? e.Children : null;
      var res = Ctx.Elements.Where(e => e.Parent.Id == opus.Id).ToList();
      return res;
    }

    public string GetSnippetsFromOpus(int opusId, Action<Element> getResource) {
      // Base information about a certain opus
      var opus = Ctx.Opuses.Find(opusId);
      Func<List<Element>, string[]> func = null;
      func = e => e
          .OrderBy(s => s.OrderNr)
          .Select(s => String.Concat(CreateDataFragment(s, getResource), (s.HasChildren() ? String.Join(Environment.NewLine, func(s.Children)) : String.Empty)))
          .ToArray();

      if (opus.HasChildren()) {
        return String.Join(Environment.NewLine, func(opus.Children));
      } return null;
    }

    private static string CreateDataFragment(Element fragment, Action<Element> getResource) {
      // identify fragment
      // get attribute
      var content = String.Empty;
      var t = fragment.GetType();
      var a = t.GetCustomAttributes(typeof(SnippetBuilderAttribute), true);
      if (a.Length != 1) return content;
      var elAttribute = ((SnippetBuilderAttribute)a[0]);
      var pattern = elAttribute.HtmlPattern;
      var parameters = elAttribute.Properties;
      // replace the properties names with values
      for (int i = 0; i < parameters.Length; i++) {
        var item = parameters[i];
        var prop = t.GetProperty(item).GetValue(fragment, null);
        if (prop != null) {
          if (prop.GetType().Name.Equals("Byte[]")) {
            parameters[i] = Encoding.UTF8.GetString((byte[])prop);
          } else {
            parameters[i] = prop.ToString();
          }
        } else {
          parameters[i] = String.Empty;
        }
      }
      // set content
      content = String.Format(pattern, parameters);
      // if content is a blob reference it
      if (elAttribute.CreateResource) {
        // return content through callback method along with the written Id
        getResource(fragment);
      }
      return content;
    }

    # endregion
    // TODO: Create a Copy while publishing to save the matrix' relation to frozenfragments
    public void AddMatchMatrixToElement(int opusId) {
      var opus = Ctx.Elements.Find(opusId);
      Ctx.LoadProperty(opus, o => o.MatchMatrix);
      opus.MatchMatrix.Add(new ElementMatrix { Keyword = "New", Stage = StageType.Beginner, Target = TargetType.Professional, Element = opus });
      SaveChanges();
    }

    public void RemoveMatchMatrixFromElement(int matrixId) {
      var mm = Ctx.ElementMatrix.Find(matrixId);
      Ctx.ElementMatrix.Remove(mm);
      SaveChanges();
    }

    public void SaveMatchMatrixToElement(int elementId, int matrixId, string keyword, int targets, int stages) {
      var el = Ctx.Elements.Find(elementId);
      var mm = Ctx.ElementMatrix.Find(matrixId);
      mm.Keyword = keyword;
      mm.Target = (TargetType)targets;
      mm.Stage = (StageType)stages;
      mm.Element = el;
      SaveChanges();
    }

    public void SaveMatchMatrixToElement(int elementId, int[] matrixId, string[] keyword, int[] targets, int[] stages) {
      var el = Ctx.Elements.Find(elementId);
      if (matrixId.Length != keyword.Length || targets.Length != stages.Length || matrixId.Length != stages.Length) {
        throw new ArgumentOutOfRangeException();
      }
      for (int i = 0; i < matrixId.Length; i++) {
        var mm = Ctx.ElementMatrix.Find(matrixId[i]);
        mm.Keyword = keyword[i];
        mm.Target = (TargetType)targets[i];
        mm.Stage = (StageType)stages[i];
        mm.Element = el;
      }
      SaveChanges();
    }

  }
}