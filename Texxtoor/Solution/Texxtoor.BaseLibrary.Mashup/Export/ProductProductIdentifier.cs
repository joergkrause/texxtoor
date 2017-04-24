namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductProductIdentifier {

    private byte productIDTypeField;

    private uint iDValueField;

    /// <remarks/>
    public byte ProductIDType {
      get {
        return this.productIDTypeField;
      }
      set {
        this.productIDTypeField = value;
      }
    }

    /// <remarks/>
    public uint IDValue {
      get {
        return this.iDValueField;
      }
      set {
        this.iDValueField = value;
      }
    }
  }
}