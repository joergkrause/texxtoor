using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.BaseLibrary.Core;

namespace Texxtoor.DataModels.Models.Common {

  /// <summary>
  /// A message in the internal mail system.
  /// </summary>
  [Table("Messages", Schema = "Common")]
  public class Message : EntityBase {

    /// <summary>
    /// Null value will broadcast to all users
    /// </summary>    
    [ScaffoldColumn(false)]
    [Display(ResourceType = typeof(ModelResources), Name = "Message_Receiver_Receiver", Description = "Message_Receiver_Receiver_Helptext")]
    public virtual User Receiver { get; set; }

    /// <summary>
    /// Null value is reserved for system generated messages
    /// </summary>
    [ScaffoldColumn(false)]
    public virtual User Sender { get; set; }
    
    [Required]
    [StringLength(250)]
    [Display(ResourceType = typeof(ModelResources), Name = "Message_Subject_Subject", Description="Message_Subject_Subject_Helptext")]
    [Watermark(typeof(ModelResources), "Message_Watermark_Subject")]
    public string Subject { get; set; }

    [StringLength(5000)]
    [Display(ResourceType = typeof(ModelResources), Name = "Message_Body_Message", Description="Message_Body_Message_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 7)]
    [AdditionalMetadata("Cols", 75)]
    [Watermark(typeof(ModelResources), "Message_Watermark_Body")]
    public string Body { get; set; }

    /*
     * TODO: Extend this to send tasks, internal links, reply-to, multicast, ...
     * 
     * 
     * */

  }

}
