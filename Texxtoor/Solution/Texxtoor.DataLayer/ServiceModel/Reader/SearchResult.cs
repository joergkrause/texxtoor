using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Texxtoor.DataModels.ServiceModel {

  // Use a data contract as illustrated in the sample below to add composite types to service operations
  [DataContract]
  public class SearchResult {

    [DataMember]
    public int[] BookIds { get; set; }

    [DataMember]
    public string ErrCode { get; set; }

  }

}
