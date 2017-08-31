﻿// -----------------------------------------------------------------------
//   <copyright file="MemberList.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Proto.Cluster
{
    public static class MemberList
    {
        private static readonly Random Random = new Random();
        public static PID Pid { get; private set; }

        internal static void SubscribeToEventStream()
        {
            Actor.EventStream.Subscribe<ClusterTopologyEvent>(Pid.Tell);
        }

        internal static void Spawn()
        {
            Pid = Actor.SpawnNamed(Actor.FromProducer(() => new MemberListActor()), "memberlist");
        }

        public static async Task<string[]> GetMembersAsync(string kind)
        {
            //if there are no nodes holding the requested kind, just wait
            while (true)
            {
                var res = await Pid.RequestAsync<MemberByKindResponse>(new MemberByKindRequest(kind, true));
                if (res.Kinds.Any())
                {
                    return res.Kinds;
                }
                await Task.Delay(500);
            }
        }

        public static async Task<string> GetRandomActivatorAsync(string kind)
        {
            var r = Random.Next();
            var members = await GetMembersAsync(kind);
            return members[r % members.Length];
        }

        public static async Task<string> GetMemberAsync(string name, string kind)
        {
            var members = await GetMembersAsync(kind);
            var hashring = new HashRing(members);
            var member = hashring.GetNode(name);
            return member;
        }
    }

    internal class MemberByKindResponse
    {
        public MemberByKindResponse(string[] kinds)
        {
            Kinds = kinds ?? throw new ArgumentNullException(nameof(kinds));
        }

        public string[] Kinds { get; set; }
    }

    internal class MemberByKindRequest
    {
        public MemberByKindRequest(string kind, bool onlyAlive)
        {
            Kind = kind ?? throw new ArgumentNullException(nameof(kind));
            OnlyAlive = onlyAlive;
        }

        public string Kind { get; }
        public bool OnlyAlive { get; }
    }
}