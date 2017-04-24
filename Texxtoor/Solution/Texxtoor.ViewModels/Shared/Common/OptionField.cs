using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.ViewModels.Common {
  
  /// <summary>
  /// This class encapsulates a button definition used to create a sorted and splitted view for option fields (lists with many item options)
  /// </summary>
  public abstract class OptionField {

    public enum LinkType {
      Link,
      Click
    }

    protected OptionField(string id, string url, string text, uint order, bool important, string styles, IDictionary<string, object> properties)
    : this(id, url, text, order, important, styles){
      HtmlProperties = properties;
    }

    protected OptionField(string id, string url, string text, uint order, bool important, string styles = null) {
      Id = id;
      Url = url;
      Text = text;
      Order = order;
      Important = important;
      AdditionalStyles = styles;
    }

    public string Id { get; set; }

    public string Text { get; set; }

    public string Url { get; set; }

    public abstract LinkType Type { get; }

    public bool Important { get; set; }

    public bool SimpleMode { get; set; }

    public uint Order { get; set; }

    public string AdditionalStyles { get; set; }

    public IDictionary<string, object> HtmlProperties { get; set; }

  }

  public class OptionLinkField : OptionField {

    public OptionLinkField(string id, bool important, string url, string text)
      : base(id, url, text, 0, important, null) {
    }

    public OptionLinkField(string id, bool important, string url, string text, string styles)
      : base(id, url, text, 0, important, styles) {
    }

    public OptionLinkField(string id, bool important, string url, string text, uint order, string styles = null)
      : base(id, url, text, order, important, styles) {      
    }

    public override OptionField.LinkType Type {
      get { return LinkType.Link; }
    }
  }

  public class OptionClickField : OptionField {

    public OptionClickField(string id, bool important, string url, string text)
      : base(id, url, text, 0, important, null) {
    }

    public OptionClickField(string id, bool important, string url, string text, string styles)
      : base(id, url, text, 0, important, styles) {
    }

    public OptionClickField(string id, bool important, string url, string text, uint order, string styles = null)
      : base(id, url, text, order, important, styles) {
    }

    public override OptionField.LinkType Type {
      get { return LinkType.Click; }
    }
  }

  public class OptionDropField : OptionField {
    public OptionDropField(string id, bool important, string url, string text, IDictionary<string, object> properties = null)
      : base(id, url, text, 0, important, null, null) {
    }

    public OptionDropField(string id, bool important, string url, string text, string styles, IDictionary<string, object> properties)
      : base(id, url, text, 0, important, styles, properties) {
    }

    public OptionDropField(string id, bool important, string url, string text, uint order, string styles, IDictionary<string, object> properties)
      : base(id, url, text, order, important, styles, properties) {
    }

    public override OptionField.LinkType Type {
      get { return LinkType.Click; }
    }
  }

}
