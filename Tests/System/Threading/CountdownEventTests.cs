﻿// CountdownEventTests.cs
//
// Authors:
//    Marek Safar  <marek.safar@gmail.com>
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using NUnit.Framework;
using System;
using System.Threading;
using Tests.Helpers;

namespace MonoTests.System.Threading
{
    [TestFixture]
    public class CountdownEventTests
    {
        [Test]
        public void Constructor_Invalid()
        {
            try
            {
                using (new CountdownEvent(-2))
                {
                    Assert.Fail("#1");
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Theraot.No.Op(ex);
            }
        }

        [Test]
        public void Constructor_Zero()
        {
            using (var ce = new CountdownEvent(0))
            {
                Assert.IsTrue(ce.IsSet, "#1");
                Assert.AreEqual(0, ce.InitialCount, "#2");
                Assert.IsTrue(ce.Wait(0), "#3");
            }
        }

        [Test]
        public void Constructor_Max()
        {
            using (new CountdownEvent(int.MaxValue))
            {
                // Empty
            }
        }

        [Test]
        public void AddCount_Invalid()
        {
            using (var ev = new CountdownEvent(1))
            {
                try
                {
                    ev.AddCount(0);
                    Assert.Fail("#1");
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Theraot.No.Op(ex);
                }

                try
                {
                    ev.AddCount(-1);
                    Assert.Fail("#2");
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Theraot.No.Op(ex);
                }
            }
        }

        [Test]
        public void AddCount_HasBeenSet()
        {
            using (var ev = new CountdownEvent(0))
            {
                try
                {
                    ev.AddCount(1);
                    Assert.Fail("#1");
                }
                catch (InvalidOperationException ex)
                {
                    Theraot.No.Op(ex);
                }
            }

            using (var ev = new CountdownEvent(1))
            {
                Assert.IsTrue(ev.Signal(), "#2");
                try
                {
                    ev.AddCount(1);
                    Assert.Fail("#3");
                }
                catch (InvalidOperationException ex)
                {
                    Theraot.No.Op(ex);
                }
            }
        }

        [Test]
        public void AddCountSignalStressTestCase()
        {
            var evt = new CountdownEvent(5);

            var count = 0;
            ParallelTestHelper.ParallelStressTest
            (
                () =>
                {
                    var num = Interlocked.Increment(ref count);
                    if (num % 2 == 0)
                    {
                        evt.AddCount();
                    }
                    else
                    {
                        evt.Signal();
                    }
                }, 7
            );

            Assert.AreEqual(4, evt.CurrentCount, "#1");
            Assert.IsFalse(evt.IsSet, "#2");
        }

        [Test]
        public void InitialTestCase()
        {
            using (var evt = new CountdownEvent(5))
            {
                Assert.AreEqual(5, evt.InitialCount, "#1");
                evt.AddCount();
                evt.Signal(3);
                Assert.AreEqual(5, evt.InitialCount, "#2");
            }
        }

        [Test]
        public void CurrentCountTestCase()
        {
            using (var evt = new CountdownEvent(5))
            {
                Assert.AreEqual(5, evt.CurrentCount, "#1");

                evt.AddCount();
                Assert.AreEqual(6, evt.CurrentCount, "#2");

                evt.TryAddCount(2);
                Assert.AreEqual(8, evt.CurrentCount, "#3");

                evt.Signal(4);
                Assert.AreEqual(4, evt.CurrentCount, "#4");

                evt.Reset();
                Assert.AreEqual(5, evt.CurrentCount, "#5");
            }
        }

        [Test]
        public void Dispose()
        {
            var ce = new CountdownEvent(1);
            ce.Dispose();
            Assert.AreEqual(1, ce.CurrentCount, "#0a");
            Assert.AreEqual(1, ce.InitialCount, "#0b");
            Assert.IsFalse(ce.IsSet, "#0c");

            try
            {
                ce.AddCount();
                Assert.Fail("#1");
            }
            catch (ObjectDisposedException ex)
            {
                Theraot.No.Op(ex);
            }

            try
            {
                ce.Reset();
                Assert.Fail("#2");
            }
            catch (ObjectDisposedException ex)
            {
                Theraot.No.Op(ex);
            }

            try
            {
                ce.Signal();
                Assert.Fail("#3");
            }
            catch (ObjectDisposedException ex)
            {
                Theraot.No.Op(ex);
            }

            try
            {
                ce.TryAddCount();
                Assert.Fail("#4");
            }
            catch (ObjectDisposedException ex)
            {
                Theraot.No.Op(ex);
            }

            try
            {
                ce.Wait(5);
                Assert.Fail("#4");
            }
            catch (ObjectDisposedException ex)
            {
                Theraot.No.Op(ex);
            }

            try
            {
                GC.KeepAlive(ce.WaitHandle);
                Assert.Fail("#5");
            }
            catch (ObjectDisposedException ex)
            {
                Theraot.No.Op(ex);
            }
        }

        [Test]
        public void Dispose_Double()
        {
            var ce = new CountdownEvent(1);
            ce.Dispose();
            ce.Dispose();
        }

        [Test]
        public void IsSetTestCase()
        {
            using (var evt = new CountdownEvent(5))
            {
                Assert.IsFalse(evt.IsSet, "#1");

                evt.Signal(5);
                Assert.IsTrue(evt.IsSet, "#2");

                evt.Reset();
                Assert.IsFalse(evt.IsSet, "#3");
            }
        }

        [Test]
        public void Reset_Invalid()
        {
            using (var ev = new CountdownEvent(1))
            {
                try
                {
                    ev.Reset(-1);
                    Assert.Fail("#1");
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(1, ev.CurrentCount, "#1a");
                }
            }
        }

        [Test]
        public void Reset_FullInitialized()
        {
            using (var ev = new CountdownEvent(0))
            {
                Assert.IsTrue(ev.IsSet, "#1");
                Assert.AreEqual(0, ev.CurrentCount, "#2");

                ev.Reset(4);
                Assert.IsFalse(ev.IsSet, "#3");
                Assert.AreEqual(4, ev.CurrentCount, "#4");
                Assert.IsFalse(ev.Wait(0), "#5");
            }
        }

        [Test]
        public void Reset_Zero()
        {
            using (var ev = new CountdownEvent(1))
            {
                Assert.IsFalse(ev.IsSet, "#1");

                ev.Reset(0);
                Assert.IsTrue(ev.IsSet, "#2");
                Assert.IsTrue(ev.Wait(0), "#3");
                Assert.AreEqual(0, ev.CurrentCount, "#4");
            }
        }

        [Test]
        public void Signal_Invalid()
        {
            using (var ev = new CountdownEvent(1))
            {
                try
                {
                    ev.Signal(0);
                    Assert.Fail("#1");
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(1, ev.CurrentCount, "#1a");
                }

                try
                {
                    ev.Signal(-1);
                    Assert.Fail("#2");
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(1, ev.CurrentCount, "#2a");
                }
            }
        }

        [Test]
        public void Signal_Negative()
        {
            using (var ev = new CountdownEvent(1))
            {
                try
                {
                    ev.Signal(2);
                    Assert.Fail("#1");
                }
                catch (InvalidOperationException)
                {
                    Assert.AreEqual(1, ev.CurrentCount, "#1a");
                }

                ev.Signal();
                try
                {
                    ev.Signal();
                    Assert.Fail("#2");
                }
                catch (InvalidOperationException)
                {
                    Assert.AreEqual(0, ev.CurrentCount, "#2a");
                }
            }
        }

        [Test]
        [Category("RaceCondition")] // This test creates a race condition
        public void Signal_Concurrent() // TODO: Review
        {
            for (var r = 0; r < 100; ++r)
            {
                using (var ce = new CountdownEvent(500))
                {
                    for (var i = 0; i < ce.InitialCount; ++i)
                    {
                        ThreadPool.QueueUserWorkItem(delegate { ce.Signal(); });
                    }

                    Assert.IsTrue(ce.Wait(1000), "#1");
                }
            }
        }

        [Test]
        public void TryAddCountTestCase()
        {
            using (var evt = new CountdownEvent(5))
            {
                Assert.IsTrue(evt.TryAddCount(2), "#1");
                evt.Signal(7);
                Assert.IsFalse(evt.TryAddCount(), "#2");
            }
        }

        [Test]
        public void TryAddCount_Invalid()
        {
            using (var ev = new CountdownEvent(1))
            {
                try
                {
                    ev.TryAddCount(0);
                    Assert.Fail("#1");
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Theraot.No.Op(ex);
                }

                try
                {
                    ev.TryAddCount(-1);
                    Assert.Fail("#2");
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Theraot.No.Op(ex);
                }
            }
        }

        [Test]
        public void TryAddCount_HasBeenSet()
        {
            using (var ev = new CountdownEvent(0))
            {
                Assert.IsFalse(ev.TryAddCount(1), "#1");
            }

            using (var ev = new CountdownEvent(1))
            {
                ev.Signal();
                Assert.IsFalse(ev.TryAddCount(1), "#2");
            }

            using (var ev = new CountdownEvent(2))
            {
                ev.Signal(2);
                Assert.IsFalse(ev.TryAddCount(66), "#3");
            }
        }

        [Test]
        public void WaitTestCase()
        {
            var evt = new CountdownEvent(5);

            var count = 0;
            var s = false;

            ParallelTestHelper.ParallelStressTest
            (
                () =>
                {
                    if (Interlocked.Increment(ref count) % 2 == 0)
                    {
                        Thread.Sleep(100);
                        while (!evt.IsSet)
                        {
                            evt.Signal();
                        }
                    }
                    else
                    {
                        evt.Wait();
                        s = true;
                    }
                }, 3
            );

            Assert.IsTrue(s, "#1");
            Assert.IsTrue(evt.IsSet, "#2");
        }

        [Test]
        public void ResetTest()
        {
            using (var evt = new CountdownEvent(5))
            {
                Assert.AreEqual(5, evt.CurrentCount);
                evt.Signal();
                Assert.AreEqual(4, evt.CurrentCount);
                evt.Reset();
                Assert.AreEqual(5, evt.CurrentCount);
                Assert.AreEqual(5, evt.InitialCount);
                evt.Signal();
                evt.Signal();
                Assert.AreEqual(3, evt.CurrentCount);
                Assert.AreEqual(5, evt.InitialCount);
                evt.Reset(10);
                Assert.AreEqual(10, evt.CurrentCount);
                Assert.AreEqual(10, evt.InitialCount);
            }
        }
    }
}