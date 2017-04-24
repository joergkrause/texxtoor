﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Texxtoor.BusinessLayer.Finder.DTO
{
    [DataContract]
    internal class ImageDTO : FileDTO
    {   
        [DataMember(Name = "tmb")]
        public object Thumbnail { get; set; }

        [DataMember(Name = "dim")]
        public string Dimension { get; set; }
    }
}