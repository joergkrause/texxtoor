namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductLanguage {

    private byte languageRoleField;

    private string languageCodeField;

    /// <remarks/>
    public byte LanguageRole {
      get {
        return this.languageRoleField;
      }
      set {
        this.languageRoleField = value;
      }
    }

    /// <remarks/>
    public string LanguageCode {
      get {
        return this.languageCodeField;
      }
      set {
        this.languageCodeField = value;
      }
    }
  }
}