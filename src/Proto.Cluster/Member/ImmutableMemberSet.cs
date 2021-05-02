// -----------------------------------------------------------------------
// <copyright file="ImmutableMemberSet.cs" company="Asynkron AB">
//      Copyright (C) 2015-2021 Asynkron AB All rights reserved
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Proto.Cluster
{
    public class ImmutableMemberSet
    {
        public static readonly ImmutableMemberSet Empty = new(Array.Empty<Member>());
        
        public uint TopologyHash { get; }
        public IReadOnlyCollection<Member> Members { get; }
        public ImmutableDictionary<string,Member> Lookup { get; }
        
        public ImmutableMemberSet(IEnumerable<Member> members)
        {
            Members = members.OrderBy(m => m.Id).ToArray();
            TopologyHash = Member.TopologyHash(members);
            Lookup = members.ToImmutableDictionary(m => m.Id);
        }

        public bool Contains(string id) => Lookup.ContainsKey(id);

        public ImmutableMemberSet Except(ImmutableMemberSet other)
        {
            var both = Members.Except(other.Members);
            return new ImmutableMemberSet(both);
        }
        
        public ImmutableMemberSet Union(ImmutableMemberSet other)
        {
            var both = Members.Union(other.Members);
            return new ImmutableMemberSet(both);
        }

        public Member? GetById(string id)
        {
            Lookup.TryGetValue(id, out var res);
            return res;
        }
    }
}