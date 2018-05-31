﻿#if NET20 || NET30 || NET35


namespace System.Threading
{
    public partial class ManualResetEventSlim
    {
        private enum Status
        {
            Disposed = -1,
            NotSet = 0,
            Set = 1,
            HandleRequested = 2,
            HandleReady = 3
        }
    }
}

#endif