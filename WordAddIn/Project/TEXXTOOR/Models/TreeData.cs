using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEXXTOOR
{
    public class TreeData
    {
        public string data { get; set; }
        public Attribute attr { get; set; }
        public List<TreeData> children { get; set; }
    }
    public class Attribute
    {
        public string id { get; set; }
        public string rel { get; set; }
        public string dataitem { get; set; }
        public string datatext { get; set; }
    }
    
}
