﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClusterTest.Messages;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Proto.Cluster.Tests
{
    public abstract class ClusterTests : ClusterTestBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

        protected ClusterTests(ITestOutputHelper testOutputHelper, IClusterFixture clusterFixture)
            : base(clusterFixture) => _testOutputHelper = testOutputHelper;

        [Fact]
        public async Task HandlesSlowResponsesCorrectly()
        {
            CancellationToken timeout = new CancellationTokenSource(8000).Token;

            string msg = "Hello-slow-world";
            Pong response = await Members.First().RequestAsync<Pong>(CreateIdentity("slow-test"), EchoActor.Kind,
                new SlowPing {Message = msg, DelayMs = 5000}, timeout
            );
            response.Should().NotBeNull();
            response.Message.Should().Be(msg);
        }

        [Fact]
        public async Task ReSpawnsClusterActorsFromDifferentNodes()
        {
            CancellationToken timeout = new CancellationTokenSource(5000).Token;
            string id = CreateIdentity("1");
            await PingPong(Members[0], id, timeout);
            await PingPong(Members[1], id, timeout);

            //Retrieve the node the virtual actor was not spawned on
            HereIAm nodeLocation =
                await Members[0].RequestAsync<HereIAm>(id, EchoActor.Kind, new WhereAreYou(), timeout);
            nodeLocation.Should().NotBeNull("We expect the actor to respond correctly");
            Cluster otherNode = Members.First(node => node.System.Address != nodeLocation.Address);

            //Kill it
            await otherNode.RequestAsync<Ack>(id, EchoActor.Kind, new Die(), timeout);

            Stopwatch timer = Stopwatch.StartNew();
            // And force it to restart.
            // DeadLetterResponse should be sent to requestAsync, enabling a quick initialization of the new virtual actor
            await PingPong(otherNode, id, timeout);
            timer.Stop();

            _testOutputHelper.WriteLine("Respawned virtual actor in {0}", timer.Elapsed);
        }

        [Fact]
        public async Task HandlesLosingANode()
        {
            List<string> ids = Enumerable.Range(1, 200).Select(id => id.ToString()).ToList();

            await CanGetResponseFromAllIdsOnAllNodes(ids, Members, 5000);

            Cluster toBeRemoved = Members.Last();
            _testOutputHelper.WriteLine("Removing node " + toBeRemoved.System.Id + " / " + toBeRemoved.System.Address);
            await ClusterFixture.RemoveNode(toBeRemoved);
            _testOutputHelper.WriteLine("Removed node " + toBeRemoved.System.Id + " / " + toBeRemoved.System.Address);
            await ClusterFixture.SpawnNode();

            await CanGetResponseFromAllIdsOnAllNodes(ids, Members, 5000);

            _testOutputHelper.WriteLine("All responses OK. Terminating fixture");
        }

        // [Fact]
        // public async Task HandlesLosingANodeWhileProcessing()
        // {
        //     var ingressNodes = new[] {Members[0], Members[1]};
        //     var victim = Members[2];
        //     var ids = Enumerable.Range(1, 200).Select(id => id.ToString()).ToList();
        //
        //     var cts = new CancellationTokenSource();
        //
        //     var worker = Task.Run(async () => {
        //             while (!cts.IsCancellationRequested)
        //             {
        //                 await CanGetResponseFromAllIdsOnAllNodes(ids, ingressNodes, 10000);
        //             }
        //         }
        //     );
        //     await Task.Delay(200);
        //     await ClusterFixture.RemoveNode(victim);
        //     await ClusterFixture.SpawnNode();
        //     await Task.Delay(1000);
        //     cts.Cancel();
        //     await worker;
        //
        //     //Repair cluster..
        // }

        private async Task CanGetResponseFromAllIdsOnAllNodes(IEnumerable<string> actorIds, IList<Cluster> nodes,
            int timeoutMs)
        {
            Stopwatch timer = Stopwatch.StartNew();
            CancellationToken timeout = new CancellationTokenSource(timeoutMs).Token;
            await Task.WhenAll(nodes.SelectMany(entryNode => actorIds.Select(id => PingPong(entryNode, id, timeout))));
            _testOutputHelper.WriteLine("Got response from {0} nodes in {1}", nodes.Count(), timer.Elapsed);
        }

        [Theory]
        [InlineData(100, 10000)]
        public async Task CanSpawnVirtualActorsSequentially(int actorCount, int timeoutMs)
        {
            CancellationToken timeout = new CancellationTokenSource(timeoutMs).Token;

            Cluster entryNode = Members.First();

            Stopwatch timer = Stopwatch.StartNew();

            foreach (string id in GetActorIds(actorCount))
            {
                await PingPong(entryNode, id, timeout);
            }

            timer.Stop();
            _testOutputHelper.WriteLine($"Spawned {actorCount} actors across {Members.Count} nodes in {timer.Elapsed}");
        }

        [Theory]
        [InlineData(100, 10000)]
        public async Task ConcurrentActivationsOnSameIdWorks(int clientCount, int timeoutMs)
        {
            CancellationToken timeout = new CancellationTokenSource(timeoutMs).Token;

            Cluster entryNode = Members.First();
            Stopwatch timer = Stopwatch.StartNew();

            string id = GetActorIds(clientCount).First();

            await Task.WhenAll(Enumerable.Range(0, clientCount).Select(_ => PingPong(entryNode, id, timeout)));

            timer.Stop();
            _testOutputHelper.WriteLine($"Spawned 1 actor from {clientCount} clients in {timer.Elapsed}");
        }

        [Theory]
        [InlineData(100, 10000)]
        public async Task CanSpawnVirtualActorsConcurrently(int actorCount, int timeoutMs)
        {
            CancellationToken timeout = new CancellationTokenSource(timeoutMs).Token;

            Cluster entryNode = Members.First();

            Stopwatch timer = Stopwatch.StartNew();
            await Task.WhenAll(GetActorIds(actorCount).Select(id => PingPong(entryNode, id, timeout)));
            timer.Stop();
            _testOutputHelper.WriteLine($"Spawned {actorCount} actors across {Members.Count} nodes in {timer.Elapsed}");
        }

        [Theory]
        [InlineData(100, 10000)]
        public async Task CanSpawnMultipleKindsWithSameIdentityConcurrently(int actorCount, int timeoutMs)
        {
            using CancellationTokenSource cts = new CancellationTokenSource(timeoutMs);
            CancellationToken timeout = cts.Token;

            Cluster entryNode = Members.First();

            Stopwatch timer = Stopwatch.StartNew();
            IEnumerable<string> actorIds = GetActorIds(actorCount);
            await Task.WhenAll(actorIds.Select(id => Task.WhenAll(
                        PingPong(entryNode, id, timeout),
                        PingPong(entryNode, id, timeout, EchoActor.Kind2)
                    )
                )
            );
            timer.Stop();
            _testOutputHelper.WriteLine(
                $"Spawned {actorCount * 2} actors across {Members.Count} nodes in {timer.Elapsed}"
            );
        }

        [Theory]
        [InlineData(100, 10000)]
        public async Task CanSpawnVirtualActorsConcurrentlyOnAllNodes(int actorCount, int timeoutMs)
        {
            CancellationToken timeout = new CancellationTokenSource(timeoutMs).Token;

            Stopwatch timer = Stopwatch.StartNew();
            await Task.WhenAll(Members.SelectMany(member =>
                    GetActorIds(actorCount).Select(id => PingPong(member, id, timeout))
                )
            );
            timer.Stop();
            _testOutputHelper.WriteLine($"Spawned {actorCount} actors across {Members.Count} nodes in {timer.Elapsed}");
        }

        [Theory]
        [InlineData(100, 20000)]
        public async Task CanRespawnVirtualActors(int actorCount, int timeoutMs)
        {
            using CancellationTokenSource cts = new CancellationTokenSource(timeoutMs);
            CancellationToken timeout = cts.Token;

            Cluster entryNode = Members.First();

            Stopwatch timer = Stopwatch.StartNew();

            List<string> ids = GetActorIds(actorCount).ToList();

            await Task.WhenAll(ids.Select(id => PingPong(entryNode, id, timeout)));
            await Task.WhenAll(ids.Select(id =>
                    entryNode.RequestAsync<Ack>(id, EchoActor.Kind, new Die(), timeout)
                )
            );
            await Task.WhenAll(ids.Select(id => PingPong(entryNode, id, timeout)));
            timer.Stop();
            _testOutputHelper.WriteLine(
                $"Spawned, killed and spawned {actorCount} actors across {Members.Count} nodes in {timer.Elapsed}"
            );
        }

        private async Task PingPong(
            Cluster cluster,
            string id,
            CancellationToken token = default,
            string kind = EchoActor.Kind
        )
        {
            await Task.Yield();

            Pong response = await cluster.Ping(id, id, new CancellationTokenSource(4000).Token, kind);
            int tries = 1;

            while (response == null && !token.IsCancellationRequested)
            {
                await Task.Delay(200, token);
                _testOutputHelper.WriteLine($"Retrying ping {kind}/{id}, attempt {++tries}");
                response = await cluster.Ping(id, id, new CancellationTokenSource(4000).Token, kind);
            }

            response.Should().NotBeNull($"We expect a response before timeout on {kind}/{id}");

            response.Should().BeEquivalentTo(new Pong {Identity = id, Kind = kind, Message = id},
                "Echo should come from the correct virtual actor"
            );
        }
    }

    // ReSharper disable once UnusedType.Global
    public class InMemoryClusterTests : ClusterTests, IClassFixture<InMemoryClusterFixture>
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public InMemoryClusterTests(ITestOutputHelper testOutputHelper, InMemoryClusterFixture clusterFixture) : base(
            testOutputHelper, clusterFixture
        )
        {
        }
    }
}
