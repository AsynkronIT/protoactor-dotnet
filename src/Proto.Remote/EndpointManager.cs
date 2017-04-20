﻿// -----------------------------------------------------------------------
//  <copyright file="EndpointManager.cs" company="Asynkron HB">
//      Copyright (C) 2015-2017 Asynkron HB All rights reserved
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Proto.Remote
{
    public class Endpoint
    {
        public Endpoint(PID writer, PID watcher)
        {
            Writer = writer;
            Watcher = watcher;
        }

        public PID Writer { get; }
        public PID Watcher { get; }
    }

    public class EndpointManager : IActor
    {
        private readonly RemoteConfig _config;
        private readonly ConcurrentDictionary<string, Endpoint> _connections = new ConcurrentDictionary<string, Endpoint>();

        private readonly ILogger _logger = Log.CreateLogger<EndpointManager>();

        public EndpointManager(RemoteConfig config)
        {
            _config = config;
        }

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                {
                    _logger.LogDebug("Started EndpointManager");
                    return Actor.Done;
                }
                case EndpointTerminatedEvent msg:
                {
                    var endpoint = EnsureConnected(msg.Address, context);
                    endpoint.Watcher.Tell(msg);
                    return Actor.Done;
                }
                case RemoteTerminate msg:
                {
                    var endpoint = EnsureConnected(msg.Watchee.Address, context);
                    endpoint.Watcher.Tell(msg);
                    return Actor.Done;
                }
                case RemoteWatch msg:
                {
                    var endpoint = EnsureConnected(msg.Watchee.Address, context);
                    endpoint.Watcher.Tell(msg);
                    return Actor.Done;
                }
                case RemoteUnwatch msg:
                {
                    var endpoint = EnsureConnected(msg.Watchee.Address, context);
                    endpoint.Watcher.Tell(msg);
                    return Actor.Done;
                }
                case RemoteDeliver msg:
                {
                    var endpoint = EnsureConnected(msg.Target.Address, context);
                    endpoint.Writer.Tell(msg);
                    return Actor.Done;
                }
                default:
                    return Actor.Done;
            }
        }


        private Endpoint EnsureConnected(string address, IContext context)
        {
            var ok = _connections.TryGetValue(address, out var endpoint);
            if (!ok)
            {
                var writer = SpawnWriter(address, context);

                var watcher = SpawnWatcher(address, context);

                endpoint = new Endpoint(writer, watcher);
                
                if (!_connections.TryAdd(address, endpoint)){
                    _connections.TryGetValue(address, out endpoint);
                }
            }

            return endpoint;
        }

        private static PID SpawnWatcher(string address, IContext context)
        {
            var watcherProps = Actor.FromProducer(() => new EndpointWatcher(address));
            var watcher = context.Spawn(watcherProps);
            return watcher;
        }

        private PID SpawnWriter(string address, IContext context)
        {
            var writerProps =
                Actor.FromProducer(() => new EndpointWriter(address, _config.ChannelOptions, _config.CallOptions, _config.ChannelCredentials))
                    .WithMailbox(() => new EndpointWriterMailbox(_config.EndpointWriterBatchSize));
            var writer = context.Spawn(writerProps);
            return writer;
        }
    }
}