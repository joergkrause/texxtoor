using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Texxtoor.Portal.Core.Extensions.Attributes {

  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
  public class NgDataContextFunction :Attribute  {

    public NgDataContextFunction(){
      
    }

    public NgDataContextFunction(string exportName){
      ExportName = exportName;
    }

    public string ExportName { get; set; }

  }

}