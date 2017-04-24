using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using Texxtoor.BusinessLayer.Finder.DTO;

namespace Texxtoor.BusinessLayer.Finder.Response
{
    [DataContract]
    internal class InitResponse : OpenResponseBase
    {
        private static string[] _empty = new string[0];
        [DataMember(Name="api")]
        public string Api { get { return "2.0"; } }

        [DataMember(Name = "uplMaxSize")]
        public string UploadMaxSize { get; set; }

        [DataMember(Name = "netDrivers")]
        public IEnumerable<string> NetDrivers { get { return _empty; } }

        public InitResponse(DTOBase currentWorkingDirectory, Options options)
            : base(currentWorkingDirectory)
        {
            Options = options;
        }
    }
}