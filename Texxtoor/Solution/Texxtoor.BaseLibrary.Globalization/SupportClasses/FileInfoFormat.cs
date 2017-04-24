using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Texxtoor.BaseLibrary.Globalization.Support
{
    /// <summary>
    /// Internal structure that contains format information about a file
    /// resource. Used internally to figure out how to write 
    /// a resource into the database
    /// </summary>
    internal class FileInfoFormat
    {
        public string FileName = "";
        public string Encoding = "";
        public byte[] BinContent = null;
        public string TextContent = "";
        public FileFormatTypes FileFormatType = FileFormatTypes.Binary;
        public string ValueString = "";
        public string Type = "File";
    }
}
