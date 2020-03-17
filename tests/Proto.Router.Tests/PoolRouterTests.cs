﻿using System;
using Proto.Router.Messages;
using Proto.TestFixtures;
using Xunit;

namespace Proto.Router.Tests
{
    public class PoolRouterTests
    {
        private readonly ActorSystem ActorSystem = new ActorSystem();
        private static readonly Props MyActorProps = Props.FromProducer(() => new DoNothingActor());
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(1000);

        [Fact]
        public async void BroadcastGroupPool_CreatesRoutees()
        {
            var props = new ActorSystem().Root.GetRouters().NewBroadcastPool(MyActorProps, 3)
                .WithMailbox(() => new TestMailbox());
            var router = ActorSystem.Root.Spawn(props);
            var routees = await ActorSystem.Root.RequestAsync<Routees>(router, new RouterGetRoutees(), _timeout);
            Assert.Equal(3, routees.PIDs.Count);
        }

        [Fact]
        public async void RoundRobinPool_CreatesRoutees()
        {
            var props = new ActorSystem().Root.GetRouters().NewRoundRobinPool(MyActorProps, 3)
                .WithMailbox(() => new TestMailbox());
            var router = ActorSystem.Root.Spawn(props);
            var routees = await ActorSystem.Root.RequestAsync<Routees>(router, new RouterGetRoutees(), _timeout);
            Assert.Equal(3, routees.PIDs.Count);
        }

        [Fact]
        public async void ConsistentHashPool_CreatesRoutees()
        {
            var props = new ActorSystem().Root.GetRouters().NewConsistentHashPool(MyActorProps, 3)
                .WithMailbox(() => new TestMailbox());
            var router = ActorSystem.Root.Spawn(props);
            var routees = await ActorSystem.Root.RequestAsync<Routees>(router, new RouterGetRoutees(), _timeout);
            Assert.Equal(3, routees.PIDs.Count);
        }

        [Fact]
        public async void RandomPool_CreatesRoutees()
        {
            var props = new ActorSystem().Root.GetRouters().NewRandomPool(MyActorProps, 3)
                .WithMailbox(() => new TestMailbox());
            var router = ActorSystem.Root.Spawn(props);
            var routees = await ActorSystem.Root.RequestAsync<Routees>(router, new RouterGetRoutees(), _timeout);
            Assert.Equal(3, routees.PIDs.Count);
        }
    }
}
