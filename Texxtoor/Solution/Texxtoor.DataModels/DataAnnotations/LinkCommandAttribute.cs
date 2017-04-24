using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.DataModels.DataAnnotations {

  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class LinkCommandAttribute : CommandAttribute {

    public LinkCommandAttribute(Type resourceType, string titleKey, string action, string controller) {
      ResourceType = resourceType;
      TitleKey = titleKey;
      Action = action;
      Controller = controller;
    }
  }
}
