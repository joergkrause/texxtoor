using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.BaseLibrary.Exceptions {
  public class NoAddressException : ManagerException {

    public int UserId { get; set; }


  }
}
