﻿#if LESSTHAN_NET35

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace System.Linq.Expressions.Interpreter
{
#if FEATURE_MAKE_RUN_METHODS
    internal static partial class DelegateHelpers
    {
        private const int MaximumArity = 17;

        internal static Type MakeDelegate(Type[] types)
        {
            Debug.Assert(types != null && types.Length > 0);

            // Can only used predefined delegates if we have no byref types and
            // the arity is small enough to fit in Func<...> or Action<...>
            if (types.Length > MaximumArity || types.Any(t => t.IsByRef))
            {
                throw ContractUtils.Unreachable;
            }

            Type returnType = types[types.Length - 1];
            if (returnType == typeof(void))
            {
                Array.Resize(ref types, types.Length - 1);
                switch (types.Length)
                {
                    case 0: return typeof(Action);

                    case 1: return typeof(Action<>).MakeGenericType(types);
                    case 2: return typeof(Action<,>).MakeGenericType(types);
                    case 3: return typeof(Action<,,>).MakeGenericType(types);
                    case 4: return typeof(Action<,,,>).MakeGenericType(types);
                    case 5: return typeof(Action<,,,,>).MakeGenericType(types);
                    case 6: return typeof(Action<,,,,,>).MakeGenericType(types);
                    case 7: return typeof(Action<,,,,,,>).MakeGenericType(types);
                    case 8: return typeof(Action<,,,,,,,>).MakeGenericType(types);
                    case 9: return typeof(Action<,,,,,,,,>).MakeGenericType(types);
                    case 10: return typeof(Action<,,,,,,,,,>).MakeGenericType(types);
                    case 11: return typeof(Action<,,,,,,,,,,>).MakeGenericType(types);
                    case 12: return typeof(Action<,,,,,,,,,,,>).MakeGenericType(types);
                    case 13: return typeof(Action<,,,,,,,,,,,,>).MakeGenericType(types);
                    case 14: return typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(types);
                    case 15: return typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(types);
                    case 16: return typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                }
            }
            else
            {
                switch (types.Length)
                {
                    case 1: return typeof(Func<>).MakeGenericType(types);
                    case 2: return typeof(Func<,>).MakeGenericType(types);
                    case 3: return typeof(Func<,,>).MakeGenericType(types);
                    case 4: return typeof(Func<,,,>).MakeGenericType(types);
                    case 5: return typeof(Func<,,,,>).MakeGenericType(types);
                    case 6: return typeof(Func<,,,,,>).MakeGenericType(types);
                    case 7: return typeof(Func<,,,,,,>).MakeGenericType(types);
                    case 8: return typeof(Func<,,,,,,,>).MakeGenericType(types);
                    case 9: return typeof(Func<,,,,,,,,>).MakeGenericType(types);
                    case 10: return typeof(Func<,,,,,,,,,>).MakeGenericType(types);
                    case 11: return typeof(Func<,,,,,,,,,,>).MakeGenericType(types);
                    case 12: return typeof(Func<,,,,,,,,,,,>).MakeGenericType(types);
                    case 13: return typeof(Func<,,,,,,,,,,,,>).MakeGenericType(types);
                    case 14: return typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(types);
                    case 15: return typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(types);
                    case 16: return typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                    case 17: return typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                }
            }
            throw ContractUtils.Unreachable;
        }
    }
#endif

    internal static class Assert
    {
        [Conditional("DEBUG")]
        public static void NotNull(object var)
        {
            Debug.Assert(var != null);
        }
    }

    internal static class ExceptionHelpers
    {
        public static void UnwrapAndRethrow(TargetInvocationException exception)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
        }
    }

    internal static class ScriptingRuntimeHelpers
    {
        public static object Int32ToObject(int i)
        {
            switch (i)
            {
                case -1:
                    return Utils.BoxedIntM1;

                case 0:
                    return Utils.BoxedInt0;

                case 1:
                    return Utils.BoxedInt1;

                case 2:
                    return Utils.BoxedInt2;

                case 3:
                    return Utils.BoxedInt3;

                default:
                    return i;
            }
        }

        internal static object? GetPrimitiveDefaultValue(Type type)
        {
            object result;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    result = Utils.BoxedFalse;
                    break;

                case TypeCode.SByte:
                    result = Utils.BoxedDefaultSByte;
                    break;

                case TypeCode.Byte:
                    result = Utils.BoxedDefaultByte;
                    break;

                case TypeCode.Char:
                    result = Utils.BoxedDefaultChar;
                    break;

                case TypeCode.Int16:
                    result = Utils.BoxedDefaultInt16;
                    break;

                case TypeCode.Int32:
                    result = Utils.BoxedInt0;
                    break;

                case TypeCode.Int64:
                    result = Utils.BoxedDefaultInt64;
                    break;

                case TypeCode.UInt16:
                    result = Utils.BoxedDefaultUInt16;
                    break;

                case TypeCode.UInt32:
                    result = Utils.BoxedDefaultUInt32;
                    break;

                case TypeCode.UInt64:
                    result = Utils.BoxedDefaultUInt64;
                    break;

                case TypeCode.Single:
                    return Utils.BoxedDefaultSingle;

                case TypeCode.Double:
                    return Utils.BoxedDefaultDouble;

                case TypeCode.DateTime:
                    return Utils.BoxedDefaultDateTime;

                case TypeCode.Decimal:
                    return Utils.BoxedDefaultDecimal;

                default:
                    // Also covers DBNull which is a class.
                    return null;
            }

            if (type.IsEnum)
            {
                result = Enum.ToObject(type, result);
            }

            return result;
        }
    }

    /// <summary>
    ///     A hybrid dictionary which compares based upon object identity.
    /// </summary>
    internal class HybridReferenceDictionary<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        private const int _arraySize = 10;
        private Dictionary<TKey, TValue>? _dict;
        private KeyValuePair<TKey, TValue>[]? _keysAndValues;

        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out var res))
                {
                    return res;
                }

                throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
            }
            set
            {
                if (_dict != null)
                {
                    _dict[key] = value;
                }
                else
                {
                    int index;
                    if (_keysAndValues != null)
                    {
                        index = -1;
                        for (var i = 0; i < _keysAndValues.Length; i++)
                        {
                            if (_keysAndValues[i].Key == key)
                            {
                                _keysAndValues[i] = new KeyValuePair<TKey, TValue>(key, value);
                                return;
                            }

                            if (_keysAndValues[i].Key == null)
                            {
                                index = i;
                            }
                        }
                    }
                    else
                    {
                        _keysAndValues = new KeyValuePair<TKey, TValue>[_arraySize];
                        index = 0;
                    }

                    if (index != -1)
                    {
                        _keysAndValues[index] = new KeyValuePair<TKey, TValue>(key, value);
                    }
                    else
                    {
                        _dict = new Dictionary<TKey, TValue>();
                        foreach (var pair in _keysAndValues)
                        {
                            _dict[pair.Key] = pair.Value;
                        }

                        _keysAndValues = null;

                        _dict[key] = value;
                    }
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (_dict != null)
            {
                return _dict.ContainsKey(key);
            }

            var keysAndValues = _keysAndValues;
            return keysAndValues?.Any(pair => pair.Key == key) == true;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dict?.GetEnumerator() ?? GetEnumeratorWorker();
        }

        public void Remove(TKey key)
        {
            if (_dict != null)
            {
                _dict.Remove(key);
            }
            else if (_keysAndValues != null)
            {
                for (var i = 0; i < _keysAndValues.Length; i++)
                {
                    if (_keysAndValues[i].Key != key)
                    {
                        continue;
                    }

                    _keysAndValues[i] = new KeyValuePair<TKey, TValue>();
                    return;
                }
            }
        }

        public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
        {
            if (_dict != null)
            {
                return _dict.TryGetValue(key, out value);
            }

            if (_keysAndValues != null)
            {
                foreach (var pair in _keysAndValues)
                {
                    if (pair.Key != key)
                    {
                        continue;
                    }

                    value = pair.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private IEnumerator<KeyValuePair<TKey, TValue>> GetEnumeratorWorker()
        {
            if (_keysAndValues == null)
            {
                yield break;
            }

            foreach (var pair in _keysAndValues)
            {
                if (pair.Key != null)
                {
                    yield return pair;
                }
            }
        }
    }
}

#endif