using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Author;
using System.Globalization;

namespace Texxtoor.DataModels.Models.Content
{
    /// <summary>
    /// This is just a marker interface with nothing to store, just support the builder
    /// </summary>
    [Table("Elements", Schema = "Content")]
    public abstract class NumberedSnippet : Snippet, INumberingSchema
    {

        [NotMapped]
        public string Label { get; set; }
        [NotMapped]
        public int Major { get; set; }
        [NotMapped]
        public string MajorString
        {
            get { return Major == 0 ? "" : Major.ToString(CultureInfo.InvariantCulture); }
        }
        [NotMapped]
        public int Minor { get; set; }

        [NotMapped]
        public string MinorString
        {
            get { return Minor == 0 ? "" : Minor.ToString(CultureInfo.InvariantCulture); }
        }

        [NotMapped]
        public string Separator { get; set; }
        [NotMapped]
        public string Divider { get; set; }
        [NotMapped]
        public bool IncludeParent { get; set; }
    }

    public sealed class NumberingSchema : INumberingSchema
    {

        public string Label { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public string Separator { get; set; }
        public string Divider { get; set; }
        public bool IncludeParent { get; set; }
    }

    public interface INumberingSchema
    {

        string Label { get; set; }
        int Major { get; set; }
        int Minor { get; set; }
        string Separator { get; set; }
        string Divider { get; set; }
        bool IncludeParent { get; set; }
    }
}
