using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Texxtoor.BaseLibrary;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Marketing;

namespace Texxtoor.EasyAuthor.Models {

  public class OverviewDto : OpusDto {
    public OverviewDto() {
    }

    public OverviewDto(string userName, Opus opus, ProjectManager projectUnitOfWork, ReaderManager readerManager) : base(userName, opus, projectUnitOfWork, readerManager) {
      var projId = opus.Project.Id;
      var pckgs = projectUnitOfWork.GetAssignedMarketingPackage(projId) ?? projectUnitOfWork.GetAndAddMarketingPackage(opus.Project);
      if (opus.Published != null) {
        PublishingDate = opus.Published.CreatedAt;
        Keywords = opus.Published.ExternalPublisher.Keywords.Split(';', ',');
        Categories = opus.Published.Catalogs.Select(c => c.Name);
        License = pckgs.GetLocalizedMarketingType();
        Language = new CultureInfo(opus.LocaleId).NativeName;
        Location = "Berlin"; // TODO: Manage publishers and their location
      }
    }

    public string Language { get; set; }

    public string Publisher { get; set; }

    public DateTime? PublishingDate { get; set; }

    public string Location { get; set; }

    public IEnumerable<string> Keywords { get; set; }

    public IEnumerable<string> Categories { get; set; }

    public string License { get; set; }


  }
}