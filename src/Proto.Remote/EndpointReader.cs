﻿// -----------------------------------------------------------------------
//   <copyright file="EndpointReader.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Utils;
using Proto.Mailbox;

namespace Proto.Remote
{
    public class EndpointReader : Remoting.RemotingBase
    {
        private bool suspended;
        
        public override Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ConnectResponse()
            {
                DefaultSerializerId = Serialization.DefaultSerializerId
            });
        }

        public override async Task Receive(IAsyncStreamReader<MessageBatch> requestStream,
            IServerStreamWriter<Unit> responseStream, ServerCallContext context)
        {
            await requestStream.ForEachAsync(batch =>
            {
                if (suspended)
                    return Actor.Done;
                
                var targetNames = new List<string>(batch.TargetNames);
                var typeNames = new List<string>(batch.TypeNames);
                foreach (var envelope in batch.Envelopes)
                {
                    var targetName = targetNames[envelope.Target];
                    var target = new PID(ProcessRegistry.Instance.Address, targetName);
                  
                    var typeName = typeNames[envelope.TypeId];

                    var message = Serialization.Deserialize(typeName, envelope.MessageData, envelope.SerializerId);

                    if (message is Terminated msg)
                    {
                        var rt = new RemoteTerminate(target, msg.Who);
                        Remote.EndpointManagerPid.Tell(rt);
                    }
                    else if (message is SystemMessage sys)
                    {
                        target.SendSystemMessage(sys);
                    }
                    else
                    {
                        if (envelope.Sender != null)
                            target.Request(message, envelope.Sender);
                        else
                            target.Tell(message);
                    }
                }

                return Actor.Done;
            });
        }

        public void Suspend(bool suspended)
        {
            this.suspended = suspended;
        }
    }
}