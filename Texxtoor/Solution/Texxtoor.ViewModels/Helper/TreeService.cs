using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.ViewModels.Common;

namespace Texxtoor.ViewModels.Utilities {

  public static class TreeService {

    public static JsTreeModel[] GetNavigationTreeModel<T>(IList<T> navPoints, Expression<Func<T, object>> dataProperty1, Expression<Func<T, object>> dataProperty2, Expression<Func<T, object>> idProperty) where T : HierarchyBase<T> {
      if (navPoints == null) return null;
      return navPoints.Select(n => new JsTreeModel {
        data = String.Format("{0} ({1})", 
          GetPropertyFromObject<T>(n, dataProperty1), 
          GetPropertyFromObject<T>(n, dataProperty2)), 
          attr = new JsTreeAttribute {
            id = GetPropertyFromObject<T>(n, idProperty), 
            rel = (n.HasChildren()) ? "folder" : "file"
        },
        children = GetNavigationTreeModel(n.Children, dataProperty1, dataProperty2, idProperty)
      }).ToArray();
    }

    public static JsTreeModel[] GetNavigationTreeModel<T>(IList<T> navPoints, Expression<Func<T, object>> dataProperty, Expression<Func<T, object>> idProperty) where T : HierarchyBase<T> {
      if (navPoints == null) return null;
      return navPoints.Select(n => new JsTreeModel {
        data = GetPropertyFromObject<T>(n, dataProperty), 
        attr = new JsTreeAttribute {
          id = GetPropertyFromObject<T>(n, idProperty), 
          rel = (n.HasChildren()) ? "folder" : "file"
        },
        children = GetNavigationTreeModel(n.Children, dataProperty, idProperty)
      }).ToArray();
    }

    private static string GetPropertyFromObject<T>(object source, Expression<Func<T, object>> expression) {
      var exp = expression.Body;
      string name = null;
      if (exp.NodeType == ExpressionType.MemberAccess) {
        name = ((MemberExpression)exp).Member.Name;
      }
      if (exp.NodeType == ExpressionType.Convert) {
        name = ((MemberExpression)((UnaryExpression)exp).Operand).Member.Name;
      }
      if (String.IsNullOrEmpty(name)) return String.Empty;
      var piSource = source.GetType().GetProperty(name);
      if (piSource == null) return String.Empty;
      var value = piSource.GetValue(source, null);
      return value == null ? String.Empty : value.ToString();
    }
    //public static JsTreeModel[] GetCollectionTreeModel(IList<Fragment> fragments) {
    //  if (fragments == null || fragments.Count() == 0) return null;
    //  List<JsTreeModel> ls = new List<JsTreeModel>();
    //  foreach (var n in fragments) {
    //    ls.Add(new JsTreeModel {
    //      data = n.Title.Ellipsis(50).ToHtmlString(),
    //      attr = new JsTreeAttribute {
    //        id = n.ItemHref,
    //        rel = (n.Children != null && n.Children.Count() > 0) ? "folder" : "file"
    //      },
    //      children = GetCollectionTreeModel(n.Children)
    //    });
    //  }
    //  return ls.ToArray();
    //}

    //public static JsTreeModel[] GetCatalogTreeModel(IList<Catalog> catEntries) {
    //  if (catEntries == null) return null;
    //  List<JsTreeModel> ls = new List<JsTreeModel>();
    //  foreach (var n in catEntries) {
    //    ls.Add(new JsTreeModel {
    //      data = n.Name,
    //      attr = new JsTreeAttribute {
    //        id = n.Id.ToString(),
    //        rel = (n.Children != null) ? "folder" : "file"
    //      },
    //      children = GetCatalogTreeModel(n.Children)
    //    });
    //  }
    //  return ls.ToArray();
    //}
  }
}
