using System.Collections.Generic;
using Texxtoor.BaseLibrary.Core.Logging;

namespace Texxtoor.ViewModels.Author {

  // everything we need to show sales data
  public class Revenues : IViewModel {

    public enum DateFilter {
      ToDay,
      ThisWeek,
      LastWeek,
      ThisMonth,
      LastMonth,
      ThisQuarty,
      LastQuarter,
      ThisYear,
      LastYear,
      Last12Month,
      Last24Month,
      Custom,
      All
    }

    public DateFilter Filter { get; set; }

    public List<Sales> Sales { get; set; }

  }
}
