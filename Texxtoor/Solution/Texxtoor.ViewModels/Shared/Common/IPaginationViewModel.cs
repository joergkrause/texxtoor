using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.ViewModels.Common {
  public interface IPaginationViewModel {

    bool Descending { get; set; }
    bool HasNextPage { get; }
    bool HasPreviousPage { get; }

    //Type ModelType { get; set; }
    IEnumerable<string> GetOrderColumns();
    string LocalizedOrderColumn(string orderColumn);
    string Order { get; set; }

    string PageActionLink { get; set; }

    string CreatePageLink(int? page = null, string order = null, bool? dir = null, int? pageSize = null);

    int PageIndex { get; set; }
    int PageSize { get; set; }

    int FilterCount { get; set; }
    int TotalCount { get; set; }
    int TotalPages { get; set; }
    bool WithPages { get; set; }

    // injected global buttons
    IList<IDictionary<string, string>> AddButtons { get; set; }

    Dictionary<string, FilterUIHintAttribute> FilterColumns { get; }

    string LocalizedFilterColumn(string filterColumn);
    string FilterValue { get; set; }
  }

}
