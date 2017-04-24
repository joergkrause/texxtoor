using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;

namespace Texxtoor.BusinessLayer {

  /// <summary>
  /// All BLL functions for projects.
  /// </summary>
  public partial class ProjectManager {
        
    # region TermSet Functions

    public TermSet AddTermsetForProject(int projectId, TermSet termSetData) {
      var prj = Ctx.Projects.Find(projectId);
      var ts = new TermSet {
        LocaleId = termSetData.LocaleId,
        Project = prj,
        Name = termSetData.Name,
        Shared = termSetData.Shared,
        Terms = termSetData.Terms,
        Active = termSetData.Active,
        Description = termSetData.Description
      };
      Ctx.TermSets.Add(ts);
      SaveChanges();
      return ts;
    }

    public void EditTermSet(int id, TermSet ts) {
      var prj = Ctx.Projects.Find(id);
      var oldTs = Ctx.TermSets.Find(ts.Id);
      ts.CopyProperties<TermSet>(oldTs,
        t => t.Description,
        t => t.Name,
        t => prj,
        t => t.Shared,
        t => t.Active,
        t => t.LocaleId);
      SaveChanges();
    }

    public TermSet CreateTermsetForProject(int projectId, string localeId) {
      var prj = Ctx.Projects.Find(projectId);
      var ts = new TermSet {
        LocaleId = localeId,
        Project = prj,
        Name = "",
        Shared = true,
        Terms = new List<Term>(),
        Active = true,
        Description = ""
      };
      return ts;
    }

    public IEnumerable<TermSet> GetTermSetsForProject(string localeId, string userName, int? projectId = null) {      
      var user = Ctx.Users.Single(u => u.UserName == userName);
      IEnumerable<TermSet> tsets;
      if (projectId.HasValue) {        
        tsets = Ctx.TermSets
                   .Where(t => t.Project.Id == projectId)
                   .OrderBy(t => t.CreatedAt)
                   .ToList();
      }
      else {
        tsets = Ctx.TermSets
                   .Where(t => t.Project == null && t.Owner.UserName == userName)
                   .OrderBy(t => t.CreatedAt)
                   .ToList();        
      }
      if (!tsets.Any()) {
        // create at least one initial set
        var ts = new TermSet {
          LocaleId = localeId,
          Owner = user,
          Shared = true,
          Terms = new List<Term>(),
          Active = true,
        };
        if (projectId.HasValue) {
          var prjId = projectId.Value;
          var prj = Ctx.Projects.Find(prjId);
          ts.Name = String.Format("{0} ({1})", ControllerResources.ProjectManager_GetTermSetsForProject_Default_Termset, prj.Name).Ellipsis(60).ToString();
          ts.Description = ControllerResources.ProjectManager_GetTermSetsForProject_First_public_termset_for_the_specific_project_;
          ts.Project = prj; // null is acceptable, it's a shared set, then
          tsets = Ctx.TermSets.Where(t => t.Project.Id == prjId).ToList();
        }
        else {
          ts.Name = String.Format("{0}", ControllerResources.ProjectManager_GetTermSetsForProject_Default_Termset);
          ts.Description = ControllerResources.ProjectManager_GetTermSetsForProject_This_is_the_default_termset_that_keeps_all_terms_not_defined_in_specific_termsets;
          ts.Project = null; // null is acceptable, it's a shared set, then
          tsets = Ctx.TermSets.Where(t => t.Project == null && t.Owner.UserName == userName).ToList();
        }
        Ctx.TermSets.Add(ts);
        SaveChanges();
      }
      return tsets;
    }

    public IEnumerable<Term> GetTermsSetOfTypeName(int id, TermType type) {
      var ts = Ctx.TermSets.Find(id);
      IEnumerable<Term> terms = null;
      switch (type) {
        case TermType.Abbreviation:
          terms = ts.Terms.OfType<AbbreviationTerm>();
          break;
        case TermType.Cite:
          terms = ts.Terms.OfType<CiteTerm>();
          break;
        case TermType.Definition:
          terms = ts.Terms.OfType<DefinitionTerm>();
          break;
        case TermType.Idiom:
          terms = ts.Terms.OfType<IdiomTerm>();
          break;
        case TermType.Variable:
          terms = ts.Terms.OfType<VariableTerm>();
          break;
        case TermType.Link:
          terms = ts.Terms.OfType<LinkTerm>();
          break;
      }
      return terms;
    }

    public IEnumerable<Term> AddTermsSetOfTypeName(int id, TermType type, string key, string val, string locale) {
      var ts = Ctx.TermSets.Find(id);
      IEnumerable<Term> terms = null;
      Term term = null;
      switch (type) {
        case TermType.Abbreviation:
          terms = ts.Terms.OfType<AbbreviationTerm>();
          term = new AbbreviationTerm();
          SetTermProperties(term, key, val, locale);
          Ctx.AbbreviationTerms.Add(term as AbbreviationTerm);
          break;
        case TermType.Cite:
          terms = ts.Terms.OfType<CiteTerm>();
          term = new CiteTerm();
          SetTermProperties(term, key, val, locale);
          Ctx.CiteTerms.Add(term as CiteTerm);
          break;
        case TermType.Definition:
          terms = ts.Terms.OfType<DefinitionTerm>();
          term = new DefinitionTerm();
          SetTermProperties(term, key, val, locale);
          Ctx.DefinitionTerms.Add(term as DefinitionTerm);
          break;
        case TermType.Idiom:
          terms = ts.Terms.OfType<IdiomTerm>();
          term = new IdiomTerm();
          SetTermProperties(term, key, val, locale);
          Ctx.IdiomTerms.Add(term as IdiomTerm);
          break;
        case TermType.Variable:
          terms = ts.Terms.OfType<VariableTerm>();
          term = new VariableTerm();
          SetTermProperties(term, key, val, locale);
          Ctx.VariableTerms.Add(term as VariableTerm);
          break;
        case TermType.Link:
          terms = ts.Terms.OfType<LinkTerm>();
          term = new LinkTerm();
          SetTermProperties(term, key, val, locale);
          Ctx.LinkTerms.Add(term as LinkTerm);
          break;
      }
      if (term != null) {
        if (term.TermSets == null) {
          term.TermSets = new List<TermSet>();
        }
        term.TermSets.Add(ts);
        SaveChanges();
      }
      return terms;
    }

    private void SetTermProperties(Term term, string key, string val, string locale) {
      term.Active = true;
      term.Text = key;
      term.Content = val;
      term.LocaleId = locale;
    }

    public void ChangeTermsSetOfTypeName(int id, TermType type, string key, string val, string locale) {
      var term = Ctx.Terms.Find(id);
      term.Text = key;
      term.Content = val;
      if (!string.IsNullOrEmpty(locale)) {
        term.LocaleId = locale;
      }
      SaveChanges();
    }

    public IEnumerable<Term> RemoveTermsSetOfTypeName(int id, TermType type, int key) {
      var ts = Ctx.TermSets.Find(id);
      IEnumerable<Term> terms = null;
      Term term = null;
      switch (type) {
        case TermType.Abbreviation:
          terms = ts.Terms.OfType<AbbreviationTerm>();
          term = terms.FirstOrDefault(t => t.Id == key);
          Ctx.AbbreviationTerms.Remove(term as AbbreviationTerm);
          break;
        case TermType.Cite:
          terms = ts.Terms.OfType<CiteTerm>();
          term = terms.FirstOrDefault(t => t.Id == key);
          Ctx.CiteTerms.Remove(term as CiteTerm);
          break;
        case TermType.Definition:
          terms = ts.Terms.OfType<DefinitionTerm>();
          term = terms.FirstOrDefault(t => t.Id == key);
          Ctx.DefinitionTerms.Remove(term as DefinitionTerm);
          break;
        case TermType.Idiom:
          terms = ts.Terms.OfType<IdiomTerm>();
          term = terms.FirstOrDefault(t => t.Id == key);
          Ctx.IdiomTerms.Remove(term as IdiomTerm);
          break;
        case TermType.Variable:
          terms = ts.Terms.OfType<VariableTerm>();
          term = terms.FirstOrDefault(t => t.Id == key);
          Ctx.VariableTerms.Remove(term as VariableTerm);
          break;
        case TermType.Link:
          terms = ts.Terms.OfType<LinkTerm>();
          term = terms.FirstOrDefault(t => t.Id == key);
          Ctx.LinkTerms.Remove(term as LinkTerm);
          break;
      }
      SaveChanges();
      return terms;
    }


    public TermType RemoveTerm(int id) {
      var term = Ctx.Terms.Find(id);
      var type = term.TermType;
      Ctx.Terms.Remove(term);
      SaveChanges();
      return type; // for refresh
    }


    # endregion

 }
}