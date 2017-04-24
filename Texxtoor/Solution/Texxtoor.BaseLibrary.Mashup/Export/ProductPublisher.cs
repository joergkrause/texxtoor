namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductPublisher {

    private byte publishingRoleField;

    private string publisherNameField;

    /// <remarks/>
    public byte PublishingRole {
      get {
        return this.publishingRoleField;
      }
      set {
        this.publishingRoleField = value;
      }
    }

    /// <remarks/>
    public string PublisherName {
      get {
        return this.publisherNameField;
      }
      set {
        this.publisherNameField = value;
      }
    }
  }
}