﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using KafkaVirtualActorIngress.Messages;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Consul;
using Proto.Cluster.Identity;
using Proto.Cluster.Identity.Redis;
using Proto.Remote;
using Proto.Remote.GrpcCore;
using StackExchange.Redis;

namespace KafkaVirtualActorIngress
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            ActorSystemConfig systemConfig = GetSystemConfig();
            GrpcCoreRemoteConfig remoteConfig = GetRemoteConfig();
            ClusterConfig clusterConfig = GetClusterConfig("my-cluster");

            ActorSystem system = new ActorSystem(systemConfig)
                .WithRemote(remoteConfig)
                .WithCluster(clusterConfig);

            Cluster cluster = system.Cluster();
            await cluster.StartMemberAsync();

            await RunKafkaConsumeLoop(cluster);
        }

        private static async Task RunKafkaConsumeLoop(Cluster cluster)
        {
            while (true)
            {
                Stopwatch sw = Stopwatch.StartNew();
                //get the messages from Kafka or other log/queue
                IEnumerable<MyEnvelope> messages = GetBatchFromKafka();
                List<Task> tasks = new List<Task>();

                //forward each message to their actors
                foreach (MyEnvelope message in messages)
                {
                    object m = message.MessageCase switch
                    {
                        MyEnvelope.MessageOneofCase.SomeMessage      => message.SomeMessage,
                        MyEnvelope.MessageOneofCase.SomeOtherMessage => message.SomeOtherMessage
                    };

                    Task<Ack> task = cluster
                        .RequestAsync<Ack>(message.DeviceId, "device", m, CancellationTokens.WithTimeout(5000));

                    tasks.Add(task);
                }

                //await response form all actors
                await Task.WhenAll(tasks);
                //TODO: commit back to Kafka that all messages succeeded
                sw.Stop();
                double tps = 1000.0 / sw.Elapsed.TotalMilliseconds * tasks.Count;

                //show throughput, messages per second
                Console.WriteLine(tps.ToString("n0"));
            }
        }

        private static IEnumerable<MyEnvelope> GetBatchFromKafka()
        {
            //Fake Kafka consumer message generator
            List<MyEnvelope> messages = new List<MyEnvelope>();
            Random rnd = new Random();
            for (int i = 0; i < 50; i++)
            {
                MyEnvelope message = new MyEnvelope
                {
                    DeviceId = rnd.Next(1, 1000).ToString(),
                    SomeMessage = new SomeMessage {Data = Guid.NewGuid().ToString()}
                };
                messages.Add(message);

                MyEnvelope message2 = new MyEnvelope
                {
                    DeviceId = rnd.Next(1, 1000).ToString(),
                    SomeOtherMessage = new SomeOtherMessage {IntProperty = rnd.Next(1, 100000)}
                };
                messages.Add(message2);
            }

            return messages;
        }

        private static ActorSystemConfig GetSystemConfig() =>
            ActorSystemConfig
                .Setup()
                .WithDeadLetterThrottleCount(3)
                .WithDeadLetterThrottleInterval(TimeSpan.FromSeconds(1))
                .WithDeveloperSupervisionLogging(true);
        //TODO: Uncomment to enable metrics
        //  .WithMetricsProviders(new StatsdConfigurator(new[] { new Label("service", "my-system-name") }));

        private static GrpcCoreRemoteConfig GetRemoteConfig() => GrpcCoreRemoteConfig
            .BindTo("127.0.0.1")
            //   .WithAdvertisedHost("the hostname or ip of this pod")
            .WithProtoMessages(MyMessagesReflection.Descriptor);

        private static ClusterConfig GetClusterConfig(string clusterName) => ClusterConfig
            .Setup(clusterName, GetClusterProvider(), new IdentityStorageLookup(GetIdentityLookup(clusterName)))
            .WithClusterKind("device", Props.FromProducer(() => new DeviceActor())
                //TODO: Uncomment to enable tracing
                // .WithOpenTracing()

                //TODO: Uncomment to enable local affinity
                // .WithPoisonOnRemoteTraffic(0.1f)
                // .WithPidCacheInvalidation()
            );

        //TODO: Uncomment to enable local affinity
        // .WithMemberStrategyBuilder((cluster, kind) => {
        //         if (kind == "device")
        //         {
        //             return new LocalAffinityStrategy(cluster, 500000);
        //         }
        //
        //         return null!;
        //     }
        // );

        private static IClusterProvider GetClusterProvider() =>
            new ConsulProvider(new ConsulProviderConfig());

        private static IIdentityStorage GetIdentityLookup(string clusterName) =>
            new RedisIdentityStorage(clusterName, ConnectionMultiplexer
                .Connect("localhost:6379" /* use proper config */)
            );
    }
}
