namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductTitleTitleText {

    private byte textcaseField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte textcase {
      get {
        return this.textcaseField;
      }
      set {
        this.textcaseField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value {
      get {
        return this.valueField;
      }
      set {
        this.valueField = value;
      }
    }
  }
}