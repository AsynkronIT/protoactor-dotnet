// -----------------------------------------------------------------------
// <copyright file="TaskClock.cs" company="Asynkron AB">
//      Copyright (C) 2015-2021 Asynkron AB All rights reserved
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Proto.Utils
{
    public class TaskClock
    {
        private readonly TimeSpan _bucketSize;
        private readonly CancellationToken _ct;
        private readonly TimeSpan _updateInterval;

        public TaskClock(TimeSpan timeout, TimeSpan updateInterval, CancellationToken ct)
        {
            _bucketSize = timeout + updateInterval;
            _updateInterval = updateInterval;
            _ct = ct;
        }

        public Task CurrentBucket { get; private set; }

        public void Start()
        {
            CurrentBucket = Task.Delay(_bucketSize, _ct);
            _ = SafeTask.Run(async () =>
                {
                    while (!_ct.IsCancellationRequested)
                    {
                        CurrentBucket = Task.Delay(_bucketSize, _ct);
                        await Task.Delay(_updateInterval, _ct);
                    }
                }
            );
        }
    }
}
