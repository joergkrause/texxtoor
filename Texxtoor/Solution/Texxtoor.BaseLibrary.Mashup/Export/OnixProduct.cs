using System.Xml.Serialization;

namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [XmlTypeAttribute(AnonymousType = true)]
  [XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "Product")]
  public partial class OnixProduct21 : IOnixProduct{
    [XmlIgnore]
    public string OnixVersion{
      get { return "2.1";  }
    }

    /// <remarks/>
    public uint RecordReference { get; set; }

    /// <remarks/>
    public byte NotificationType { get; set; }

    /// <remarks/>
    public ProductProductIdentifier ProductIdentifier { get; set; }

    /// <remarks/>
    public string ProductForm { get; set; }

    /// <remarks/>
    public ProductTitle Title { get; set; }

    /// <remarks/>
    public ProductContributor[] Contributor { get; set; }

    /// <remarks/>
    public string EditionTypeCode { get; set; }

    /// <remarks/>
    public byte EditionNumber { get; set; }

    /// <remarks/>
    public ProductLanguage Language { get; set; }

    /// <remarks/>
    public ushort NumberOfPages { get; set; }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("BASICMainSubject")]
    public string BasicMainSubject { get; set; }

    /// <remarks/>
    public byte AudienceCode { get; set; }

    /// <remarks/>
    [XmlElement("OtherText")]
    public ProductOtherText[] OtherText { get; set; }

    /// <remarks/>
    public ProductImprint Imprint { get; set; }

    /// <remarks/>
    public ProductPublisher Publisher { get; set; }

    /// <remarks/>
    public ushort PublicationDate { get; set; }

    /// <remarks/>
    [XmlElement("Measure")]
    public ProductMeasure[] Measure { get; set; }

    /// <remarks/>
    public ProductSupplyDetail SupplyDetail { get; set; }

    public Price Price { get; set; }
  }
}