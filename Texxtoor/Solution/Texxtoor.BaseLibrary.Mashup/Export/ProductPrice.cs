using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.BaseLibrary.Mashup.Export {

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class Price {

    private byte priceTypeCodeField;

    private byte priceStatusField;

    private decimal priceAmountField;

    private string currencyCodeField;

    private string countryCodeField;

    private string taxRateCode1Field;

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
    public byte PriceStatus {
      get {
        return this.priceStatusField;
      }
      set {
        this.priceStatusField = value;
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

    /// <remarks/>
    public string CurrencyCode {
      get {
        return this.currencyCodeField;
      }
      set {
        this.currencyCodeField = value;
      }
    }

    /// <remarks/>
    public string CountryCode {
      get {
        return this.countryCodeField;
      }
      set {
        this.countryCodeField = value;
      }
    }

    /// <remarks/>
    public string TaxRateCode1 {
      get {
        return this.taxRateCode1Field;
      }
      set {
        this.taxRateCode1Field = value;
      }
    }
  }


}
