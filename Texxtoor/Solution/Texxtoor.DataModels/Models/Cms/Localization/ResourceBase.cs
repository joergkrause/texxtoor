using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Texxtoor.DataModels.Model.Cms.Localization {

  [Table("StaticResources", Schema = "Cms")]
  public abstract class ResourceBase {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string ResourceId { get; set; }

    [Required]
    public string ResourceSet { get; set; }

    public string LocaleId { get; set; }

    public string Value { get; set; }

    [NotMapped]
    public CultureInfo Culture {
      get {
        return String.IsNullOrEmpty(LocaleId) ? CultureInfo.InvariantCulture : new CultureInfo(LocaleId);
      }
      set {
        LocaleId = value.Name;
      }
    }

  }

  [Table("StaticResources", Schema = "Cms")]
  public class StringResource : ResourceBase {

  }

  [Table("StaticResources", Schema = "Cms")]
  public class ImageResource : ResourceBase {
    
    public byte[] BinData { get; set; }
    
    [StringLength(256)]
    public string FileName { get; set; }
  }


}
