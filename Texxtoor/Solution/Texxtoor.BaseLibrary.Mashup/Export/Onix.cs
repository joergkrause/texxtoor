using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Texxtoor.BaseLibrary.Mashup.Export{

  /// <summary>
  /// Supports the export of books and ebook descriptions for libraries and grossery.
  /// </summary>
  public class Onix{

    public enum OnixVersion{
      V21,
      V30
    }

    public Onix(){
      _product = new Dictionary<string, IOnixProduct>();
    }

    public Onix(OnixVersion forVersion)
      : this(){
      _version = forVersion;
    }

    private readonly IDictionary<string, IOnixProduct> _product;
    private readonly OnixVersion _version = OnixVersion.V21;

    public IOnixProduct GetProduct(string culture){
      if (!_product.ContainsKey(culture)){
        _product.Add(culture, OnixProductFactory(_version));
      }
      return _product[culture];
    }

    public string GetOnixAsString(string culture){
      CheckArgument(culture);
      var onix = _product[culture];
      var xs = new XmlSerializer(onix.GetType());
      var sb = new StringBuilder();
      string result;
      using (var sw = new StringWriter(sb)) {
        xs.Serialize(sw, this);
        result = sb.ToString();
      }
      return result;
    } 

    public Stream GetOnixAsStream(string culture){
      CheckArgument(culture);
      var onix = GetOnixAsString(culture);
      var bytes = Encoding.UTF8.GetBytes(onix);
      var result = new MemoryStream(bytes) { Position = 0 };
      return result;
    }

    private void CheckArgument(string culture){
      if (!_product.ContainsKey(culture)) {
        throw new ArgumentOutOfRangeException("culture");
      }
    }

    private static IOnixProduct OnixProductFactory(OnixVersion version){
      switch (version){
        case OnixVersion.V21:
          return new OnixProduct21();
        case OnixVersion.V30:
          return null; // currently not supported
      }
      throw new ArgumentOutOfRangeException("version");
    }
  }
}

