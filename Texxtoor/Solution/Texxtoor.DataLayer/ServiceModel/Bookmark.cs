using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Texxtoor.DataModels.ServiceModel {

  [DataContract]
  public class Bookmark {

    [DataMember]
    public int Id {
      get;
      set;
    }

    [DataMember]
    public int BookId {
      get;
      set;
    }
    
    [DataMember]
    public string FragmentCfi {
      get;
      set;
    }

  }

}
