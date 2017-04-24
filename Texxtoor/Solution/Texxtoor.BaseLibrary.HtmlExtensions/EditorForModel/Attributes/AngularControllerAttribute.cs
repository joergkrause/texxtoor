using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Texxtoor.Portal.Core.Extensions.Attributes {
  public class AngularControllerAttribute : Attribute {

    public string ControllerPath { get; set; }

    public Type DefaultViewModel { get; set; }

    public AngularControllerAttribute() {
      
    }

    public AngularControllerAttribute(string path) {
      ControllerPath = path;
    }

    public AngularControllerAttribute(string path, Type defaultViewModel) {
      ControllerPath = path;
      DefaultViewModel = defaultViewModel;
    }

  }
}