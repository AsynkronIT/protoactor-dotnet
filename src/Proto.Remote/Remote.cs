﻿// -----------------------------------------------------------------------
//   <copyright file="Remote.cs" company="Asynkron HB">
//       Copyright (C) 2015-2018 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------
// Modified file in context of repo fork : https://github.com/Optis-World/protoactor-dotnet
// Copyright 2019 ANSYS, Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Proto.Remote
{
    public static class Remote
    {
        private static readonly ILogger Logger = Log.CreateLogger(typeof(Remote).FullName);

        private static Server _server;
        private static readonly Dictionary<string, Props> Kinds = new Dictionary<string, Props>();
        public static RemoteConfig RemoteConfig { get; private set; }
        public static PID ActivatorPid { get; private set; }

        private static EndpointReader _endpointReader;

        public static string[] GetKnownKinds()
        {
            return Kinds.Keys.ToArray();
        }

        public static void RegisterKnownKind(string kind, Props props)
        {
            Kinds.Add(kind, props);
        }

        // Modified class in context of repo fork : https://github.com/Optis-World/protoactor-dotnet
        public static void UnregisterKnownKind(string kind)
        {
            Kinds.Remove(kind);
        }
        
        public static Props GetKnownKind(string kind)
        {
            if (Kinds.TryGetValue(kind, out var props)){
                return props;
            }
            throw new ArgumentException($"No Props found for kind '{kind}'");
        }

        public static void Start(string hostname, int port)
        {
            Start(hostname, port, new RemoteConfig());
        }

        public static void Start(string hostname, int port, RemoteConfig config)
        {
            RemoteConfig = config;

            ProcessRegistry.Instance.RegisterHostResolver(pid => new RemoteProcess(pid));

            EndpointManager.Start();
            _endpointReader = new EndpointReader();
            _server = new Server
            {
                Services = { Remoting.BindService(_endpointReader) },
                Ports = { new ServerPort(hostname, port, config.ServerCredentials) }
            };
            _server.Start();

            var boundPort = _server.Ports.Single().BoundPort;
            var boundAddr = $"{hostname}:{boundPort}";
            var addr = $"{config.AdvertisedHostname??hostname}:{config.AdvertisedPort?? boundPort}";
            ProcessRegistry.Instance.Address = addr;

            SpawnActivator();

            Logger.LogDebug($"Starting Proto.Actor server on {boundAddr} ({addr})");
        }

        public static void Shutdown(bool gracefull = true)
        {
            try
            {
                if (gracefull)
                {
                    EndpointManager.Stop();
                    _endpointReader.Suspend(true);
                    StopActivator();
                    _server.ShutdownAsync().Wait(10000);
                }
                else
                {
                    _server.KillAsync().Wait(10000);
                }
                
                Logger.LogDebug($"Proto.Actor server stopped on {ProcessRegistry.Instance.Address}. Graceful:{gracefull}");
            }
            catch(Exception ex)
            {
                _server.KillAsync().Wait(1000);
                Logger.LogError($"Proto.Actor server stopped on {ProcessRegistry.Instance.Address} with error:\n{ex.Message}");
            }
        }

        private static void SpawnActivator()
        {
            var props = Props.FromProducer(() => new Activator()).WithGuardianSupervisorStrategy(Supervision.AlwaysRestartStrategy);
            ActivatorPid = RootContext.Empty.SpawnNamed(props, "activator");
        }

        private static void StopActivator()
        {
            ActivatorPid.Stop();
        }

        public static PID ActivatorForAddress(string address)
        {
            return new PID(address, "activator");
        }

        public static Task<ActorPidResponse> SpawnAsync(string address, string kind, TimeSpan timeout)
        {
            return SpawnNamedAsync(address, "", kind, timeout);
        }

        public static async Task<ActorPidResponse> SpawnNamedAsync(string address, string name, string kind, TimeSpan timeout)
        {
            var activator = ActivatorForAddress(address);

            var res = await RootContext.Empty.RequestAsync<ActorPidResponse>(activator, new ActorPidRequest
            {
                Kind = kind,
                Name = name
            }, timeout);

            return res;
        }

        public static void SendMessage(PID pid, object msg, int serializerId)
        {
            var (message, sender, header) = Proto.MessageEnvelope.Unwrap(msg);

            var env = new RemoteDeliver(header, message, pid, sender, serializerId);
            EndpointManager.RemoteDeliver(env);
        }
    }
}
