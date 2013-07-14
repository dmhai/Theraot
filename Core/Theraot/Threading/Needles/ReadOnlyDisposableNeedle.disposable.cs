﻿// <auto-generated />

using System;

using Theraot.Core;
using Theraot.Threading;

namespace Theraot.Threading.Needles
{
    public sealed partial class ReadOnlyDisposableNeedle<T> : IDisposable, IExtendedDisposable
    {
        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        private int _status;

        [global::System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        [global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralexceptionTypes", Justification = "Pokemon")]
        ~ReadOnlyDisposableNeedle()
        {
            try
            {
                //Empty
            }
            finally
            {
                try
                {
                    Dispose(false);
                }
                catch
                {
                    //Pokemon
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        public bool IsDisposed
        {
            [global::System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return _status == -1;
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        public void Dispose()
        {
            try
            {
                Dispose(true);
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        public void DisposedConditional(Action whenDisposed, Action whenNotDisposed)
        {
            if (_status == -1)
            {
                if (whenDisposed != null)
                {
                    whenDisposed.Invoke();
                }
            }
            else
            {
                if (whenNotDisposed != null)
                {
                    ThreadingHelper.SpinWaitExchangeIgnoringRelative(-1, ref _status, 1);
                    if (_status == -1)
                    {
                        if (whenDisposed != null)
                        {
                            whenDisposed.Invoke();
                        }
                    }
                    else
                    {
                        try
                        {
                            whenNotDisposed.Invoke();
                        }
                        finally
                        {
                            System.Threading.Interlocked.Decrement(ref _status);
                        }
                    }
                }
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        public TReturn DisposedConditional<TReturn>(Func<TReturn> whenDisposed, Func<TReturn> whenNotDisposed)
        {
            if (_status == -1)
            {
                if (whenDisposed == null)
                {
                    return default(TReturn);
                }
                else
                {
                    return whenDisposed.Invoke();
                }
            }
            else
            {
                if (whenNotDisposed == null)
                {
                    return default(TReturn);
                }
                else
                {
                    ThreadingHelper.SpinWaitExchangeIgnoringRelative(-1, ref _status, 1);
                    if (_status == -1)
                    {
                        if (whenDisposed == null)
                        {
                            return default(TReturn);
                        }
                        else
                        {
                            return whenDisposed.Invoke();
                        }
                    }
                    else
                    {
                        try
                        {
                            return whenNotDisposed.Invoke();
                        }
                        finally
                        {
                            System.Threading.Interlocked.Decrement(ref _status);
                        }
                    }
                }
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        private void Dispose(bool disposeManagedResources)
        {
            if (TakeDisposalExecution())
            {
                try
                {
                    if (disposeManagedResources)
                    {
                        //Empty
                    }
                }
                finally
                {
                    try
                    {
                        this.Kill();
                    }
                    finally
                    {
                        //Empty
                    }
                }
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        private void ProtectedCheckDisposed(string exceptionMessegeWhenDisposed)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(exceptionMessegeWhenDisposed);
            }
        }

        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        private bool TakeDisposalExecution()
        {
            if (_status == -1)
            {
                return false;
            }
            else
            {
                ThreadingHelper.SpinWaitExchangeIgnoring(-1, ref _status, -1, 0);
                if (_status == -1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        private void ThrowDisposedexception()
        {
            throw new ObjectDisposedException(GetType().FullName);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        private TReturn ThrowDisposedexception<TReturn>()
        {
            throw new ObjectDisposedException(GetType().FullName);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("DisposableTemplate", "1.0.0.0")]
        private bool UnDispose()
        {
            if (System.Threading.Thread.VolatileRead(ref _status) == -1)
            {
                System.Threading.Thread.VolatileWrite(ref _status, 0);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
