using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Serialization;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Author {

  public enum TermType {
    [Display(ResourceType = typeof (ModelResources), Name = "TermType_Cite_Cite")]
    Cite,
    [Display(ResourceType = typeof (ModelResources), Name = "TermType_Definition_Definition")]
    Definition,
    [Display(ResourceType = typeof (ModelResources), Name = "TermType_Abbreviation_Abbreviation")]
    Abbreviation,
    [Display(ResourceType = typeof (ModelResources), Name = "TermType_Idiom_Idiom")]
    Idiom,
    [Display(ResourceType = typeof (ModelResources), Name = "TermType_Variable_Variable")]
    Variable,
    [Display(ResourceType = typeof (ModelResources), Name = "TermType_Link_Link")]
    Link
  }

  [Table("Terms", Schema = "Author")]
  public abstract class Term : LocalizedEntityBase {
    protected Term() {
      Active = true;
    }

    [NotMapped]
    [Display(ResourceType = typeof(ModelResources), Name = "Term_TermType_Term_Type", Description = "Term_TermType_Term_Type_Helptext")]
    [XmlAttribute("TermType")]
    public abstract TermType TermType { get; }

    /// <summary>
    /// Short text in editor.
    /// </summary>
    [Required]
    [StringLength(32)]
    [Display(ResourceType = typeof(ModelResources), Name = "Term_Text_Name_or_Text", Description = "Term_Text_Name_or_Text_Helptext")]
    public string Text { get; set; }

    /// <summary>
    /// Long text that explains the value (optional)
    /// </summary>
    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "Term_Content_Content", Description = "Term_Content_Content_Helptext")]
    public string Content { get; set; }
    
    /// <summary>
    /// Deactivate in selection dialogs
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Term_Active_Is_Active", Description = "Term_Active_Is_Active_Helptext")]
    public bool Active { get; set; }

    /// <summary>
    /// Whether this is part of any term set.
    /// </summary>
    public virtual List<TermSet> TermSets { get; set; }

    /// <summary>
    /// Use resources to resolve the enum names.
    /// </summary>
    /// <returns></returns>
    public string GetLocalizedTermType() {
      return typeof(TermType)
        .GetField(TermType.ToString())
        .GetCustomAttributes(typeof(DisplayAttribute), true)
        .Cast<DisplayAttribute>()
        .Single()
        .GetName();
    }
  }

  [Table("Terms", Schema = "Author")]
  public class CiteTerm : Term {
    public override TermType TermType {
      get {
        return TermType.Cite;
      }
    }
  }

  [Table("Terms", Schema = "Author")]
  public class DefinitionTerm : Term {
    public override TermType TermType {
      get {
        return TermType.Definition;
      }
    }
  }

  [Table("Terms", Schema = "Author")]
  public class AbbreviationTerm : Term {
    public override TermType TermType {
      get {
        return TermType.Abbreviation;
      }
    }
  }

  [Table("Terms", Schema = "Author")]
  public class IdiomTerm : Term {
    public override TermType TermType {
      get {
        return TermType.Idiom;
      }
    }
  }

  [Table("Terms", Schema = "Author")]
  public class VariableTerm : Term {
    public override TermType TermType {
      get {
        return TermType.Variable;
      }
    }
  }

  [Table("Terms", Schema = "Author")]
  public class LinkTerm : Term {
    public override TermType TermType {
      get {
        return TermType.Link;
      }
    }
  }
  
}
