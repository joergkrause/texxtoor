using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.BaseLibrary.Core.Extensions {

  public static class LinqExtensions {

    public static ProjectionExpression<TSource> Project<TSource>(this IQueryable<TSource> source) {
      return new ProjectionExpression<TSource>(source);
    }

    private delegate Func<TA, TR> Recursive<TA, TR>(Recursive<TA, TR> r);

    private static Func<TA, TR> Y<TA, TR>(Func<Func<TA, TR>, Func<TA, TR>> f) {
      Recursive<TA, TR> rec = r => a => f(r(r))(a);
      return rec(rec);
    }

    public static IEnumerable<T> Traverse<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : IHierarchyBase<T> {
      var traverse = LinqExtensions.Y<IEnumerable<T>, IEnumerable<T>>(
        f => items => {
          var r = new List<T>(items.Where(predicate));
          r.AddRange(items.SelectMany(i => f(i.Children)));
          return r;
        });
      return traverse(source);
    }

    public static IEnumerable<T> Iterate<T>(this IEnumerable<T> enumerable, Action<T> callback) {
      if (enumerable == null) {
        throw new ArgumentNullException("enumerable");
      }

      IterateHelper(enumerable, (x, i) => callback(x));

      return enumerable;
    }

    public static void Iterate<T>(this IEnumerable<T> enumerable, Action<T, int> callback) {
      if (enumerable == null) {
        throw new ArgumentNullException("enumerable");
      }

      IterateHelper(enumerable, callback);
    }

    private static void IterateHelper<T>(this IEnumerable<T> enumerable, Action<T, int> callback) {
      int count = 0;
      foreach (var cur in enumerable) {
        callback(cur, count);
        count++;
      }
    }

    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> act) {
      for (int i = 0; i < source.Count(); act(source.ElementAt(i)), i++) ;
      return source;
    }

    public static IEnumerable<T> FlattenHierarchy<T>(this IEnumerable<T> nodes) where T : IHierarchyBase<T> {
      foreach (var node in nodes) {
        yield return node;
        if (!node.HasChildren()) continue;
        foreach (var child in node.Children.FlattenHierarchy()) {
          yield return child;
        }
      }
    }

    public static IEnumerable<TOut> RecursiveSelect<TIn, TOut>(this IEnumerable<TIn> source, Func<TIn, IEnumerable<TIn>> fnRecurse, Func<TIn, IEnumerable<TOut>, TOut> projection) {
      return source.Select(item => projection(item, RecursiveSelect(fnRecurse(item) ?? new TIn[] { }, fnRecurse, projection)));
    }

    public static void RecursiveForEach<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> fnRecurse, Action<T> action) {
      source.ForEach(e => {
        RecursiveForEach(fnRecurse(e) ?? new T[] { }, fnRecurse, action);
        action(e);
      });
    }

    public static IEnumerable<T> MaskToList<T>(Enum mask) where T : struct {
      if (typeof(T).IsSubclassOf(typeof(Enum)) == false)
        throw new ArgumentException();

      return Enum.GetValues(typeof(T))
                           .Cast<Enum>()
                           .Where(m => mask.HasFlag(m))
                           .Cast<T>();
    }

    # region dynamic order and filter

    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property) {
      return ApplyOrder<T>(source.AsQueryable(), property, "OrderBy");
    }
    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property) {
      return ApplyOrder<T>(source, property, "OrderByDescending");
    }
    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property) {
      return ApplyOrder<T>(source, property, "ThenBy");
    }
    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property) {
      return ApplyOrder<T>(source, property, "ThenByDescending");
    }
    static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName) {
      string[] props = property.Split('.');
      Type type = typeof(T);
      ParameterExpression arg = Expression.Parameter(type, "x");
      Expression expr = arg;
      foreach (string prop in props) {
        // use reflection (not ComponentModel) to mirror LINQ
        PropertyInfo pi = type.GetProperty(prop);
        expr = Expression.Property(expr, pi);
        type = pi.PropertyType;
      }
      Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
      object result = null;
      //BinaryExpression lambdaWhere = Expression.NotEqual(expr, Expression.Constant(null));
      //result = typeof(Queryable).GetMethods().Single(
      //  method => method.Name == "Where"
      //          && method.IsGenericMethodDefinition
      //          && method.GetGenericArguments().Length == 2
      //          && method.GetParameters().Length == 2)
      //  .MakeGenericMethod(typeof(T), type)
      //  .Invoke(null, new object[] { source, lambdaWhere });
      LambdaExpression lambdaOrder = Expression.Lambda(delegateType, expr, arg);      
      result = typeof(Queryable).GetMethods().Single(
              method => method.Name == methodName
                      && method.IsGenericMethodDefinition
                      && method.GetGenericArguments().Length == 2
                      && method.GetParameters().Length == 2)
              .MakeGenericMethod(typeof(T), type)
              .Invoke(null, new object[] { source, lambdaOrder });
      return (IOrderedQueryable<T>)result;
    }

    # endregion

  }
}
