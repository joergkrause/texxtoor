using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.ViewModels.Content;

namespace Texxtoor.DataModels.Models.Marketing {
  
  [Table("IsbnStore", Schema="Common")]
  public class IsbnStore : EntityBase {

    public IsbnStore() {
      
    }

    public IsbnStore(string isbn) {
      Isbn = new Isbn {Isbn10 = isbn};
    }

    public Isbn Isbn { get; set; }

    public Published AssignedTo { get; set; }

  }
}
