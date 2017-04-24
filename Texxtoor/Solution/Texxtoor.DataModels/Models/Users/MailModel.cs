using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Users {


  [DebuggerDisplay("Mail")]
  [Table("Mail", Schema = "Common")]
  public class MailModel : EntityBase {

    public string Subject { get; set; }

    public string Body { get; set; }

    public string Folder { get; set; }

    public string From { get; set; }

    public IList<string> To { get; set; }

    public IList<string> Cc { get; set; }

    public IList<string> Bcc { get; set; }

    public User User { get; set; }


  }
}
