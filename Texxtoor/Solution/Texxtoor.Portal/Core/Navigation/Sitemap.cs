using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Texxtoor.Portal.Core.Navigation {

  [XmlRoot(ElementName = "urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
  [Serializable]
  public class Sitemap : List<SitemapUrl> {
    [XmlInclude(typeof(SitemapUrl))]
    public void Serialize(TextWriter writer) {
      var serializer = new XmlSerializer(typeof(Sitemap));
      var xmlTextWriter = new XmlTextWriter(writer);
      serializer.Serialize(xmlTextWriter,
                           this);
    }
  }

}