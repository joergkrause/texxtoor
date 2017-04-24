using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.DataModels.Models.Reader.Functions {

  /// <summary>
  /// An experts forum that let readers ask questions and experts answer. Might have a monitary impact.
  /// </summary>
  [Table("QuestionsAnswers", Schema = "Reader")]
  public class QuestionsAnswers : Discussion<QuestionsAnswers> {

    [ScaffoldColumn(false)]
    public override string Name {
      get {
        return base.Subject;
      }
      set {
        base.Subject = value;
      }
    }

    /// <summary>
    /// The theme under this thread runs.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "QuestionsAnswers_Theme_Theme", Description = "QuestionsAnswers_Theme_Theme_Helptext", Order = 11)]
    public Theme Theme { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "QuestionsAnswers_Work_Work", Description = "QuestionsAnswers_Work_Work_Helptext", Order = 1)]
    [UIHint("ReadersWork")]
    public Work Work { get; set; }

    /// <summary>
    /// Type this discussion thread is associated to.
    /// </summary>
    [ScaffoldColumn(false)]
    public QandAType Type { get; set; }

    [ScaffoldColumn(false)]
    public override bool Closed {
      get {
        return base.Closed;
      }
      set {
        base.Closed = value;
      }
    }

    [ScaffoldColumn(false)]
    public override bool GroupOnly {
      get {
        return base.GroupOnly;
      }
      set {
        base.GroupOnly = value;
      }
    }



    private static List<SelectListItem> _types;

    [NotMapped]
    public static List<SelectListItem> Types {
      get {
        return _types ?? (_types = Enum.GetNames(typeof (QandAType))
                                     .Select(t => new SelectListItem {
                                       Text = t,
                                       Value = t
                                     }).ToList());
      }
    }
  }

}