namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductSupplyDetailPrice {

    private byte priceTypeCodeField;

    private decimal priceAmountField;

    /// <remarks/>
    public byte PriceTypeCode {
      get {
        return this.priceTypeCodeField;
      }
      set {
        this.priceTypeCodeField = value;
      }
    }

    /// <remarks/>
    public decimal PriceAmount {
      get {
        return this.priceAmountField;
      }
      set {
        this.priceAmountField = value;
      }
    }
  }
}