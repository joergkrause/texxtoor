using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Texxtoor.DataModels.Context;

namespace Texxtoor.DataModels.Models.Reader.Content {
  public class CatalogTypeConverter : TypeConverter {

    public override bool IsValid(ITypeDescriptorContext context, object value) {
      return true;
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
      if (sourceType == typeof(string)) {
        return true;
      }
      return base.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
      if (destinationType == typeof(IList<Catalog>)) {
        return true;
      }
      return base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
      var stringVal = value as string;
      if (!String.IsNullOrEmpty(stringVal)) {
        var arrayVal = stringVal.Split(',').Select(s => Int32.Parse(s));
        throw new NotImplementedException();
        //return ctx.Catalog.Where(c => arrayVal.Any(a => a == c.Id)).ToList();
      }
      return base.ConvertTo(value, destinationType);
    }
   
    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
      if (value != null) {
        var list = value as IList<Catalog>;
        if (list != null) {
          return String.Join(",", list.Select(s => s.Id.ToString()).ToArray());
        }
      }
      return base.ConvertFrom(context, culture, value);
    }

  }
}
