using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models {

  /// <summary>
  /// This table contains all search for active service sessions. Entries are deleted when user logs out.
  /// </summary>
  [Table("Session", Schema="Services")]
  public class Session: EntityBase {

    // session that stores this result
    [Required]
    public string SessionId { get; set; }
    
    // book that matches the search
    public string SessionData { get; set; }

    public virtual User User { get; set; }

  }


}
