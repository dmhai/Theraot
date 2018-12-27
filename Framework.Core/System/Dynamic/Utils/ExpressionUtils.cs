﻿#if NET20 || NET30

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Theraot.Collections;
using Theraot.Reflection;

namespace System.Dynamic.Utils
{
    internal static class ExpressionUtils
    {
        public static void RequiresCanRead(Expression expression, string paramName)
        {
            RequiresCanRead(expression, paramName, -1);
        }

        public static void RequiresCanRead(Expression expression, string paramName, int idx)
        {
            ContractUtils.RequiresNotNull(expression, paramName, idx);

            // validate that we can read the node
            switch (expression.NodeType)
            {
                case ExpressionType.Index:
                    var index = (IndexExpression)expression;
                    if (index.Indexer != null && !index.Indexer.CanRead)
                    {
                        throw Error.ExpressionMustBeReadable(paramName, idx);
                    }
                    break;

                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)expression;
                    if (member.Member is PropertyInfo prop)
                    {
                        if (!prop.CanRead)
                        {
                            throw Error.ExpressionMustBeReadable(paramName, idx);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Helper which is used for specialized subtypes which use ReturnReadOnly(ref object, ...).
        /// This is the reverse version of ReturnReadOnly which takes an IArgumentProvider.
        ///
        /// This is used to return the 1st argument.  The 1st argument is typed as object and either
        /// contains a ReadOnlyCollection or the Expression.  We check for the Expression and if it's
        /// present we return that, otherwise we return the 1st element of the ReadOnlyCollection.
        /// </summary>
        public static T ReturnObject<T>(object collectionOrT) where T : class
        {
            if (collectionOrT is T t)
            {
                return t;
            }

            return ((ReadOnlyCollection<T>)collectionOrT)[0];
        }

        /// <summary>
        /// See overload with <see cref="IArgumentProvider"/> for more information.
        /// </summary>
        public static ReadOnlyCollection<ParameterExpression> ReturnReadOnly(IParameterProvider provider, ref object collection)
        {
            if (collection is ParameterExpression tObj)
            {
                // otherwise make sure only one read-only collection ever gets exposed
                Interlocked.CompareExchange(
                    ref collection,
                    new ReadOnlyCollection<ParameterExpression>(new ListParameterProvider(provider, tObj)),
                    tObj
                );
            }

            // and return what is not guaranteed to be a read-only collection
            return (ReadOnlyCollection<ParameterExpression>)collection;
        }

        public static ReadOnlyCollection<T> ReturnReadOnly<T>(ref IReadOnlyList<T> collection)
        {
            var value = collection;

            // if it's already read-only just return it.
            if (value is ReadOnlyCollection<T> res)
            {
                return res;
            }

            // otherwise make sure only read-only collection every gets exposed
            Interlocked.CompareExchange(ref collection, (TrueReadOnlyCollection<T>)value.ToTrueReadOnly(), value);

            // and return it
            return (ReadOnlyCollection<T>)collection;
        }

        /// <summary>
        /// Helper used for ensuring we only return 1 instance of a ReadOnlyCollection of T.
        ///
        /// This is similar to the ReturnReadOnly of T. This version supports nodes which hold
        /// onto multiple Expressions where one is typed to object.  That object field holds either
        /// an expression or a ReadOnlyCollection of Expressions.  When it holds a ReadOnlyCollection
        /// the IList which backs it is a ListArgumentProvider which uses the Expression which
        /// implements IArgumentProvider to get 2nd and additional values.  The ListArgumentProvider
        /// continues to hold onto the 1st expression.
        ///
        /// This enables users to get the ReadOnlyCollection w/o it consuming more memory than if
        /// it was just an array.  Meanwhile The DLR internally avoids accessing  which would force
        /// the read-only collection to be created resulting in a typical memory savings.
        /// </summary>
        public static ReadOnlyCollection<Expression> ReturnReadOnly(IArgumentProvider provider, ref object collection)
        {
            if (collection is Expression tObj)
            {
                // otherwise make sure only one read-only collection ever gets exposed
                Interlocked.CompareExchange(
                    ref collection,
                    new ReadOnlyCollection<Expression>(new ListArgumentProvider(provider, tObj)),
                    tObj
                );
            }

            // and return what is not guaranteed to be a read-only collection
            return (ReadOnlyCollection<Expression>)collection;
        }

        // Attempts to auto-quote the expression tree. Returns true if it succeeded, false otherwise.
        public static bool TryQuote(Type parameterType, ref Expression argument)
        {
            // We used to allow quoting of any expression, but the behavior of
            // quote (produce a new tree closed over parameter values), only
            // works consistently for lambdas
            var quoteable = typeof(LambdaExpression);

            if (parameterType.IsSameOrSubclassOfInternal(quoteable) && parameterType.IsInstanceOfType(argument))
            {
                argument = Expression.Quote(argument);
                return true;
            }

            return false;
        }

        public static void ValidateArgumentCount(MethodBase method, ExpressionType nodeKind, int count, ParameterInfo[] pis)
        {
            if (pis.Length != count)
            {
                // Throw the right error for the node we were given
                switch (nodeKind)
                {
                    case ExpressionType.New:
                        throw Error.IncorrectNumberOfConstructorArguments();
                    case ExpressionType.Invoke:
                        throw Error.IncorrectNumberOfLambdaArguments();
                    case ExpressionType.Dynamic:
                    case ExpressionType.Call:
                        throw Error.IncorrectNumberOfMethodCallArguments(method, nameof(method));
                    default:
                        throw ContractUtils.Unreachable;
                }
            }
        }

        public static void ValidateArgumentCount(this LambdaExpression lambda)
        {
            if (((IParameterProvider)lambda).ParameterCount >= ushort.MaxValue)
            {
                throw Error.InvalidProgram();
            }
        }

        public static void ValidateArgumentTypes(MethodBase method, ExpressionType nodeKind, ref Expression[] arguments, string methodParamName)
        {
            Debug.Assert(nodeKind == ExpressionType.Invoke || nodeKind == ExpressionType.Call || nodeKind == ExpressionType.Dynamic || nodeKind == ExpressionType.New);

            var pis = GetParametersForValidation(method, nodeKind);

            ValidateArgumentCount(method, nodeKind, arguments.Length, pis);

            Expression[] newArgs = null;
            for (int i = 0, n = pis.Length; i < n; i++)
            {
                var arg = arguments[i];
                var pi = pis[i];
                arg = ValidateOneArgument(method, nodeKind, arg, pi, methodParamName, nameof(arguments), i);

                if (newArgs == null && arg != arguments[i])
                {
                    newArgs = new Expression[arguments.Length];
                    for (var j = 0; j < i; j++)
                    {
                        newArgs[j] = arguments[j];
                    }
                }
                if (newArgs != null)
                {
                    newArgs[i] = arg;
                }
            }
            if (newArgs != null)
            {
                arguments = newArgs;
            }
        }

        public static Expression ValidateOneArgument(MethodBase method, ExpressionType nodeKind, Expression arguments, ParameterInfo pi, string methodParamName, string argumentParamName, int index = -1)
        {
            RequiresCanRead(arguments, argumentParamName, index);
            var pType = pi.ParameterType;
            if (pType.IsByRef)
            {
                pType = pType.GetElementType();
            }
            TypeUtils.ValidateType(pType, methodParamName, allowByRef: true, allowPointer: true);
            if (!pType.IsReferenceAssignableFromInternal(arguments.Type))
            {
                if (!TryQuote(pType, ref arguments))
                {
                    // Throw the right error for the node we were given
                    switch (nodeKind)
                    {
                        case ExpressionType.New:
                            throw Error.ExpressionTypeDoesNotMatchConstructorParameter(arguments.Type, pType, argumentParamName, index);
                        case ExpressionType.Invoke:
                            throw Error.ExpressionTypeDoesNotMatchParameter(arguments.Type, pType, argumentParamName, index);
                        case ExpressionType.Dynamic:
                        case ExpressionType.Call:
                            throw Error.ExpressionTypeDoesNotMatchMethodParameter(arguments.Type, pType, method, argumentParamName, index);
                        default:
                            throw ContractUtils.Unreachable;
                    }
                }
            }
            return arguments;
        }

        internal static ParameterInfo[] GetParametersForValidation(MethodBase method, ExpressionType nodeKind)
        {
            var pis = method.GetParameters();

            if (nodeKind == ExpressionType.Dynamic)
            {
                pis = pis.RemoveFirst(); // ignore CallSite argument
            }
            return pis;
        }

        internal static bool SameElements<T>(ICollection<T> replacement, T[] current) where T : class
        {
            Debug.Assert(current != null);
            if (replacement == current) // Relatively common case, so particularly useful to take the short-circuit.
            {
                return true;
            }

            if (replacement == null) // Treat null as empty.
            {
                return current.Length == 0;
            }

            return SameElementsInCollection(replacement, current);
        }

        internal static bool SameElements<T>(ref IEnumerable<T> replacement, T[] current) where T : class
        {
            Debug.Assert(current != null);
            if (replacement == current) // Relatively common case, so particularly useful to take the short-circuit.
            {
                return true;
            }

            if (replacement == null) // Treat null as empty.
            {
                return current.Length == 0;
            }

            // Ensure arguments is safe to enumerate twice.
            // If we have to build a collection, build a TrueReadOnlyCollection<T>
            // so it won't be built a second time if used.
            if (!(replacement is ICollection<T> replacementCol))
            {
                replacement = replacementCol = replacement.ToTrueReadOnly();
            }

            return SameElementsInCollection(replacementCol, current);
        }

        private static bool SameElementsInCollection<T>(ICollection<T> replacement, T[] current) where T : class
        {
            var count = current.Length;
            if (replacement.Count != count)
            {
                return false;
            }

            if (count != 0)
            {
                var index = 0;
                foreach (var replacementObject in replacement)
                {
                    if (replacementObject != current[index])
                    {
                        return false;
                    }

                    index++;
                }
            }

            return true;
        }
    }
}

#endif