using System;
using System.Collections.Generic;
using System.Linq;
using Texxtoor.Models.BaseEntities;

namespace Texxtoor.Editor.Core.Extensions {

  public static class LinqExtensions {

    private delegate Func<A, R> Recursive<A, R>(Recursive<A, R> r);

    private static Func<A, R> Y<A, R>(Func<Func<A, R>, Func<A, R>> f) {
      Recursive<A, R> rec = r => a => f(r(r))(a);
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
        if (node.HasChildren()) {
          foreach (var child in node.Children.FlattenHierarchy()) {
            yield return child;
          }
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

    ///<summary>
    /// Implementes a for each with a break
    ///</summary>
    ///<param name="sequence"></param>
    ///<param name="action">the action to be performed, return false to end the foreach loop</param>
    ///<typeparam name="T"></typeparam>
    ///<exception cref="ArgumentNullException"></exception>
    public static void ForEach<T>(this IEnumerable<T> sequence, Func<T, bool> action) {
      if (sequence == null) throw new ArgumentNullException("sequence");
      if (action == null) throw new ArgumentNullException("action");

      if (sequence.Any(item => !action(item))) {
        return;
      }
    }

  }
}
