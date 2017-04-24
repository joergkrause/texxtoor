using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Texxtoor.BaseLibrary;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Marketing;

namespace Texxtoor.EasyAuthor.Models {

  public class MetadataDto {

    public string Title { get; set; }

    public string Subtitle { get; set; }

    public string Description { get; set; }

    public string Language { get; set; }

    public string Publisher { get; set; }

    //public DateTime? PublishingDate { get; set; }

    public string Location { get; set; }

    public IEnumerable<string> Keywords { get; set; }

    public IEnumerable<string> Categories { get; set; }

    public string License { get; set; }


  }
}