using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.ViewModels.Common {

  /// <summary>
  /// USed to create a sequence of droppable areas in PRO mode.
  /// </summary>
  public class DropCommand {
    
    public DropCommand(Texxtoor.DataModels.DataAnnotations.DropCommandAttribute c) {
      Title = c.GetTitle();
      Description = c.GetDescription();
      Action = c.Action;
      Controller = c.Controller;
      ItemKey = c.KeyName;
      ResultType = c.ActionType.ToString(); // JSON or HREF
      UpdateTable = c.UpdateTable;
    }

    public string Title { get; set; }
    public string Description { get; set; }
    public string Action { get; set; }
    public string Controller { get; set; }
    public string Value { get; set; }
    public string ResultType { get; set; }
    public bool IsLast { get; set; }
    public string ItemKey { get; set; }
    public bool UpdateTable { get; set; }

    public bool HasAction {
      get {
        return !String.IsNullOrEmpty(Action);
      }
    }
  }
}
