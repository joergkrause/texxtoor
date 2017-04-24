using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Texxtoor.Portal.Core.Navigation {

  public enum ChangeFrequency {
    [XmlEnum(Name = "always")]
    Always,
    [XmlEnum(Name = "hourly")]
    Hourly,
    [XmlEnum(Name = "daily")]
    Daily,
    [XmlEnum(Name = "weekly")]
    Weekly,
    [XmlEnum(Name = "monthly")]
    Monthly,
    [XmlEnum(Name = "yearly")]
    Yearly,
    [XmlEnum(Name = "never")]
    Never
  }
}