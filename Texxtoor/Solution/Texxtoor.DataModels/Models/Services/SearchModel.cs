using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models {

  /// <summary>
  /// This table contains all search for active service sessions. Entries are deleted when user logs out.
  /// </summary>
  [Table("SearchTracking", Schema="Services")]
  public class SearchTracking: EntityBase {

    // session that stores this result
    public string SessionId { get; set; }
    // book that matches the search
    public int BookId { get; set; }
    // trackid that identifies this particular search
    public int TrackId { get; set; }
    // whether the search is active (currently always true)
    public bool Active { get; set; }

  }
}