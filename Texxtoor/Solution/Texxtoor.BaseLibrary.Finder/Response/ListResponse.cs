using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Texxtoor.BusinessLayer.Finder.Response
{
    [DataContract]
    internal class ListResponse
    {
        [DataMember(Name="list")]
        public List<string> List { get; private set; }

        public ListResponse()
        {
            List = new List<string>();
        }     
    }
}