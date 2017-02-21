﻿// -----------------------------------------------------------------------
//  <copyright file="InMemoryProviderState.cs" company="Asynkron HB">
//      Copyright (C) 2015-2017 Asynkron HB All rights reserved
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Proto.Persistence
{
    internal class InMemoryProviderState : IProviderState
    {
        private readonly IDictionary<string, List<object>> _events = new ConcurrentDictionary<string, List<object>>();

        private readonly IDictionary<string, Tuple<object, ulong>> _snapshots =
            new Dictionary<string, Tuple<object, ulong>>();

        public void Restart()
        {
        }

        public int GetSnapshotInterval()
        {
            return 0;
        }

        public Task<Tuple<object, ulong>> GetSnapshotAsync(string actorName)
        {
            Tuple<object, ulong> snapshot;
            _snapshots.TryGetValue(actorName, out snapshot);
            return Task.FromResult(snapshot);
        }

        public Task GetEventsAsync(string actorName, ulong eventIndexStart, Action<object> callback)
        {
            List<object> events;
            if (_events.TryGetValue(actorName, out events))
            {
                foreach (var e in events)
                {
                    callback(e);
                }
            }
            return Task.CompletedTask;
        }

        public Task PersistEventAsync(string actorName, ulong eventIndex, IMessage @event)
        {
            List<object> events;
            if (_events.TryGetValue(actorName, out events))
            {
                events.Add(@event);
            }
            return Task.CompletedTask;
        }

        public Task PersistSnapshotAsync(string actorName, ulong eventIndex, IMessage snapshot)
        {
            _snapshots[actorName] = Tuple.Create((object) snapshot, eventIndex);
            return Task.CompletedTask;
        }
    }
}