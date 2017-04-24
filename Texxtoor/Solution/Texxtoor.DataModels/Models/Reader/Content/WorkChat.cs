using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.DataModels.Models.Reader.Functions;

namespace Texxtoor.DataModels.Models.Reader.Content {

  [Table("WorkChat", Schema = "Reader")]
  public class WorkChat : Discussion<WorkChat> {

    /// <summary>
    /// Published work to discuss about.
    /// </summary>
    public Work Work { get; set; }

  }


}
