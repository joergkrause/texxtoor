namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductOtherText {

    private byte textTypeCodeField;

    private string textField;

    /// <remarks/>
    public byte TextTypeCode {
      get {
        return this.textTypeCodeField;
      }
      set {
        this.textTypeCodeField = value;
      }
    }

    /// <remarks/>
    public string Text {
      get {
        return this.textField;
      }
      set {
        this.textField = value;
      }
    }
  }
}