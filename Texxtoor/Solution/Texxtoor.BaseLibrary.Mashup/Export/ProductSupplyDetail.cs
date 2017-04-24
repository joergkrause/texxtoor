namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductSupplyDetail {

    private uint supplierSANField;

    private string availabilityCodeField;

    private ProductSupplyDetailPrice priceField;

    /// <remarks/>
    public uint SupplierSAN {
      get {
        return this.supplierSANField;
      }
      set {
        this.supplierSANField = value;
      }
    }

    /// <remarks/>
    public string AvailabilityCode {
      get {
        return this.availabilityCodeField;
      }
      set {
        this.availabilityCodeField = value;
      }
    }

    /// <remarks/>
    public ProductSupplyDetailPrice Price {
      get {
        return this.priceField;
      }
      set {
        this.priceField = value;
      }
    }
  }
}