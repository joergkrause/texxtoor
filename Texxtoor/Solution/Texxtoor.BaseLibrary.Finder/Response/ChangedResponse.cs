using System.Collections.Generic;
using System.Runtime.Serialization;
using Texxtoor.BusinessLayer.Finder.DTO;

namespace Texxtoor.BusinessLayer.Finder.Response
{
    [DataContract]
    internal class ChangedResponse
    {
        [DataMember(Name="changed")]
        public List<FileDTO> Changed { get; private set; }

        public ChangedResponse()
        {
            Changed = new List<FileDTO>();
        }
    }
}