using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Marketing;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.DataModels.ViewModels.Content;

namespace Texxtoor.ViewModels.Portal.Author {
  public class PublishingPreset {

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Published_Title_Work_s_Title", Description = "Published_Title_Work_s_Title_Helptext")]
    [AdditionalMetadata("Length", 44)]
    public string Title { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Published_SubTitle_Work_s_Sub_Title", Description = "Published_SubTitle_Work_s_Sub_Title_Helptext")]
    [StringLength(512)]
    [AdditionalMetadata("Length", 44)]
    public string SubTitle { get; set; }

    /// <summary>
    /// Number of levels used to create independent navigation elements.
    /// </summary>
    [Range(0, 3)]
    [Display(ResourceType = typeof(ModelResources), Name = "Published_NavLevel_Navigation_Level", Description = "Published_NavLevel_Navigation_Level_Helptext")]
    [UIHint("NavLevel")]
    public int? NavLevel { get; set; }

    # region -== Backlink to Authors [> This is the connection to authoring module <] ==-

    /// <summary>
    /// The project this opus was created with    
    /// </summary>
    public virtual Opus SourceOpus { get; set; }

    /// <summary>
    /// Name of publisher, mostly used for imports
    /// </summary>
    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "Published_Publisher_Publisher", Description = "Published_Publisher_Publisher_Helptext")]
    public string Publisher { get; set; }

    # endregion

    # region -== Properties that support the marketing methods ==-

    /// <summary>
    /// Explicitly disallow specific media, by default everything is allowed.
    /// </summary>
    public List<OrderMedia> SupportedMedia { get; set; }


    # endregion

    # region -== Published Work Content Management ==-

    /// <summary>
    /// Associated resources for this published work, such as ZIP files, PDFs and other attachements.
    /// </summary>
    /// <remarks>We keep this flexible and allow the author to replace it at any time.</remarks>
    public IList<ResourceFile> ResourceFiles { get; set; }

    # endregion -== Published Work Content Management ==-


    # region KDP

    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_KindleDescription_Name", Description = "ExternalPublisher_KindleDescription_Name_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 45)]
    [Required]
    public string Description { get; set; }


    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_KindleKeywords_Search_Keywords", Description = "ExternalPublisher_KindleKeywords_Search_Keywords_Helptext")]
    [Required]
    public string Keywords { get; set; }

    # endregion

  }
}
