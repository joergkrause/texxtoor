using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Texxtoor.DataModels.Models.Common;

namespace Texxtoor.DataModels.DataAnnotations {

  /// <summary>
  /// Make the scaffolding used for EditorForModel more flexible.
  /// </summary>
  public class DynamicScaffoldAttribute : Attribute {

    private Complexity _complexity;

    public DynamicScaffoldAttribute(Complexity complexity) {
      _complexity = complexity;
    }

    /// <summary>
    ///  Called from <see>t:PropertyUiHintModelMetadataProvider</see> and excludes element if not for the proper complexity mode.
    /// </summary>
    /// <param name="complexity"></param>
    /// <returns></returns>
    public bool ScaffoldFor(Complexity complexity) {
      return _complexity == complexity;
    }

  }
}
