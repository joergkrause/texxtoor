using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Texxtoor.DataModels.ServiceModel {

  [DataContract]
  public class NavElement {

    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public int OrderNr { get; set; }

    [DataMember]
    public NavElement Parent { get; set; }

    [DataMember]
    public IList<NavElement> Children { get; set; }

    [DataMember]
    public string LabelText { get; set; }

    [DataMember]
    public int PlayOrder { get; set; }

    [DataMember]
    public string Content { get; set; }

    [DataMember]
    public string MetaId { get; set; }

  }

 
}
