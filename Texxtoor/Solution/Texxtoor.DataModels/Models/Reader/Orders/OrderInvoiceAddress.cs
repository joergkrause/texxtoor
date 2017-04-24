using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Reader.Orders {

  [Table("AddressBook", Schema = "Common")]
  public class OrderInvoiceAddress : AddressBook {

    public OrderProduct OrderProduct { get; set; }

  }


}