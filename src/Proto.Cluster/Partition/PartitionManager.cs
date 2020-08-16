﻿// -----------------------------------------------------------------------
//   <copyright file="Partition.cs" company="Asynkron AB">
//       Copyright (C) 2015-2020 Asynkron AB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

namespace Proto.Cluster.Partition
{
    //helper to interact with partition actors on this and other members
    internal class PartitionManager
    {
        private PID _partitionActor = null!;
        private PID _partitionActivator = null!;
        private readonly Cluster _cluster;
        private readonly ActorSystem _system;
        private readonly IRootContext _context;
        internal PartitionMemberSelector Selector { get; } = new PartitionMemberSelector();
        internal const string PartitionIdentityActorName = "partition-actor";
        internal const string PartitionPlacementActorName = "partition-activator";


        internal PartitionManager(Cluster cluster)
        {
            _cluster = cluster;
            _system = cluster.System;
            _context = _system.Root;
        }

        public void Setup()
        {
            var partitionActorProps = Props
                .FromProducer(() => new PartitionIdentityActor(_cluster, this))
                .WithGuardianSupervisorStrategy(Supervision.AlwaysRestartStrategy);
            _partitionActor = _context.SpawnNamed(partitionActorProps, PartitionIdentityActorName);
            
            var partitionActivatorProps =
                Props.FromProducer(() => new PartitionPlacementActor(_cluster, this));
            _partitionActivator = _context.SpawnNamed(partitionActivatorProps, PartitionPlacementActorName);

            //synchronous subscribe to keep accurate

            //make sure selector is updated first
            _system.EventStream.Subscribe<MemberJoinedEvent>(e =>
                {
                    Selector.AddMember(e.Member);
                    _context.Send(_partitionActor, e);
                    _context.Send(_partitionActivator,e);
                }
            );
            _system.EventStream.Subscribe<MemberLeftEvent>(e =>
                {
                    Selector.RemoveMember(e.Member);
                    _context.Send(_partitionActor, e);
                    _context.Send(_partitionActivator,e);
                }
            );

        }


        public void Shutdown()
        {
            _context.Stop(_partitionActor);
            _context.Stop(_partitionActivator);
        }

        public PID RemotePartitionIdentityActor(string address) => new PID(address, PartitionIdentityActorName);

        public PID RemotePartitionPlacementActor(string address) => new PID(address, PartitionPlacementActorName);
    }
}