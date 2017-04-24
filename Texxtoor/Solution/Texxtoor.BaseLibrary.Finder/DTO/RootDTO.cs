﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Texxtoor.BusinessLayer.Finder.DTO
{
    [DataContract]
    internal class RootDTO : DTOBase
    {
        [DataMember(Name = "volumeId")]
        public string VolumeId { get; set; }

        [DataMember(Name = "dirs")]
        public byte Dirs { get; set; }
    }
}