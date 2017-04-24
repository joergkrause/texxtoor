using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using System.Xml.Serialization;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Author {

  [Table("TermSets", Schema = "Author")]
  public class TermSet : LocalizedEntityBase {

    public TermSet() {
      Active = true;
    }

    /// <summary>
    /// Short text in editor.
    /// </summary>
    [Required]
    [StringLength(64)]
    [Display(ResourceType = typeof(ModelResources), Name = "TermSet_Name_Name", Description="TermSet_Name_Name_Helptext")]
    [Watermark(typeof(ModelResources), "TermSet_Watermark_Name")]
    public string Name { get; set; }

    /// <summary>
    /// Long text that explains the value (optional)
    /// </summary>
    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "TermSet_Description_Description", Description="TermSet_Description_Description_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 55)]
    [Watermark(typeof(ModelResources), "TermSet_Watermark_Description")]
    public string Description { get; set; }
    
    /// <summary>
    /// Deactivate in selection dialogs
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "TermSet_Active_Is_active", Description="TermSet_Active_Is_active_Helptext")]
    public bool Active { get; set; }

    /// <summary>
    /// Whether it's public or not.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "TermSet_Shared_Is_shared_between_projects", Description="TermSet_Shared_Is_shared_between_projects_Helptext")]
    public bool Shared { get; set; }

    /// <summary>
    /// Set of terms defined in this termset
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "TermSet_Terms_Terms", Description="TermSet_Terms_Terms_Helptext")]
    [ScaffoldColumn(false)]
    [XmlArray("Terms")]
    public virtual List<Term> Terms { get; set; }

    /// <summary>
    /// The project this termset is assigned to.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "TermSet_Project_Related_Project", Description="TermSet_Project_Related_Project_Helptext")]
    [ScaffoldColumn(false)]
    public virtual Project Project { get; set; }

    /// <summary>
    /// If Termset is shared we need to set the owner to assign the termset somewhere
    /// </summary>
    [ScaffoldColumn(false)]
    public virtual User Owner { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "TermSet_Culture_Assigned_Culture", Description="TermSet_Culture_Assigned_Culture_Helptext")]
    [UIHint("CultureSelection")]
    [AdditionalMetadata("Name", "Culture")]
    [XmlIgnore]
    public override System.Globalization.CultureInfo Culture {
      get {
        return base.Culture;
      }
      set {
        base.Culture = value;
      }
    }

  }

}
