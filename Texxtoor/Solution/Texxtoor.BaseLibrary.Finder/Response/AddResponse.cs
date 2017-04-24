using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Texxtoor.BusinessLayer.Finder.DTO;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.BusinessLayer.Finder.Response
{
    [DataContract]
    internal class AddResponse
    {
        private List<DTOBase> _added;

        [DataMember(Name = "added")]
        public List<DTOBase> Added { get { return _added; } }

        public AddResponse(ResourceFile newFile, Root root)
        {
            _added = new List<DTOBase>() { DTOBase.Create(newFile, root) };
        }
        public AddResponse(ResourceFolder newDir, Root root)
        {
            _added = new List<DTOBase>() { DTOBase.Create(newDir, root) };
        }
        public AddResponse()
        {
            _added = new List<DTOBase>();
        }
    }
}