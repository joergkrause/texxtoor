using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Texxtoor.DataModels.ServiceModel {

  // Use a data contract as illustrated in the sample below to add composite types to service operations
  [DataContract]
  public class BookMetadata {

    # region Helper For Creating Instances

    public BookMetadata() {
    }

    # endregion Helper For Creating Instances

    [DataMember]
    public string Title {
      get;
      set;
    }

    [DataMember]
    public int BookId {
      get;
      set;
    }

    [DataMember]
    public string Authors {
      get;
      set;
    }

    [DataMember]
    public string[] ItemHref {
      get;
      set;
    }

    [DataMember]
    public int[] ItemSize {
      get;
      set;
    }

    public byte[] Cover {
      get;
      set;
    }

    [DataMember]
    public IList<NavElement> Navigation { get; set; }

  }

}
