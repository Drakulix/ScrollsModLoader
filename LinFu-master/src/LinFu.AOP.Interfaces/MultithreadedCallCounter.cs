﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LinFu.AOP.Interfaces
{
    internal class MultiThreadedCallCounter : ICallCounter
    {
        private readonly Dictionary<int, SingleThreadedCallCounter> _counts = new Dictionary<int, SingleThreadedCallCounter>();
        #region ICallCounter Members

        public void Increment(IInvocationInfo context)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            lock (_counts)
            {
                if (!_counts.ContainsKey(threadId))
                    _counts[threadId] = new SingleThreadedCallCounter();

                _counts[threadId].Increment(context);
            }
        }

        public void Decrement(IInvocationInfo context)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            if (!_counts.ContainsKey(threadId))
                return;

            lock (_counts)
            {
                var counter = _counts[threadId];
                counter.Decrement(context);
            }
        }

        public int GetPendingCalls(IInvocationInfo context)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            if (!_counts.ContainsKey(threadId))
                return 0;

            return _counts[threadId].GetPendingCalls(context);
        }

        #endregion
    }
}
