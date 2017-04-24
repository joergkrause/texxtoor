using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Texxtoor.DataModels.ServiceModel {

  [Serializable]
  public class SessionData {

    public int UserId { get; set; }
    public int SearchCount { get; set; }

    public static SessionData Deserialize(string data) {
      var xs = new XmlSerializer(typeof(SessionData));
      using (var sr = new StringReader(data)) {
        return xs.Deserialize(sr) as SessionData;
      }
    }

    public override string ToString() {
      var xs = new XmlSerializer(typeof(SessionData));
      var sb = new StringBuilder();
      using (var sw = new StringWriter(sb)) {
        xs.Serialize(sw, this);
        return sb.ToString();
      }
    }


  }

}