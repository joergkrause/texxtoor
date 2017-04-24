using System;
using Texxtoor.BaseLibrary.Core.Logging;

namespace Texxtoor.ViewModels.Author {

  // everything we need to show sales data
  public class Sales : IViewModel {

    public string ProjectName { get; set; }

    public int ReaderCount { get; set; }

    public int SubscriptionCount { get; set; }

    public int DownloadCount { get; set; }

    public int PrintCount { get; set; }

    public decimal ProjectRevenues { get; set; }

    public decimal AuthorRevenues { get; set; }

    public DateTime Day { get; set; }

  }
}
