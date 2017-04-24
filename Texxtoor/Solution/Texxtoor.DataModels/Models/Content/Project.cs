using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.DataAnnotations;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Marketing;
using System.IO;

namespace Texxtoor.DataModels.Models.Content {

  [Table("Projects", Schema = "Content")]
  [DropCommand(typeof(ModelResources), "DropCommand_Deactivated", "DeactivateProject", "AuthorPortal/Project", KeyName = "item", DescriptionKey = "DropCommand_Deactivated_Description", Order = 1, ActionType=ActionType.Json)]
  [DropCommand(typeof(ModelResources), "DropCommand_Imports", "Import", "AuthorPortal/Project", KeyName = "item", DescriptionKey = "DropCommand_Imports_Description", Order = 2)]
  [DropCommand(typeof(ModelResources), "DropCommand_AssignAuthor", "AssignToLeadAuthor", "AuthorPortal/Project", KeyName = "item", DescriptionKey = "DropCommand_AssignAuthor_Description", Order = 3)]
  [DropCommand(typeof(ModelResources), "DropCommand_FileResources", "Index", "AuthorPortal/Resource", KeyName = "item", DescriptionKey = "DropCommand_FileResources_Description", Order = 4)]
  [DropCommand(typeof(ModelResources), "DropCommand_SemanticData", "TermSets", "AuthorPortal/Resource", KeyName = "item", DescriptionKey = "DropCommand_SemanticData_Description", Order = 5)]
  [DropCommand(typeof(ModelResources), "DropCommand_Marketing", "MarketingPackage", "AuthorPortal/Marketing", KeyName = "item", DescriptionKey = "DropCommand_Marketing_Description", Order = 6)]
  [DropCommand(typeof(ModelResources), "DropCommand_Publish", "Index", "AuthorPortal/Publishing", KeyName = "item", DescriptionKey = "DropCommand_Publish_Description", Order = 7)]
  public class Project : EntityBase {

    public Project() {
      Opuses = new List<Opus>();
      Published = new List<Published>();
      TermSets = new List<TermSet>();
    }

    /// <summary>
    /// Project Name
    /// </summary>
    [Required]
    [StringLength(150)]
    [AdditionalMetadata("Length", 55)]
    [Display(ResourceType = typeof(ModelResources), Name = "Project_Name_Project_Name", Description = "Project_Name_Project_Name_Helptext")]
    [Watermark(typeof(ModelResources), "Project_Name_MyFirstProject")]
    [FilterUIHint("StringFilter", "MVC", "width:100px")]
    public string Name { get; set; }

    /// <summary>
    /// Short description for overviews and lists. Can contain HTML.
    /// </summary>
    [Required]
    [StringLength(255)]
    [AdditionalMetadata("Length", 55)]
    [Display(ResourceType = typeof(ModelResources), Name = "Project_Short_Short_Description", Description = "Project_Short_Short_Description_Helptext")]
    [Watermark(typeof(ModelResources), "Project_Name_InternalDescription")]
    public string Short { get; set; }

    /// <summary>
    /// Project wall text. Can contain HTML.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Project_Description_Verbose_Description", Description = "Project_Description_Verbose_Description_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 55)]
    [Watermark(typeof(ModelResources), "Project_Name_Description")]
    public string Description { get; set; }

    /// <summary>
    /// Terms and Conditions ADDITIONALLY to the common project terms.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Project_Terms_Terms_for_Contributors", Description = "Project_Terms_Terms_for_Contributors_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 2)]
    [AdditionalMetadata("Cols", 55)]
    [Column("Terms")]
    public string TermsAndConditions { get; set; }

    /// <summary>
    /// Terms MUST be approved by all team members.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Project_ApproveTerms_Must_approve", Description = "Project_ApproveTerms_Must_approve_Helptext")]
    public bool ApproveTerms { get; set; }

    /// <summary>
    /// A list of members that have approved the terms. Once the terms change the approvel list is cleared by business logic.
    /// </summary>
    public virtual IList<TeamMember> MemberTermApprovals { get; set; }

    /// <summary>
    /// Deactivate projects are not visible. Deleting will just deactivatin the project.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Project_Active_Is_active", Description = "Project_Active_Is_active_Helptext")]
    public bool Active { get; set; }

    /// <summary>
    /// An unique image for project's home page.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Project_Image_Image", Description = "Project_Image_Image_Helptext")]
    public byte[] Image { get; set; }

    /// <summary>
    /// Project team
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "Project_Team_Team", Description="Project_Team_Team_Helptext")]
    public virtual Team Team { get; set; }

    /// <summary>
    /// Opus' of the project
    /// </summary>
    public virtual IList<Opus> Opuses { get; set; }

    /// <summary>
    /// Published Books that are based on this project.
    /// </summary>
    public IList<Published> Published { get; set; }

    /// <summary>
    /// Predefined package
    /// </summary>
    public MarketingPackage Marketing { get; set; }

    /// <summary>
    /// Assigned Termsets
    /// </summary>
    public IList<TermSet> TermSets { get; set; }

    /// <summary>
    /// All resources used to create the Opus', even deleted and internal ones.
    /// </summary>
    public virtual IList<Resource> Resources { get; set; }

    # region --== Service Methods ==--

    /// <summary>
    /// The project is never publishable, regardless of the content.
    /// </summary>
    public bool IsSample { get; set; }

    /// <summary>
    /// Check whether at least one opus is publishable.
    /// </summary>
    /// <returns></returns>
    public bool CanPublish() {
      // check globals first
      if (IsSample) return false;
      if (Marketing == null) return false;
      return Opuses.Any(CanPublish);
    }

    private bool CanPublish(Opus arg){
      if (!arg.Active) return false;
      //if (!Team.Members.First(m => m.TeamLead == true).Member.Profile.HasDefaultAddress()) return false;
      if (Team.Members.Count() == 1) return true;
      // todo: check shares
      return true;
    }

    public IList<Opus> GetPublishableOpuses(ref List<KeyValuePair<Opus, string>> reason) {
      IList<Opus> ops = new List<Opus>();
      // loop through all opuses
      if (reason == null) reason = new List<KeyValuePair<Opus, string>>();
      foreach (var opus in Opuses) {
        var can = true;
        // exclude inactive books
        if (!opus.Active) {
          reason.Add(new KeyValuePair<Opus, string>(opus, "Text is not active"));
          can = false;
        }
        // check all confirmations
        if (opus.Project.Team.Members.Any(m => m.Pending && !m.TeamLead)) {
          var pending = opus.Project.Team.Members.Where(m => m.Pending && !m.TeamLead).Select(p => p.Member.UserName);
          reason.Add(new KeyValuePair<Opus, string>(opus, "Members are pending: " + String.Join(", ", pending.ToArray())));
          can = false;
        }
        // Check all milestones
        if (opus.Milestones != null) {
          foreach (var ms in opus.Milestones) {
            if (ms.Done == false) {
              can = false;
              reason.Add(new KeyValuePair<Opus, string>(opus, "Milestone not done: " + ms.Name + " is at " + ms.Progress + "%"));
            }
          }
        }
        // if all confirmed at least one opus is publishable
        if (can) {
          ops.Add(opus);
        }
      }
      // if we come here there is nothing publishable
      return ops;
    }

    # endregion --== Service Methods ==--

    [NotMapped]
    public System.IO.Stream ImageStream {
      get {
        return Image == null ? null : new MemoryStream(Image);
      }
    }
  }

}
