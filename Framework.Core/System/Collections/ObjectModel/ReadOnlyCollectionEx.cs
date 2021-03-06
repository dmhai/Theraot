﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Theraot.Collections;

namespace System.Collections.ObjectModel
{
    public static class ReadOnlyCollectionEx
    {
        public static ReadOnlyCollectionEx<T> Create<T>(params T[] list)
        {
            return new ReadOnlyCollectionEx<T>(list);
        }
    }

#if LESSTHAN_NET45

    public partial class ReadOnlyCollectionEx<T> : IReadOnlyList<T>
    {
        // Empty
    }

#endif

    [Serializable]
    [ComVisible(false)]
    [DebuggerNonUserCode]
    [DebuggerDisplay("Count={" + nameof(Count) + "}")]
    public
#if LESSTHAN_NET45
    partial
#endif
    class ReadOnlyCollectionEx<T> : ReadOnlyCollection<T>
    {
        public ReadOnlyCollectionEx(IList<T> wrapped)
            : base(wrapped)
        {
            Wrapped = wrapped;
        }

        internal IList<T> Wrapped { get; }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            Extensions.CanCopyTo(Count - index, array, count);
            Extensions.CopyTo(this, index, array, arrayIndex, count);
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public void CopyTo(T[] array)
        {
            Wrapped.CopyTo(array, 0);
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            Extensions.CanCopyTo(array, arrayIndex, count);
            Extensions.CopyTo(this, array, arrayIndex, count);
        }

        public override int GetHashCode()
        {
            // Copyright (c) Microsoft. All rights reserved.
            // Licensed under the MIT license. See LICENSE file in the project root for full license information.
            var cmp = EqualityComparer<T>.Default;
            var h = 6551;
            foreach (var t in this)
            {
                h ^= (h << 5) ^ cmp.GetHashCode(t);
            }

            return h;
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public T[] ToArray()
        {
            var array = new T[Wrapped.Count];
            CopyTo(array);
            return array;
        }
    }
}