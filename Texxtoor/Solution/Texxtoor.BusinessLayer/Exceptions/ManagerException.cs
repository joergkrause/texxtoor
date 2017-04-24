using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texxtoor.BusinessLayer;

namespace Texxtoor.BaseLibrary.Exceptions {
  public class ManagerException : Exception {

    public IManager Manager { get; set; }

    public string EntityName { get; set; }

    public int EntityId { get; set; }

  }
}
