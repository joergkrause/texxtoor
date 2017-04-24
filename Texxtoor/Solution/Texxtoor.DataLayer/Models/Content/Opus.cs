using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels.Helper;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.DataAnnotations;
using Texxtoor.DataModels.Models.Common;
using System.ComponentModel;
using Texxtoor.BaseLibrary.Core;
using System.Web.Mvc;
using System.Data.Entity.ModelConfiguration;
using Texxtoor.DataModels.Properties;

namespace Texxtoor.DataModels.Models.Content {

  [Table("Elements", Schema = "Content")]
  [SnippetBuilder(GroupKind.Pdf, @"<div>{0}</div>", "BuiltContent")]
  [SnippetBuilder(GroupKind.Epub, @"{0}", "BuiltContent")]
  [SnippetBuilder(GroupKind.Html, @"{0}", "BuiltContent")] // this is now template based and does not need to be enriched at document level
  public class Opus : Element {

    public Opus() {
      Active = true;
      Variation = VariationType.HeadRevision;
    }

    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "Opus_Name_The_text_must_have_distinct_name_")]
    [Watermark(typeof(ModelResources), "Opus_Watermark_Name")]
    [Display(Order = 10, ResourceType = typeof(ModelResources), Name = "Opus_Name_Text_Name", Description = "Opus_Name_Text_Name_Helptext")]
    public override string Name {
      get {
        {
          return base.Name;
        }
      }
      set { base.Name = value; }
    }

    [NotMapped]
    public override string WidgetName {
      get { return "Opus"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return FragmentType.Meta; }
    }

    [UIHint("Variation")]
    [Display(ResourceType = typeof (ModelResources), Name = "Element_Variation_Variation", Description = "Element_Variation_Variation_Helptext", Order = 50)]
    public VariationType Variation { get; set; }

    public override string Properties {
      get {
        if (String.IsNullOrEmpty(properties)) {
          properties =
            String.Format(
              "{{\"ShowNumberChain\":\"{0}\",\"LastChapterId\":{1},\"LastSnippetId\":{2},\"AllowMetaData\":{3},\"AllowChapters\":{4},\"ShowNaviPane\":{5},\"ShowFlowPane\":{6},\"ListingSnippetDefault\":\"{7}\",\"TextSnippetDefault\":\"{8}\",\"ChapterDefault\":\"{9}\",\"SectionDefault\":\"{10}\",\"IsBoilerplate\":\"{11}\"}}",
              "true", 0, 0, "true", "true", "true", "true", "public class {\n// code goes here...\n}", "<p></p>", "Chapter", "Section", "false");
        }

        return properties;
      }
      set { properties = value; }
    }

    # region Editor Properties

    [NotMapped]
    [ScaffoldColumn(true)]
    [Display(ResourceType = typeof(ModelResources), Name = "Opus_IsBoilerplate_Is_Boilerplate", Description = "Opus_IsBoilerplate_Is_Boilerplate_Helptext", Order = 40)]
    public bool IsBoilerplate {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).IsBoilerplate; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.IsBoilerplate = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public bool ShowNumberChain {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).ShowNumberChain; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.ShowNumberChain = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public int LastChapterId {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).LastChapterId; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.LastChapterId = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public int LastSnippetId {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).LastSnippetId; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.LastSnippetId = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public bool AllowMetaData {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).AllowMetaData; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.AllowMetaData = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public bool AllowChapters {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).AllowChapters; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.AllowChapters = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public bool ShowNaviPane {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).ShowNaviPane; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.ShowNaviPane = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public string ListingSnippetDefault {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).ListingSnippetDefault; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.ListingSnippetDefault = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public string TextSnippetDefault {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).TextSnippetDefault; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.TextSnippetDefault = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public string ChapterDefault {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).ChapterDefault; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.ChapterDefault = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public string SectionDefault {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).SectionDefault; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.SectionDefault = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public bool ShowFlowPane {
      get { return System.Web.Helpers.Json.Decode<DocumentProperties>(Properties).ShowFlowPane; }
      set {
        var p = System.Web.Helpers.Json.Decode<DocumentProperties>(Properties);
        p.ShowFlowPane = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

    # endregion

    # region Builder Support

    [NotMapped]
    [ScaffoldColumn(false)]
    public string BuiltContent { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public string HeadContent { get; set; }

    # region these properties are used to retrieve global callbacks while creating HTML

    [NotMapped]
    [ScaffoldColumn(false)]
    public string TempPath { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public CreateImageHandler CreateImage { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public ScaleImageHandler ScaleImage { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public IDictionary<string, NumberingSchema> Numbering { get; set; }

    # endregion

    # endregion

    # region Project Management

    [Required]
    public virtual Project Project { get; set; }

    public virtual Published Published { get; set; }

    [Display(ResourceType = typeof (ModelResources), Name = "Opus_Active_Active", Description = "Opus_Active_Active_Helptext")]
    [ScaffoldColumn(false)]
    public bool Active { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public bool IsPublished {
      get { return Published != null && Published.IsPublished; }
    }

    /// <summary>
    /// Define any number of trackable items while developing the Opus
    /// </summary>
    public virtual List<Milestone> Milestones { get; set; }

    /// <summary>
    /// Defines the share between team members regarding the revenues using a percentage value.
    /// </summary>
    public virtual IList<ContributorRatio> ContributorRatios { get; set; }

    # endregion

    /// <summary>
    /// The version is just a counter. The version hierarchy is build using a parent relation to another opus
    /// </summary>
    [Range(0, 100)]
    [Display(ResourceType = typeof (ModelResources), Name = "Opus_Version_Version", Description = "Opus_Version_Version_Helptext", Order = 30)]
    [DynamicScaffoldAttribute(Complexity.Full)]
    [DefaultValue(1)]
    [AdditionalMetadata("Length", 5)]
    public int Version { get; set; }

    /// <summary>
    /// The relation of an element's content to its predecessors. 
    /// </summary>
    /// <remarks>
    /// On opus level it's used to organize documents. 
    /// </remarks>
    public virtual Opus PreviousVersion { get; set; }

    # region Native App support elements

    [UIHint("TextArea")]
    [AdditionalMetadata("rows", 6)]
    [AdditionalMetadata("cols", 40)]
    [StringLength(1000)]
    [Display(ResourceType = typeof (ModelResources), Name = "Opus_Requirements_Requirements", Order = 100)]
    public string Requirements { get; set; }

    [TypeConverter(typeof(ResourceFileTypeConverter))]
    [UIHint("ResourceSelection")]
    [Display(ResourceType = typeof(ModelResources), Name = "Opus_RequirementsResource_Requirements_Image", Order = 111)]
    public ResourceFile RequirementsResource { get; set; }

    [UIHint("TextArea")]
    [AdditionalMetadata("rows", 6)]
    [AdditionalMetadata("cols", 40)]
    [StringLength(1000)]
    [Display(ResourceType = typeof (ModelResources), Name = "Opus_ProposedOutcome_Proposed_Outcome", Order = 110)]
    public string ProposedOutcome { get; set; }

    [TypeConverter(typeof(ResourceFileTypeConverter))]
    [UIHint("ResourceSelection")]
    [Display(ResourceType = typeof(ModelResources), Name = "Opus_ProposedOutcomeResource_Proposed_Outcome_Image", Order = 121)]
    public ResourceFile ProposedOutcomeResource { get; set; }

    [UIHint("TextArea")]
    [AdditionalMetadata("rows", 3)]
    [AdditionalMetadata("cols", 45)]
    [StringLength(1000)]
    [Display(ResourceType = typeof (ModelResources), Name = "Opus_TargetAudience_Target_Audience", Order = 120)]
    public string TargetAudience { get; set; }

    # endregion

    /// <summary>
    /// Evaluates whether the given user is team lead for the containing project (for opus lists)
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public bool UserIsTeamLead(string userName) {
      return Project != null && Project.Team.Members.Any(t => t.Member.UserName == userName && t.TeamLead);
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public string VariationName {
      get {
        var displayAttr = typeof (VariationType).GetField(Variation.ToString()).GetCustomAttributes(typeof (DisplayAttribute), true).OfType<DisplayAttribute>().Single();
        var variationName = displayAttr.GetName();
        return variationName;
      }
    }

    public decimal GetMilestonesPercentage() {
      if (Milestones != null && Milestones.Any()) {
        // only if loaded: each milestone has same share of 100% (4 MS == 25% each). We calculate the setting per MS related to this share.
        return (decimal) Math.Round(Milestones.Average(m => m.Progress), 0);
      }
      return 0M;
    }

    public bool HasMilestones() {
      return Milestones != null && Milestones.Any();
    }
  }

  public class OpusConfiguration : EntityTypeConfiguration<Opus> {
      public OpusConfiguration() {
        Property(x => x.Variation).IsRequired();
      }
    }

  public class ResourceFileTypeConverter : TypeConverter
  {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      return true;
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      return true;
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
      return base.ConvertTo(context, culture, value, destinationType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      return base.ConvertFrom(context, culture, value);
    }
  }

  }



