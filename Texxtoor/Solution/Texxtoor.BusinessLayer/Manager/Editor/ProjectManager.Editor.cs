using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.BusinessLayer {
  public partial class ProjectManager {

    public Opus GetDocumentFromSnippetId(int id) {
      var snippet = Ctx.Elements.Find(id);
      Func<Element, Element> findParent = null;
      findParent = e => (e.Parent is Opus) ? e.Parent : findParent(e.Parent);
      return findParent(snippet) as Opus;
    }

    public IEnumerable<KeyValuePair<int, string>> GetSemantics(int opusId, TermType type) {
      var prj = Ctx.Opuses.Find(opusId).Project;
      var select = new Func<Term, KeyValuePair<int, string>>(t => new KeyValuePair<int, string>(t.Id, t.Text));
      // if project is NOT SET for termset it applies for ALL projects
      IEnumerable<KeyValuePair<int, string>> lst = null;
      switch (type) {
        case TermType.Abbreviation:
          lst = Ctx.AbbreviationTerms
            .Include(t => t.TermSets)
            .Include(t => t.TermSets.Select(ts => ts.Project))
            .Where(t => t.Active && t.TermSets.Any(ts => ts.Project.Id == prj.Id))
            .Select(select);
          break;
        case TermType.Cite:
          lst = Ctx.CiteTerms
            .Include(t => t.TermSets)
            .Include(t => t.TermSets.Select(ts => ts.Project))
            .Where(t => t.Active && t.TermSets.Any(ts => ts.Project.Id == prj.Id))
            .Select(select);
          break;
        case TermType.Idiom:
          lst = Ctx.IdiomTerms
            .Include(t => t.TermSets)
            .Include(t => t.TermSets.Select(ts => ts.Project))
            .Where(t => t.Active && t.TermSets.Any(ts => ts.Project.Id == prj.Id))
            .Select(select);
          break;
        case TermType.Variable:
          lst = Ctx.VariableTerms
            .Include(t => t.TermSets)
            .Include(t => t.TermSets.Select(ts => ts.Project))
            .Where(t => t.Active && t.TermSets.Any(ts => ts.Project.Id == prj.Id))
            .Select(select);
          break;
        case TermType.Definition:
          lst = Ctx.DefinitionTerms
            .Include(t => t.TermSets)
            .Include(t => t.TermSets.Select(ts => ts.Project))
            .Where(t => t.Active && t.TermSets.Any(ts => ts.Project.Id == prj.Id))
            .Select(select);
          break;
        case TermType.Link:
          lst = Ctx.LinkTerms
            .Include(t => t.TermSets)
            .Include(t => t.TermSets.Select(ts => ts.Project))
            .Where(t => t.Active && t.TermSets.Any(ts => ts.Project.Id == prj.Id))
            .Select(select);
          break;
      }
      return lst;
    }

    public SelectList GetSemanticListForDocument(int opusId, TermType typeOfList) {
      var lst = GetSemantics(opusId, typeOfList);
      var sl = new SelectList(lst, "Key", "Value");
      return sl;
    }

    /// <summary>
    /// Get all template groups that the user can select templates from.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GroupKind> GetTemplateGroups() {
      var publishableGroups = Enum.GetNames(typeof(GroupKind))
        .ToList()
        // UIHInt controlles what templates are provided as selectable to the user
        .Where(g => typeof(GroupKind).GetField(g).GetCustomAttributes(typeof(UIHintAttribute), false).OfType<UIHintAttribute>().Any(ui => ui.UIHint == "Publishable"))
        .Select(g => (GroupKind)Enum.Parse(typeof(GroupKind), g));
      return publishableGroups;
    }



  }
}
