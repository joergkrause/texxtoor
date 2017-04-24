namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductMeasure {

    private byte measureTypeCodeField;

    private decimal measurementField;

    private string measureUnitCodeField;

    /// <remarks/>
    public byte MeasureTypeCode {
      get {
        return this.measureTypeCodeField;
      }
      set {
        this.measureTypeCodeField = value;
      }
    }

    /// <remarks/>
    public decimal Measurement {
      get {
        return this.measurementField;
      }
      set {
        this.measurementField = value;
      }
    }

    /// <remarks/>
    public string MeasureUnitCode {
      get {
        return this.measureUnitCodeField;
      }
      set {
        this.measureUnitCodeField = value;
      }
    }
  }
}