using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.DataModels.DataAnnotations {

  /// <summary>
  /// On models or viewmodels we can define this command so in the UI it will appear as a drop target.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class DropCommandAttribute : CommandAttribute {

    public DropCommandAttribute(Type resourceType, string titleKey, string action, string controller) {
      ResourceType = resourceType;
      TitleKey = titleKey;
      Action = action;
      Controller = controller;
      ActionType = ActionType.Href;
    }

  }
}
