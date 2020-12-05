// -----------------------------------------------------------------------
// <copyright file="IdentityStorageWorker.cs" company="Asynkron AB">
//      Copyright (C) 2015-2020 Asynkron AB All rights reserved
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Proto.Cluster.Utils;

namespace Proto.Cluster.Identity
{
    internal class IdentityStorageWorker : IActor
    {
        private static readonly ConcurrentSet<string> StaleMembers = new();

        private readonly Cluster _cluster;

        private readonly Dictionary<ClusterIdentity, List<PID>> _inProgress = new();
        private readonly ILogger _logger = Log.CreateLogger<IdentityStorageWorker>();
        private readonly IdentityStorageLookup _lookup;
        private readonly MemberList _memberList;
        private readonly IIdentityStorage _storage;

        public IdentityStorageWorker(IdentityStorageLookup storageLookup)
        {
            _cluster = storageLookup.Cluster;
            _memberList = storageLookup.MemberList;
            _lookup = storageLookup;
            _storage = storageLookup.Storage;
        }

        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is not GetPid msg) return Task.CompletedTask;

            if (context.Sender == null)
            {
                _logger.LogCritical("No sender in GetPid request");
                return Task.CompletedTask;
            }

            if (_cluster.PidCache.TryGet(msg.ClusterIdentity, out var existing))
            {
                context.Respond(new PidResult
                    {
                        Pid = existing
                    }
                );
                return Task.CompletedTask;
            }

            if (_inProgress.TryGetValue(msg.ClusterIdentity, out var pidList))
            {
                pidList.Add(context.Sender);
                return Task.CompletedTask;
            }

            _inProgress.Add(msg.ClusterIdentity, new List<PID>());

            context.ReenterAfter(GetWithGlobalLock(context.Sender!, msg.ClusterIdentity, context.CancellationToken), task => {
                    try
                    {
                        var pid = task.Result;
                        if (pid != null) _cluster.PidCache.TryAdd(msg.ClusterIdentity, pid);

                        var pidResult = new PidResult
                        {
                            Pid = pid
                        };
                        //Respond to initial caller
                        context.Respond(pidResult);

                        //Any waiting subsequent callers get responded to in order
                        if (_inProgress.Remove(msg.ClusterIdentity, out pidList))
                        {
                            foreach (var waiter in pidList)
                            {
                                context.Send(waiter, pidResult);
                            }
                        }
                    }
                    catch (Exception x)
                    {
                        _logger.LogError(x, "Identity worker crashed {Id}", context.Self!.ToShortString());
                        throw;
                    }

                    return Task.CompletedTask;
                }
            );
            return Task.CompletedTask;
        }

        private async Task<PID?> GetWithGlobalLock(PID sender, ClusterIdentity clusterIdentity, CancellationToken ct)
        {
            try
            {
                var activation = await _storage.TryGetExistingActivation(clusterIdentity, ct);

                //we got an existing activation, use this
                if (activation != null)
                {
                    var existingPid = await ValidateAndMapToPid(clusterIdentity, activation);
                    if (existingPid != null) return existingPid;
                }

                //are there any members that can spawn this kind?
                //if not, just bail out
                var activator = _memberList.GetActivator(clusterIdentity.Kind, sender.Address);
                if (activator == null) return null;

                //try to acquire global lock
                var spawnLock = await _storage.TryAcquireLock(clusterIdentity, ct);

                //we didn't get the lock, wait for activation to complete
                if (spawnLock == null) return await WaitForActivation(clusterIdentity, ct);

                //we have the lock, spawn and return
                var pid = await SpawnActivationAsync(activator, spawnLock, ct);
                return pid;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get PID for {ClusterIdentity}", clusterIdentity.ToShortString());
                return null;
            }
        }

        private async Task<PID?> WaitForActivation(ClusterIdentity clusterIdentity, CancellationToken ct) =>
            await ValidateAndMapToPid(clusterIdentity, await _storage.WaitForActivation(clusterIdentity, ct));

        private async Task<PID?> SpawnActivationAsync(Member activator, SpawnLock spawnLock, CancellationToken ct)
        {
            //we own the lock
            _logger.LogDebug("Storing placement lookup for {Identity} {Kind}", spawnLock.ClusterIdentity.Identity,
                spawnLock.ClusterIdentity.Kind
            );

            var remotePid = _lookup.RemotePlacementActor(activator.Address);
            var req = new ActivationRequest
            {
                ClusterIdentity = spawnLock.ClusterIdentity,
                RequestId = spawnLock.LockId
            };

            try
            {
                var resp = ct == CancellationToken.None
                    ? await _cluster.System.Root.RequestAsync<ActivationResponse>(remotePid, req,
                        _cluster.Config!.TimeoutTimespan
                    )
                    : await _cluster.System.Root.RequestAsync<ActivationResponse>(remotePid, req, ct);

                if (resp.Pid != null)
                {
                    _cluster.PidCache.TryAdd(spawnLock.ClusterIdentity, resp.Pid!);
                    return resp.Pid;
                }
            }
            //TODO: decide if we throw or return null
            catch (TimeoutException)
            {
                _logger.LogDebug("Remote PID request timeout {@Request}", req);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured requesting remote PID {@Request}", req);
            }

            //Clean up our mess..
            await _storage.RemoveLock(spawnLock, ct);
            return null;
        }

        private async Task<PID?> ValidateAndMapToPid(ClusterIdentity clusterIdentity, StoredActivation? activation)
        {
            if (activation?.Pid == null) return null;

            var memberExists = _memberList.ContainsMemberId(activation.MemberId);

            if (memberExists) return activation.Pid;

            if (StaleMembers.TryAdd(activation.MemberId!))
            {
                _logger.LogWarning(
                    "Found placement lookup for {ClusterIdentity}, but Member {MemberId} is not part of cluster, dropping stale entries",
                    clusterIdentity.ToShortString(), activation.MemberId
                );
            }

            //let all requests try to remove, but only log on the first occurrence
            await _storage.RemoveMember(activation.MemberId!, CancellationToken.None);
            return null;
        }
    }
}