﻿// -----------------------------------------------------------------------
//   <copyright file="Cluster.cs" company="Asynkron AB">
//       Copyright (C) 2015-2020 Asynkron AB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Proto.Cluster.IdentityLookup;
using Proto.Cluster.Partition;
using Proto.Remote;

namespace Proto.Cluster
{
    [PublicAPI]
    public class Cluster
    {
        private static ILogger _logger = null!;

        public Cluster(ActorSystem system, Serialization serialization)
        {
            System = system;
            Remote = new Remote.Remote(system, serialization);
            PidCache = new PidCache();
            PidCacheUpdater = new PidCacheUpdater(this, PidCache);
        }

        public Guid Id { get; } = Guid.NewGuid();

        internal ClusterConfig Config { get; private set; } = null!;

        public ActorSystem System { get; }

        public Remote.Remote Remote { get; }


        internal MemberList MemberList { get; private set; }
        internal PidCache PidCache { get; }
        internal PidCacheUpdater PidCacheUpdater { get; }

        private IIdentityLookup? IdentityLookup { get; set; }

        internal IClusterProvider Provider { get; set; }

        public string LoggerId => System.ProcessRegistry.Address;

        public Task StartAsync(string clusterName, string address, int port, IClusterProvider cp)
            => StartAsync(new ClusterConfig(clusterName, address, port, cp));

        public async Task StartAsync(ClusterConfig config)
        {
            Config = config;


            //default to partition identity lookup
            IdentityLookup = config.IdentityLookup ?? new PartitionIdentityLookup();
            Remote.Start(Config.Address, Config.Port, Config.RemoteConfig);
            Remote.Serialization.RegisterFileDescriptor(ProtosReflection.Descriptor);
            _logger = Log.CreateLogger($"Cluster-{LoggerId}");
            _logger.LogInformation("Starting");
            MemberList = new MemberList(this);

            var kinds = Remote.GetKnownKinds();
            IdentityLookup.Setup(this, kinds);


            if (config.UsePidCache)
            {
                PidCacheUpdater.Setup();
            }

            var (host, port) = System.ProcessRegistry.GetAddress();

            Provider = Config.ClusterProvider;

            await Provider.StartAsync(
                this,
                Config.Name,
                host,
                port,
                kinds,
                MemberList
            );

            _logger.LogInformation("Started");
        }

        public async Task ShutdownAsync(bool graceful = true)
        {
            _logger.LogInformation("Stopping");
            if (graceful)
            {
                PidCacheUpdater!.Shutdown();
                IdentityLookup!.Shutdown();
            }

            await Config!.ClusterProvider.ShutdownAsync(graceful);
            await Remote.ShutdownAsync(graceful);

            _logger.LogInformation("Stopped");
        }

        public Task<PID?> GetAsync(string identity, string kind) =>
            GetAsync(identity, kind, CancellationToken.None);

        public Task<PID?> GetAsync(string identity, string kind, CancellationToken ct)
        {
            if (Config.UsePidCache)
            {
                //Check Cache
                if (PidCache.TryGetCache(identity, out var pid))
                {
                    return Task.FromResult<PID?>(pid);
                }
            }

            return IdentityLookup!.GetAsync(identity, kind, ct);
        }

        //TODO: this retries with a timeout CT. we need a new CT per retry
        public async Task<T> RequestAsync<T>(string identity, string kind, object message, CancellationToken ct2)
        {
            for (var i = 0; i < 20; i++)
            {
                var ct = new CancellationTokenSource(5000).Token;
                var delay = i * 10;
                var pid = await GetAsync(identity, kind,ct);
                if (pid == null)
                {
                    _logger.LogDebug("Got null pid for {Identity}", identity);
                    await Task.Delay(delay, CancellationToken.None);
                    continue;
                }

                var res = await System.Root.RequestAsync<T>(pid, message, ct);
                if (res != null)
                {
                    return res;
                }

                _logger.LogDebug("Got null response from request to {Identity}", identity);

                await Task.Delay(delay, CancellationToken.None);
            }

            return default!;
        }
    }
}