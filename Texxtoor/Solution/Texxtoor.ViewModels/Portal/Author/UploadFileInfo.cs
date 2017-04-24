
using System;
using System.ComponentModel.DataAnnotations;
namespace Texxtoor.ViewModels.Author {
  public class UploadFileInfo {

    [Required]
    public string Name { get; set; }

    [Display(Name = "Target Folder")]
    public string Folder { get; set; }

    public long Size { get; set; }

    [Required]
    public int ProjectId { get; set; }

    public Guid ResourceId { get; set; }

  }
}
