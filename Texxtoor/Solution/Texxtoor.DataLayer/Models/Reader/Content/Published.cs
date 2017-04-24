using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Marketing;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.DataModels.ViewModels.Content;

namespace Texxtoor.DataModels.Models.Reader.Content {

  /// <summary>
  /// Contains the finally published opuses, regardless the usage of a reader. Reader have readonly access here.
  /// </summary>
  [Table("Published", Schema = "Reader")]
  public class Published : EntityBase {

    public Published() {
      Rating = 1;
      Starred = 1;
      NavLevel = 1;
      Catalogs = new List<Catalog>();
      ResourceFiles = new List<ResourceFile>();
      AuditingComment = new List<string>();
      AuditingLevel = new List<int>();
      Auditing = new List<bool>();
      Authors = new List<User>();
      FrozenFragments = new List<FrozenFragment>();
      CoverImage = new Cover();
    }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Published_Title_Work_s_Title", Description="Published_Title_Work_s_Title_Helptext")]
    [AdditionalMetadata("Length", 44)]
    public string Title { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Published_SubTitle_Work_s_Sub_Title", Description="Published_SubTitle_Work_s_Sub_Title_Helptext")]
    [StringLength(512)]
    [AdditionalMetadata("Length", 44)]
    public string SubTitle { get; set; }

    /// <summary>
    /// Number of levels used to create independent navigation elements.
    /// </summary>
    [Range(0, 3)]
    [Display(ResourceType = typeof(ModelResources), Name = "Published_NavLevel_Navigation_Level", Description="Published_NavLevel_Navigation_Level_Helptext")]
    [UIHint("NavLevel")]
    public int? NavLevel { get; set; }

    # region -== Backlink to Authors [> This is the connection to authoring module <] ==-

    /// <summary>
    /// The project this opus was created with    
    /// </summary>
    public virtual Opus SourceOpus { get; set; }

    /// <summary>
    /// The lead author
    /// </summary>
    public virtual User Owner { get; set; }

    /// <summary>
    /// Either the authors of an import or explicitly set by the lead author during the publishing procedure.
    /// </summary>
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Publis" +
                                                           "hed_Authors_Authors", Description = "Published_Authors_Authors_Helptext")]
    public virtual IList<User> Authors { get; set; }

    /// <summary>
    /// Name of publisher, mostly used for imports
    /// </summary>
    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "Published_Publisher_Publisher", Description = "Published_Publisher_Publisher_Helptext")]
    public string Publisher { get; set; }

    # endregion

    # region -== Properties that support the search algorithm ==-

    // Cover override the default cover that's provided by Book property
    [Display(ResourceType = typeof(ModelResources), Name = "Published_CoverImage_Cover_Image", Description="Published_CoverImage_Cover_Image_Helptext")]
    public Cover CoverImage { get; set; }

    /// <summary>
    /// All catalogs this title appears in.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Published_Catalogs_Catalogs", Description="Published_Catalogs_Catalogs_Helptext")]
    [TypeConverter(typeof(CatalogTypeConverter))]
    public List<Catalog> Catalogs { get; set; }

    public List<Review> Reviews { get; set; }

    // Zusammenfassung der Rating - Tabelle
    [Display(ResourceType = typeof(ModelResources), Name = "Published_Rating_Global_Rating", Description="Published_Rating_Global_Rating_Helptext")]
    [Range(1, 1000)]
    public int Rating { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Published_Starred_Simple_star_to_rank_the_work", Description="Published_Starred_Simple_star_to_rank_the_work_Helptext")]
    [Range(1, 5)]
    public int Starred { get; set; }

    # endregion -== Properties that support the search algorithm ==-

    # region -== Properties that support the marketing methods ==-

    /// <summary>
    /// Explicitly disallow specific media, by default everything is allowed.
    /// </summary>
    public List<OrderMedia> SupportedMedia { get; set; }

    /// <summary>
    /// Each published book must run a specific package. Choose from existing or create a new one.
    /// </summary>
    /// <remarks>
    /// Each <see cref="Opus"/> has exactly the same property. This is a predefined copy, that means, 
    /// the lead author can define a package for the Opus and afterwards overwrite during the
    /// publishing process. While the Opus remains unchanged, the published version may contain another 
    /// set.
    /// </remarks>
    public MarketingPackage Marketing { get; set; }

    # endregion

    # region -== Published Work Content Management ==-

    /// <summary>
    /// The fragments that form this published work and hence being frozen in their final state.
    /// </summary>
    public virtual IList<FrozenFragment> FrozenFragments { get; set; }

    /// <summary>
    /// For each contributor the lead can define a ratio after which the revenues can be shared among the team members.
    /// </summary>
    /// <remarks>
    /// This is the real sharing. The same relation is established for <see cref="Project"/>s, where it acts as a template for same.
    /// </remarks>
    public IList<ContributorRatio> ContributorRatios { get; set; }

    /// <summary>
    /// Associated resources for this published work, such as ZIP files, PDFs and other attachements.
    /// </summary>
    /// <remarks>We keep this flexible and allow the author to replace it at any time.</remarks>
    public IList<ResourceFile> ResourceFiles { get; set; }

    # endregion -== Published Work Content Management ==-

    # region -== Publishing Management ==-

    [ScaffoldColumn(false)]
    public List<Work> DerivedWorks { get; set; }

    [ScaffoldColumn(false)]
    public bool IsPublished {
      get;
      set;
    }

    [ScaffoldColumn(false)]
    public virtual Imprint Imprint { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public bool HasContent {
      get {
        return (FrozenFragments.Any());
      }
    }

    /// <summary>
    /// Covers all external stores, such as IBook Store, KDP etc. as a universal JSON object.
    /// </summary>
    [ScaffoldColumn(false)]
    public string ExternalPublisherProperties { get; set; }

    private ExternalPublisherSettings _externalPublisher;

    [NotMapped]
    [ScaffoldColumn(false)]
    public ExternalPublisherSettings ExternalPublisher {
      get {
        if (_externalPublisher == null) {
          if (string.IsNullOrEmpty(ExternalPublisherProperties)) {
            ExternalPublisherProperties = new JavaScriptSerializer().Serialize(new ExternalPublisherSettings());
          }
          _externalPublisher = new JavaScriptSerializer().Deserialize<ExternalPublisherSettings>(ExternalPublisherProperties);
          _externalPublisher.PropertyChanged += (sender, args) => {
            ExternalPublisherProperties = new JavaScriptSerializer().Serialize(sender as ExternalPublisherSettings);
          };
        }
        return _externalPublisher;
      }
    }

    # endregion

    [ScaffoldColumn(false)]
    [Display(ResourceType = typeof(ModelResources), Name = "Published_PreferredTemplateGroup_Preferred_Template_Group", Description = "Published_PreferredTemplateGroup_Preferred_Template_Group_Helptext")]
    public virtual IList<TemplateGroup> PreferredTemplateGroup { get; set; }

    // We fill this dynamically from authors' list once we need it.
    [NotMapped]
    public IList<UserProfile> AuthorProfiles { get; set; }

    // We fill this dynamically from authors' list once we need it.
    [NotMapped]
    public string AuthorInformation { get; set; }

    [NotMapped]
    public string Abstract {
      get {
        return ExternalPublisher.Description;
      } 
    }

    [Display(Name = "Audited")]
    [NotMapped]
    public List<bool> Auditing {
      get;
      set;
    }

    [Display(Name = "Peer review (1=best, 5 = worst)")]
    [NotMapped]
    public List<int> AuditingLevel {
      get;
      set;
    }

    [Display(Name = "Peer review comments")]
    [NotMapped]
    public List<string> AuditingComment {
      get;
      set;
    }

    [NotMapped]
    [Display(Name = "Appear in the recommendation list")]
    public bool IsRecommendation { get; set; }

    [NotMapped]
    public string LocaleId {
      get { return SourceOpus.LocaleId; } }
  }
}