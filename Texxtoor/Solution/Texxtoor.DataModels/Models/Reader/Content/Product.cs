using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Reader.Content {

  /// <summary>
  /// Products contain personalized copies of items the user can order.
  /// </summary>
  /// <remarks>
  /// The content stream herein is a private, unique copy of the work, enriched with advertisments, and personalized information, such as watermarks.
  /// </remarks>
  [Table("Product", Schema = "Reader")]
  public class Product : BaseProduct {
  }

  public abstract class BaseProduct : LocalizedEntityBase {

    [ScaffoldColumn(false)]
    public override CultureInfo Culture { get; set; }

    /// <summary>
    /// The work that is many2one related with this production (each work can create as many products as one likes)
    /// </summary>
    public virtual Work Work { get; set; }

    /// <summary>
    /// Collection of all authors that have been provided fragments (comma separated)
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "BaseProduct_Authors_Authors", Description="BaseProduct_Authors_Authors_Helptext")]
    [ScaffoldColumn(false)]
    public string Authors { get; set; }

    [StringLength(255)]
    [Display(ResourceType = typeof(ModelResources), Name = "BaseProduct_Name_Name", Description = "BaseProduct_Name_Name_Helptext", Order = 1)]
    public string Name { get; set; }

    /// <summary>
    /// Either black & White or colored, if this is an option in production
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "BaseProduct_Colored_Colored", Description = "BaseProduct_Colored_Colored_Helptext", Order = 7)]
    public bool Colored { get; set; }

    [StringLength(255)]
    [Display(ResourceType = typeof(ModelResources), Name = "BaseProduct_Title_Title", Description = "BaseProduct_Title_Title_Helptext", Order = 2)]
    [Editable(false)]
    public string Title { get; set; }

    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "BaseProduct_SubTitle_Sub_title", Description = "BaseProduct_SubTitle_Sub_title_Helptext", Order = 3)]
    [Editable(false)]
    public string SubTitle { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "BaseProduct_Proprietor_Proprietor", Description = "BaseProduct_Proprietor_Proprietor_Helptext", Order = 4)]
    [ScaffoldColumn(true)]
    public string Proprietor { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "BaseProduct_Issue_Issue", Description = "BaseProduct_Issue_Issue_Helptext", Order = 5)]
    [Editable(false)]
    [ScaffoldColumn(false)]
    public string Issue { get; set; }

    [StringLength(1024)]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 55)]
    [Display(ResourceType = typeof(ModelResources), Name = "BaseProduct_Dedication_Dedication", Description = "BaseProduct_Dedication_Dedication_Helptext", Order = 6)]
    public string Dedication { get; set; }

    public virtual IList<OrderProduct> Orders { get; set; }

    /// <summary>
    /// The content is the PERSONALIZED copy of a specific (private) work. Assume that this is the media the user needs.
    /// </summary>
    /// <remarks>
    /// While in pre-production (Product) this might be empty, as the <see cref="Work"/> contains the content as a hierarchal list of fragments.
    /// After purchase the final names of files on a "per media" base in Blob Storage go here.
    /// </remarks>
    [ScaffoldColumn(false)]
    public byte[] Content { get; set; }

    public void SetContent(MediaFiles files) {      
      Content = System.Text.Encoding.UTF8.GetBytes(files.ToString());
    }

    public MediaFiles GetContent() {
      return MediaFiles.Deserialize(System.Text.Encoding.UTF8.GetString(Content));
    }

    // Owner of this product
    [ScaffoldColumn(false)]
    public virtual User Owner { get; set; }

  }

}