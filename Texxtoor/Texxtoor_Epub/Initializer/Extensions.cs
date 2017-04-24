using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Texxtoor.Editor {
  public static class Extensions {
    public static string ToXml<T>(this T toSerialize) {
      var serializer = new XmlSerializer(typeof(T));
      var sb = new StringBuilder();
      using (var writer = new StringWriter(sb))
        serializer.Serialize(writer, toSerialize);
      return sb.ToString();
    }

    public static T DeserializeXmlString<T>(this string xml) {
      var serializer = new XmlSerializer(typeof(T));
      using (var reader = new StringReader(xml))
        return (T)serializer.Deserialize(reader);
    }

    public static string GetInnerXml(this XElement xe) {
      var cr = xe.CreateReader();
      cr.MoveToContent();
      return cr.ReadInnerXml().Trim().Replace(Environment.NewLine, "");
    }

    public static T GetEnumAttribute<T>(this XElement e, XName attribute) where T : struct, IConvertible {
      var val = e.Attribute(attribute).GetNullSafeValue();
      return (T)Enum.Parse(typeof(T), val, true);
    }

    public static T GetEnumValue<T>(this XElement e, XName childElement) where T : struct, IConvertible {
      var val = e.Element(childElement).GetNullSafeValue();
      return (T)Enum.Parse(typeof(T), val, true);
    }

    public static string GetNullSafeValue(this XAttribute a) {
      return a != null ? a.Value : String.Empty;
    }

    public static string GetNullSafeValue(this XElement e) {
      return e != null ? e.Value : String.Empty;
    }

  }
}
