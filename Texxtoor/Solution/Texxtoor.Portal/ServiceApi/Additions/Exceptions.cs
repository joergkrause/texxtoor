using System.Runtime.Serialization;

namespace Texxtoor.Portal.ServiceApi.Services {
  
  [DataContract(Namespace="http://www.texxtoor.com/Fault")]
  public class SignFault {

    [DataMember]
    public string Operation { get; set; }

    [DataMember]
    public string Description { get; set; }

  }

  public class DataFault : SignFault {

    [DataMember]
    public string Data { get; set; }

  }



}