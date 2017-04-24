using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Texxtoor.Portal.ServiceApi.Services.ServiceDtos {
  [DataContract]
  public class ServiceElement {
    [DataMember]
    public int Id { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public IList<ServiceElement> Children { get; set; }
  }

  [DataContract]
  public class ViewDataUploadFilesResult {
    [DataMember]
    public string ThumbnailUrl { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public int Length { get; set; }
    [DataMember]
    public string Type { get; set; }
  }

  [DataContract]
  public class DocumentProperties {

    [DataMember]
    public bool AllowChapters { get; set; }

    [DataMember]
    public bool AllowMetaData { get; set; }

    [DataMember]
    public string ChapterDefault { get; set; }

    [DataMember]
    public string SectionDefault { get; set; }

    [DataMember]
    public string TextSnippetDefault { get; set; }

    [DataMember]
    public string ListingSnippetDefault { get; set; }

    public bool ShowNaviPane { get; set; }

    public bool ShowFlowPane { get; set; }

    [DataMember]
    public bool ShowNumberChain { get; set; }

    [DataMember]
    public string DocumentLanguage { get; set; }

  }

  [DataContract]
  public class Comment {

    [DataMember]
    public string Subject { get; set; }

    [DataMember]
    public string Date { get; set; }

    [DataMember]
    public string Text { get; set; }

    [DataMember]
    public string UserName { get; set; }

  }
}