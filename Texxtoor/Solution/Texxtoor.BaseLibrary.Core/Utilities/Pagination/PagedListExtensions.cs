using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Texxtoor.BaseLibrary.Core.Utilities.Pagination
{
	/// <summary>
	/// Container for extension methods designed to simplify the creation of instances of <see cref="PagedList{T}"/>.
	/// </summary>
	/// <remarks>
	/// Container for extension methods designed to simplify the creation of instances of <see cref="PagedList{T}"/>.
	/// </remarks>
	public static class PagedListExtensions
	{
		/// <summary>
		/// Creates a subset of this collection of objects that can be individually accessed by index and containing metadata about the collection of objects the subset was created from.
		/// </summary>
		/// <typeparam name="T">The type of object the collection should contain.</typeparam>
		/// <param name="superset">The collection of objects to be divided into subsets. If the collection implements <see cref="IQueryable{T}"/>, it will be treated as such.</param>
		/// <param name="index">The index of the subset of objects to be contained by this instance.</param>
		/// <param name="pageSize">The maximum size of any individual subset.</param>
		/// <returns>A subset of this collection of objects that can be individually accessed by index and containing metadata about the collection of objects the subset was created from.</returns>
		/// <seealso cref="PagedList{T}"/>
    public static IPagedList<T> ToPagedList<T>(this IQueryable<T> superset, int index, int pageSize)
		{
			return new PagedList<T>(superset, index, pageSize);
		}

    public static IPagedList<T> ToPagedList<T>(this IQueryable<T> superset, int index, int pageSize, string filterValue, string filterColumn, string orderField, bool desc) {
      Expression<Func<T, bool>> filter = null;
      if (!String.IsNullOrEmpty(filterColumn) && !String.IsNullOrEmpty(filterValue)) {
        var arg = Expression.Parameter(typeof(T), filterColumn);
        var pi = typeof(T).GetProperty(filterColumn);
        var me = Expression.Property(arg, pi);
        var type = pi.PropertyType;
        var innerMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var searchExpression = Expression.Constant(filterValue, type);
        // var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
        // Where(e => e.<Property>.Contains(<value>))
        var expr = Expression.Call(me, innerMethod, new Expression[] { searchExpression });
        filter = (Expression<Func<T, bool>>)Expression.Lambda(expr, arg);        
      }
      return new PagedList<T>(superset, index, pageSize, filter, filterValue, orderField, desc);
    }

	}
}