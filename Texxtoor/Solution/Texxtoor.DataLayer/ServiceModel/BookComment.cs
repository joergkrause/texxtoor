using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Texxtoor.DataModels.ServiceModel {

  [DataContract]
  public class BookComment {

    [DataMember]
    public int BookId {
      get;
      set;
    }

    [DataMember]
    public int? Id {
      get;
      set;
    }

    [DataMember]
    public string Subject {
      get;
      set;
    }

    [DataMember]
    public string Owner {
      get;
      set;
    }

    [DataMember]
    public string Content {
      get;
      set;
    }

    [DataMember]
    public string FragmentCfi {
      get;
      set;
    }

    [DataMember]
    public string NavigationItem {
      get;
      set;
    }

    [DataMember]
    public int? ParentId { get; set; }

    [DataMember]
    public List<BookComment> Children { get; set; }

  }

 
}
