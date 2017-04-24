using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Texxtoor.DataModels.Models.Reader.Functions {

  /// <summary>
  /// Save private and public comments for each fragment. Might build an hierarchy by itself to have forum like threads.
  /// </summary>
  [Table("Forums", Schema = "Reader")]
  public class Forum : Discussion<Forum> {

  }


}