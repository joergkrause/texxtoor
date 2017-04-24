using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Texxtoor.BaseLibrary.Core.Extensions;

namespace Texxtoor.BaseLibrary.Core.Utilities.Pagination {
  /// <summary>
  /// Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
  /// </summary>
  /// <remarks>
  /// Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
  /// </remarks>
  /// <typeparam name="T">The type of object the collection should contain.</typeparam>
  /// <seealso cref="IPagedList{T}"/>
  /// <seealso cref="BasePagedList{T}"/>
  /// <seealso cref="StaticPagedList{T}"/>
  /// <seealso cref="List{T}"/>
  public class PagedList<T> : BasePagedList<T> {
    /// <summary>
    /// Initializes a new instance of the <see cref="PagedList{T}"/> class that divides the supplied superset into subsets the size of the supplied pageSize. The instance then only containes the objects contained in the subset specified by index.
    /// </summary>
    /// <param name="superset">The collection of objects to be divided into subsets. If the collection implements <see cref="IQueryable{T}"/>, it will be treated as such.</param>
    /// <param name="index">The index of the subset of objects to be contained by this instance.</param>
    /// <param name="pageSize">The maximum size of any individual subset.</param>
    /// <exception cref="ArgumentOutOfRangeException">The specified index cannot be less than zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The specified page size cannot be less than one.</exception>
    public PagedList(IQueryable<T> superset, int index, int pageSize)
      : this(superset ?? new List<T>().AsQueryable(), index, pageSize, null, String.Empty, null, false) {
    }

    public PagedList(IQueryable<T> superset, int index, int pageSize, Expression<Func<T, bool>> filter, string filterValue, string order, bool desc)
      : base(index, pageSize, superset.Count(), filter, order, desc) {
      // keep values
      FilterValue = filterValue;
      Order = order;
      Descending = desc;
      // order, than page
      if (!String.IsNullOrEmpty(Order)) {
        superset = desc ? superset.OrderByDescending(Order) : superset.OrderBy<T>(Order);
      }
      if (Filter != null) {
        superset = superset.Where(Filter);
        FilterItemCount = superset.Count();
      }
      else {
        FilterItemCount = TotalItemCount;
      }
      // check index
      var ind = (PageCount > index) ? index : ((PageCount > 0) ? PageCount - 1 : 0);
      // add items to internal list
      if (TotalItemCount > 0)
        AddRange(ind == 0 ? superset.Take(PageSize).ToList() : superset.Skip((ind) * PageSize).Take(PageSize).ToList());
    }
  }
}