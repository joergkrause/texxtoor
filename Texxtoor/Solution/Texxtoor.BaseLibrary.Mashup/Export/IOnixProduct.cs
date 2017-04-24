namespace Texxtoor.BaseLibrary.Mashup.Export{
  public interface IOnixProduct{
    /// <remarks/>
    uint RecordReference { get; set; }

    /// <remarks/>
    byte NotificationType { get; set; }

    /// <remarks/>
    ProductProductIdentifier ProductIdentifier { get; set; }

    /// <remarks/>
    string ProductForm { get; set; }

    /// <remarks/>
    ProductTitle Title { get; set; }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Contributor")]
    ProductContributor[] Contributor { get; set; }

    /// <remarks/>
    string EditionTypeCode { get; set; }

    /// <remarks/>
    byte EditionNumber { get; set; }

    /// <remarks/>
    ProductLanguage Language { get; set; }

    /// <remarks/>
    ushort NumberOfPages { get; set; }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("BASICMainSubject")]
    string BasicMainSubject { get; set; }

    /// <remarks/>
    byte AudienceCode { get; set; }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("OtherText")]
    ProductOtherText[] OtherText { get; set; }

    /// <remarks/>
    ProductImprint Imprint { get; set; }

    /// <remarks/>
    ProductPublisher Publisher { get; set; }

    /// <remarks/>
    ushort PublicationDate { get; set; }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Measure")]
    ProductMeasure[] Measure { get; set; }

    /// <remarks/>
    ProductSupplyDetail SupplyDetail { get; set; }

    Price Price { get; set; }
  }
}