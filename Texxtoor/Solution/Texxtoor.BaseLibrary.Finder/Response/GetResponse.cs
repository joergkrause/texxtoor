using System.Runtime.Serialization;

namespace Texxtoor.BusinessLayer.Finder.Response
{
    [DataContract]
    internal class GetResponse
    {
        [DataMember(Name="content")]
        public string Content { get; set; }
    }
}