using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Texxtoor.DataModels.DataAnnotations {

  /// <summary>
  /// Help the script to identify the right way to call the action.
  /// </summary>
  public enum ActionType {
    /// <summary>
    /// Call expect a Json result that can be show with toastr
    /// </summary>
    Json,  
    /// <summary>
    /// Call forwards to another location
    /// </summary>
    Href
  }


  public abstract class CommandAttribute : Attribute {

    /// <summary>
    /// Key for title displayed on drop zone.
    /// </summary>
    public string TitleKey { get; set; }

    /// <summary>
    /// Key for description displayed on drop zone.
    /// </summary>
    public string DescriptionKey { get; set; }

    /// <summary>
    /// Resource providing localizable information.
    /// </summary>
    public Type ResourceType { get; set; }

    /// <summary>
    /// Controller where the <see cref="Action"/> is invoked when a drop occurs.
    /// </summary>
    public string Controller { get; set; }

    /// <summary>
    /// Action for command
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// The name of a data- attribute that provides additional route data from draggable object. Usually the primary key (Id).
    /// </summary>
    public string KeyName { get; set; }

    /// <summary>
    /// Who to handle the call in script (ui.js).
    /// </summary>
    public ActionType ActionType { get; set; }

    /// <summary>
    /// Force an update after the operation. Usually required after Json result, does not work after Href ActionType.
    /// </summary>
    public bool UpdateTable { get; set; }

    /// <summary>
    /// Order in which the drop zones appear in UI.
    /// </summary>
    public int Order { get; set; }

    public string GetTitle() {
      if (ResourceType == null) {
        return TitleKey;
      }
      var property = ResourceType.GetProperty(TitleKey);
      if (property != null) {
        return property.GetValue(null, null) as string;
      }
      return TitleKey;
    }

    public string GetDescription() {
      if (ResourceType == null) {
        return DescriptionKey;
      }
      var property = ResourceType.GetProperty(DescriptionKey);
      if (property != null) {
        return property.GetValue(null, null) as string;
      }
      return DescriptionKey;
    }

  }
}
