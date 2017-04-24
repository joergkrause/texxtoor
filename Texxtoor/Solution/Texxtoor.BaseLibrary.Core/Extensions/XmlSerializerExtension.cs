using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System;

namespace Texxtoor.BaseLibrary.Core.Extensions {

  // TODO: Replace by more efficient serializer

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

    /// <summary>
    /// Get the pure XML of an element as string. If the element's first child is CDATA, the method returns the CDATA's content.
    /// </summary>
    /// <param name="xe"></param>
    /// <returns></returns>
    public static string GetInnerXml(this XElement xe) {
      if (xe.FirstNode != null) {
        if (xe.FirstNode.NodeType == XmlNodeType.CDATA) {
          return ((XCData)xe.FirstNode).Value.Trim().Replace(Environment.NewLine, "");
        }
      }
      var cr = xe.CreateReader();
      cr.MoveToContent();
      return cr.ReadInnerXml().Trim().Replace(Environment.NewLine, "");
    }

  } 
}
