using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Texxtoor.Portal.Core.Extensions.EditorForModel {

  internal delegate bool TryGetValueDelegate(object dictionary, string key, out object value);

  internal static class TypeHelpers {
    private static readonly Dictionary<Type, TryGetValueDelegate> _tryGetValueDelegateCache = new Dictionary<Type, TryGetValueDelegate>();
    private static readonly ReaderWriterLockSlim _tryGetValueDelegateCacheLock = new ReaderWriterLockSlim();
    private static readonly MethodInfo _strongTryGetValueImplInfo = typeof(TypeHelpers).GetMethod("StrongTryGetValueImpl", BindingFlags.Static | BindingFlags.NonPublic);
    public static readonly Assembly MsCorLibAssembly = typeof(string).Assembly;
    public static readonly Assembly MvcAssembly = typeof(Controller).Assembly;
    public static readonly Assembly SystemWebAssembly = typeof(HttpContext).Assembly;

    static TypeHelpers() {
    }

    public static TDelegate CreateDelegate<TDelegate>(Assembly assembly, string typeName, string methodName, object thisParameter) where TDelegate : class {
      Type type = assembly.GetType(typeName, false);
      if (type == (Type)null)
        return default(TDelegate);
      else
        return TypeHelpers.CreateDelegate<TDelegate>(type, methodName, thisParameter);
    }

    public static TDelegate CreateDelegate<TDelegate>(Type targetType, string methodName, object thisParameter) where TDelegate : class {
      Type[] types = Array.ConvertAll<ParameterInfo, Type>(typeof(TDelegate).GetMethod("Invoke").GetParameters(), (Converter<ParameterInfo, Type>)(pInfo => pInfo.ParameterType));
      MethodInfo method = targetType.GetMethod(methodName, types);
      if (method == (MethodInfo)null)
        return default(TDelegate);
      else
        return Delegate.CreateDelegate(typeof(TDelegate), thisParameter, method, false) as TDelegate;
    }

    public static TryGetValueDelegate CreateTryGetValueDelegate(Type targetType) {
      TypeHelpers._tryGetValueDelegateCacheLock.EnterReadLock();
      TryGetValueDelegate getValueDelegate;
      try {
        if (TypeHelpers._tryGetValueDelegateCache.TryGetValue(targetType, out getValueDelegate))
          return getValueDelegate;
      } finally {
        TypeHelpers._tryGetValueDelegateCacheLock.ExitReadLock();
      }
      Type genericInterface = TypeHelpers.ExtractGenericInterface(targetType, typeof(IDictionary<,>));
      if (genericInterface != (Type)null) {
        Type[] genericArguments = genericInterface.GetGenericArguments();
        Type type1 = genericArguments[0];
        Type type2 = genericArguments[1];
        if (type1.IsAssignableFrom(typeof(string)))
          getValueDelegate = (TryGetValueDelegate)Delegate.CreateDelegate(typeof(TryGetValueDelegate), TypeHelpers._strongTryGetValueImplInfo.MakeGenericMethod(type1, type2));
      }
      if (getValueDelegate == null && typeof(IDictionary).IsAssignableFrom(targetType))
        getValueDelegate = new TryGetValueDelegate(TypeHelpers.TryGetValueFromNonGenericDictionary);
      TypeHelpers._tryGetValueDelegateCacheLock.EnterWriteLock();
      try {
        TypeHelpers._tryGetValueDelegateCache[targetType] = getValueDelegate;
      } finally {
        TypeHelpers._tryGetValueDelegateCacheLock.ExitWriteLock();
      }
      return getValueDelegate;
    }

    public static Type ExtractGenericInterface(Type queryType, Type interfaceType) {
      Func<Type, bool> predicate = (Func<Type, bool>)(t => {
        if (t.IsGenericType)
          return t.GetGenericTypeDefinition() == interfaceType;
        else
          return false;
      });
      if (!predicate(queryType))
        return ((IEnumerable<Type>)queryType.GetInterfaces()).FirstOrDefault(predicate);
      else
        return queryType;
    }

    public static object GetDefaultValue(Type type) {
      if (!TypeHelpers.TypeAllowsNullValue(type))
        return Activator.CreateInstance(type);
      else
        return (object)null;
    }

    public static bool IsCompatibleObject<T>(object value) {
      if (value is T)
        return true;
      if (value == null)
        return TypeHelpers.TypeAllowsNullValue(typeof(T));
      else
        return false;
    }

    public static bool IsNullableValueType(Type type) {
      return Nullable.GetUnderlyingType(type) != (Type)null;
    }

    private static bool StrongTryGetValueImpl<TKey, TValue>(object dictionary, TKey key, out object value) {
      TValue obj;
      bool flag = ((IDictionary<TKey, TValue>)dictionary).TryGetValue(key, out obj);
      value = (object)obj;
      return flag;
    }

    private static bool TryGetValueFromNonGenericDictionary(object dictionary, string key, out object value) {
      IDictionary dictionary1 = (IDictionary)dictionary;
      bool flag = dictionary1.Contains((object)key);
      value = flag ? dictionary1[(object)key] : (object)null;
      return flag;
    }

    public static bool TypeAllowsNullValue(Type type) {
      if (type.IsValueType)
        return TypeHelpers.IsNullableValueType(type);
      else
        return true;
    }
  }
}