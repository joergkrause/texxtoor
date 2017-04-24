using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LinqDemo.Models {

  #region -= Elements =-

  [Table("Elements")]
  public abstract class Element : LocalizedHierarchyBase<Element> {

    protected string properties;

    [ScaffoldColumn(false)]
    public virtual string Properties {
      get { return properties; }
      set { properties = value; }
    }

    /// <summary>
    /// If parts of the document are pre-generated one can set this property to block UI operations.
    /// </summary>
    [ScaffoldColumn(false)]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// The actual data in the Element
    /// </summary>
    public virtual byte[] Content { get; set; }

    
    # region Helper for Builder

    public string GetSectionLevel(int stopAtId) {
      // we calculate sections only
      var s = this;
      // first, get the position in the stack of sections (ignoring other widget types)
      var res = new List<string>();
      var current = this;
      do {
        var stack = current.Parent.Children.OfType<Section>().OrderBy(se => se.OrderNr).Cast<Element>().ToList();
        var pos = stack.IndexOf(current);
        if (pos < 0) { return String.Empty /* not a section */; }
        res.Add((pos + 1).ToString(CultureInfo.InvariantCulture));
        current = current.Parent as Section;
      } while (current != null && current.Id != stopAtId);
      return res.Count.ToString(CultureInfo.InvariantCulture);
    }

    public string GetSectionNumber(int stopAtId) {
      // we calculate sections only
      var s = this;
      // first, get the position in the stack of sections (ignoring other widget types)
      var res = new List<string>();
      var current = this;
      do {
        var stack = current.Parent.Children.OfType<Section>().OrderBy(se => se.OrderNr).Cast<Element>().ToList();
        var pos = stack.IndexOf(current);
        if (pos < 0) { pos = 0; }
        res.Add((pos + 1).ToString(CultureInfo.InvariantCulture));
        current = current.Parent as Section;
      } while (current != null && current.Id != stopAtId);
      res.Reverse();
      return String.Concat(".", String.Join(".", res.ToArray()));
    }

    public Opus GetOpus() {
      var e = this;
      if (e.Parent is Opus) return (Opus) e.Parent;
      do {
        e = e.Parent;
      } while (!(e is Opus));
      return (Opus)e;
    }

    # endregion

  }

  #endregion

}
