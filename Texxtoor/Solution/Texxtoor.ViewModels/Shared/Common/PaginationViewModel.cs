using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;

namespace Texxtoor.ViewModels.Common {

  public class PaginationFormModel {

    public PaginationFormModel() {
      PageSize = 5; // global default
    }

    public int Page { get; set; }
    public int PageSize { get; set; }
    public string Order { get; set; }
    public bool Dir { get; set; }
    public string FilterValue { get; set; }
    public string FilterName { get; set; }
  }

  public class PaginationViewModel<T> : IPaginationViewModel {

    public PaginationViewModel() {
      Order = FilterValue = String.Empty;
      Descending = false;
      OrderColumns = new List<Expression<Func<T, object>>>();
      FilterColumns = new Dictionary<string, FilterUIHintAttribute>();
    }

    public static IPaginationViewModel Create(IPagedList<T> model, string updateAction, bool withPages, params Expression<Func<T, object>>[] orderColumns) {
      var listBtns = new List<IDictionary<string, string>>();
      return new PaginationViewModel<T>(model, updateAction, listBtns, withPages, orderColumns);
    }

    public static IPaginationViewModel Create(IPagedList<T> model, string updateAction, IDictionary<string, string> addBtn, bool withPages, params Expression<Func<T, object>>[] orderColumns) {
      var listBtns = new List<IDictionary<string, string>>(new[] { addBtn });
      return new PaginationViewModel<T>(model, updateAction, listBtns, withPages, orderColumns);
    }

    public static IPaginationViewModel Create(IPagedList<T> model, string updateAction, IList<IDictionary<string, string>> addBtns, bool withPages, params Expression<Func<T, object>>[] orderColumns) {
      return new PaginationViewModel<T>(model, updateAction, addBtns, withPages, orderColumns);
    }

    public PaginationViewModel(IPagedList<T> model, string updateAction, IList<IDictionary<string, string>> addBtns, bool withPages, params Expression<Func<T, object>>[] orderColumns) {
      AddButtons = addBtns;
      PageIndex = model.PageIndex;
      PageActionLink = updateAction;
      Order = model.Order;
      Descending = model.Descending;
      WithPages = withPages;
      PageIndex = model.PageNumber;
      TotalPages = model.PageCount;
      TotalCount = model.TotalItemCount;
      FilterCount = model.FilterItemCount;
      FilterValue = model.FilterValue;
      PageSize = model.PageSize;
      OrderColumns = new List<Expression<Func<T, object>>>(orderColumns);
      var filter = typeof(T).GetProperties()
        .Where(p => p.GetCustomAttributes(true).OfType<FilterUIHintAttribute>().Any())
        .Select(p => new {
          p.Name,
          Filter = p.GetCustomAttributes(true).OfType<FilterUIHintAttribute>().Single()
        })
        .ToDictionary(k => k.Name, v => v.Filter);
      FilterColumns = filter;
    }

    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int FilterCount { get; set; }
    public int TotalCount { get; set; }
    public string PageActionLink { get; set; }

    public string CreatePageLink(int? page = null, string order = null, bool? dir = null, int? pageSize = null) {
      var pi = PageIndex;
      var o = Order;
      var ps = PageSize;
      var d = Descending;
      if (pageSize.HasValue) ps = Math.Max(5, pageSize.Value);
      if (!String.IsNullOrEmpty(order)) o = order;
      if (page.HasValue) pi = page.Value;
      if (dir.HasValue) d = dir.Value;
      return PageActionLink
        .Replace("{page}", (pi.ToString(CultureInfo.InvariantCulture)))
        .Replace("{order}", o)
        .Replace("{dir}", d ? "true" : "false")
        .Replace("{pagesize}", ps.ToString(CultureInfo.InvariantCulture));
    }

    public bool HasPreviousPage { get { return (PageIndex > 1); } }
    public bool HasNextPage { get { return (PageIndex * PageSize) < TotalCount; } }
    public string Order { get; set; }
    public string FilterValue { get; set; }
    public bool Descending { get; set; }

    public bool WithPages { get; set; }

    public List<Expression<Func<T, object>>> OrderColumns { get; set; }

    public IEnumerable<string> GetOrderColumns()
    {

      return OrderColumns.Select(e => {
        switch (e.Body.GetType().Name) {
          case "UnaryExpression":
            return ((MemberExpression)((UnaryExpression)e.Body).Operand).Member.Name;
          case "PropertyExpression":
            var m = ((MemberExpression)e.Body);
            if (m.NodeType == ExpressionType.MemberAccess) {
              // here we have a chain ob objects and resolve to dotted notation (e.g. Team.Project.Name)
              string property = m.Member.Name;
              string result = "";
              while (m.Expression.NodeType == ExpressionType.MemberAccess) {
                m = ((MemberExpression)e.Body).Expression as MemberExpression;
                if (m != null) {
                  result += m.Member.Name + ".";
                }
              }
              return result + property;
            }
            return ((MemberExpression)e.Body).Member.Name;
        }
        return "";
      });
    }

    public string LocalizedOrderColumn(string orderColumn) {
      DisplayAttribute attr = null;
      if (orderColumn.Contains(".")) {
        var chain = orderColumn.Split('.');
        Type mt = typeof(T);
        foreach (var segment in chain) {
          var pi = mt.GetProperty(segment);
          if (segment == chain.Last()) {
            attr = mt.GetProperty(segment).GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().SingleOrDefault();
            if (attr == null) return orderColumn;
            return attr.GetName();
          }
          mt = pi.PropertyType;
        }
        return orderColumn;
      } else {
        attr = typeof(T).GetProperty(orderColumn).GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().SingleOrDefault();
        if (attr == null) return orderColumn;
        return attr.GetName();
      }
    }

    public IList<IDictionary<string, string>> AddButtons {
      get;
      set;
    }

    public Dictionary<string, FilterUIHintAttribute> FilterColumns { get; private set; }

    public string LocalizedFilterColumn(string filterColumn) {
      DisplayAttribute attr = null;
      if (filterColumn.Contains(".")) {
        var chain = filterColumn.Split('.');
        Type mt = typeof(T);
        foreach (var segment in chain) {
          var pi = mt.GetProperty(segment);
          if (segment == chain.Last()) {
            attr = mt.GetProperty(segment).GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().SingleOrDefault();
            if (attr == null) return filterColumn;
            return attr.GetName();
          }
          mt = pi.PropertyType;
        }
        return filterColumn;
      } else {
        attr = typeof(T).GetProperty(filterColumn).GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().SingleOrDefault();
        if (attr == null) return filterColumn;
        return attr.GetName();
      }
    }

  }
}