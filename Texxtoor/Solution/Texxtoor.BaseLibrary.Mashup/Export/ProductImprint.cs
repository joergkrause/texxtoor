namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductImprint {

    private string imprintNameField;

    /// <remarks/>
    public string ImprintName {
      get {
        return this.imprintNameField;
      }
      set {
        this.imprintNameField = value;
      }
    }
  }
}