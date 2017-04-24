using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.DataModels.Models.Common {

  /// <summary>
  /// This is stored in session and controls the behavior of the application globally and per user.
  /// </summary>
  [Serializable]
  [ComplexType]
  public class RunControl {

    private string _uiLanguage;

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_UILanguage_UILanguage",
      Description = "UserProfile_UILanguage_UILanguage_Helptext")]
    [StringLength(10)]
    [Required]
    public string UiLanguage {
      get {
        if (String.IsNullOrEmpty(_uiLanguage) || _uiLanguage == "iv") {
          return "en";
        }
        return _uiLanguage;
      }
      set { _uiLanguage = value; }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public string TwoLetterISO {
      get {
        var c = System.Globalization.CultureInfo.CreateSpecificCulture(UiLanguage);
        return c.TwoLetterISOLanguageName;
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public string NativeName {
      get {
        var c = System.Globalization.CultureInfo.CreateSpecificCulture(UiLanguage);
        return (c.IsNeutralCulture) ? c.NativeName : c.Parent.NativeName; ;
      }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "RunControl_RunMode_Run_Mode",
      Description = "RunControl_RunMode_Run_Mode_Helptext"), Required, ScaffoldColumn(false)]
    public RunMode RunMode { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "RunControl_Complexity_Complexity",
      Description = "RunControl_Complexity_Complexity_Helptext"), Required]
    public Complexity Complexity { get; set; }

    protected string properties;

    [ScaffoldColumn(false)]
    public string Properties {
      get {
        if (String.IsNullOrEmpty(properties)) {
          properties = System.Web.Helpers.Json.Encode(new RunProperties {
            Favorites = new Dictionary<string, string>()
          });
        }
        return properties;
      }
      set { properties = value; }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public IDictionary<string, string> Favorites {
      get { return System.Web.Helpers.Json.Decode<RunProperties>(Properties).Favorites; }
      set {
        var p = System.Web.Helpers.Json.Decode<RunProperties>(Properties);
        p.Favorites = value;
        Properties = System.Web.Helpers.Json.Encode(p);
      }
    }

  }

  /// <summary>
  /// A class that contains additional information about the user's selections and behavior, such as settings in grids and favorites.
  /// </summary>
  public class RunProperties {

    public IDictionary<string, string> Favorites { get; set; }

    public string Theme { get; set; }


  }



}
