using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Marketing;

namespace Texxtoor.BusinessLayer.Repository.Marketing {

  internal class MarketingPackageRepository : Manager<MarketingPackageRepository>
  {
    public MarketingPackage SetMarketingPackage(Project prj, int marketingId, bool unassign) {
      var pckg = Ctx.MarketingPackages.Find(marketingId);
      prj.Marketing = unassign ? null : pckg;
      SaveChanges();
      return pckg;
    }

  }
}
