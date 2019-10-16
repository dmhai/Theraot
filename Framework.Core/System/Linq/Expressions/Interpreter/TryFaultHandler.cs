﻿#if LESSTHAN_NET35

#pragma warning disable CC0021 // Use nameof

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic.Utils;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Theraot;
using Theraot.Core;
using Theraot.Reflection;
using AstUtils = System.Linq.Expressions.Utils;

namespace System.Linq.Expressions.Interpreter
{

    internal sealed class TryFaultHandler
    {
        internal readonly int FinallyEndIndex;
        internal readonly int FinallyStartIndex;
        internal readonly int TryEndIndex;
        internal readonly int TryStartIndex;

        internal TryFaultHandler(int tryStart, int tryEnd, int finallyStart, int finallyEnd)
        {
            TryStartIndex = tryStart;
            TryEndIndex = tryEnd;
            FinallyStartIndex = finallyStart;
            FinallyEndIndex = finallyEnd;
        }
    }
}

#endif