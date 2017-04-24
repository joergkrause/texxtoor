using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Texxtoor.DataModels.Attributes;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.DataModels.Models.Content {

  #region -= Elements =-

  [Table("Elements", Schema = "Content")]
  [EditorServiceWrapper(typeof(SectionJsonBehavior))]
  [SnippetBuilder(GroupKind.Pdf, "<h{0} id='{5}'><span>{4}</span> {1}</h{0}>{3}", "Deep", "RawContent", "BuildStyle", "Item", "ParentNumbering", "BuilderId")]
  [SnippetBuilder(GroupKind.Epub, "{6}<h{0} id='{5}'><span>{4}</span> {1}</h{0}>{3}{7}", "Deep", "RawContent", "BuildStyle", "Item", "ParentNumbering", "BuilderId", "EpubContainerStart", "EpubContainerEnd")]
  [SnippetBuilder(GroupKind.Html, "<h{0} id='{5}'><span>{4}</span> {1}</h{0}>{3}", "Deep", "RawContent", "BuildStyle", "Item", "ParentNumbering", "BuilderId")]
  public class Section : Snippet {

    public Section() {
      Counter = 1;
    }

    # region Support for building hierarchy on import

    private int _designatedLevelOnImport;

    public void SetDesignatedLevelOnImport(int level) {
      _designatedLevelOnImport = level;
    }

    public int GetDesignatedLevelFromImport() {
      return _designatedLevelOnImport;
    }

    # endregion

    /// <summary>
    /// Checks if 
    /// </summary>
    [NotMapped]
    public bool IsBoilerplate {
      get {
        var parent = Parent;
        while (!(parent is Opus)) {
          parent = parent.Parent;
        }
        return ((Opus)parent).IsBoilerplate;
      }
    }

    [NotMapped]
    public int Deep {
      get {
        int level = 0;
        var parent = Parent;
        while (parent != null) {
          level++;
          parent = parent.Parent;
        }
        return level;
      }
    }

    public int Counter { get; set; }

    /// <summary>
    /// Creates a string such as 1.3.2 out of the parent hierarchies Counter values and the given separator. The value is written into the <see href="ParentNumbering">ParentNumbering</see> field as well.
    /// </summary>
    /// <param name="separator"></param>
    /// <returns></returns>
    public string SetCounterString(string separator) {
      string pn = "";
      Section parent = this;
      while (parent != null) {
        pn = String.Format("{0}{1}{2}", parent.Counter, separator, pn);
        parent = parent.Parent as Section;
      }
      return ParentNumbering = pn;
    }

    /// <summary>
    /// Supports creation of TOC and Index as well as hyperlink references.
    /// </summary>
    [NotMapped]
    public override string BuilderId {
      get { return String.Format("{0}", Id); }
    }

    [NotMapped]
    public override string WidgetName {
      get { return "Section"; }
    }

    [NotMapped]
    public override FragmentType ProposedFragmentType {
      get { return Reader.Content.FragmentType.Html; }
    }

    private string _parentNumbering;

    [NotMapped]
    public string ParentNumbering {
      get {
        if (String.IsNullOrEmpty(_parentNumbering)) {
          return String.Empty;
        }
        return _parentNumbering.EndsWith(".") ? _parentNumbering.Substring(0, _parentNumbering.Length - 1) : _parentNumbering;
      }
      set { _parentNumbering = value; }
    }

    /// <summary>
    /// support the boom! style. L==1 is Opus.
    /// </summary>
    [NotMapped]
    public string BuildStyle {
      // Prince Support for chapter counting and page break
      get { return String.Format("heading{0}", Level - 1); }
    }

    /*
     * The builder will create a separate file per chapter for epubs. To have the file completed the head and containter data are provided.
     * TODO: Make the level configurable
    */
    [NotMapped]
    public string EpubContainerStart {
      // This is a support functions that ensures nesting levels. The composition is made based on templates
      get { return Deep > 1 ? @"<div>" : ""; }
    }

    [NotMapped]
    public string EpubContainerEnd {
      get { return Deep > 1 ? @"</div>" : ""; }
    }

    // this is an indexer to get a property that takes a parameter, used in the snippetbuilderattribute
    [NotMapped]
    public override string this[GroupKind target, FrozenFragment targetFragment] {
      get {
        return HasChildren() ? CreateChildren(Children.OfType<Snippet>().OrderBy(c => c.OrderNr), target, targetFragment) : String.Empty; // content is pulled from RawContent if no children
      }
    }


  }

  #endregion

}
