using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Texxtoor.BusinessLayer.Finder.Response
{
    [DataContract]
    internal class Debug
    {
        [DataMember(Name = "connector")]
        public string Connector { get { return ".net"; } }
    }
}
