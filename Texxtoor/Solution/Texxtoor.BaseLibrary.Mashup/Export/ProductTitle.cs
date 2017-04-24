namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductTitle {

    private byte titleTypeField;

    private ProductTitleTitleText titleTextField;
    private ProductTitleTitleText titleSubtitle;

    /// <remarks/>
    public byte TitleType {
      get {
        return this.titleTypeField;
      }
      set {
        this.titleTypeField = value;
      }
    }

    /// <remarks/>
    public ProductTitleTitleText TitleText {
      get {
        return this.titleTextField;
      }
      set {
        this.titleTextField = value;
      }
    }

    public ProductTitleTitleText Subtitle {
      get {
        return this.titleSubtitle;
      }
      set {
        this.titleSubtitle = value;
      }
    }
  }
}