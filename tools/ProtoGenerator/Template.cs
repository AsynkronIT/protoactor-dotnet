﻿// -----------------------------------------------------------------------
//   <copyright file="Template.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

namespace ProtoBuf
{
    public static class Template
    {
        public static string Code = @"
using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Proto;
using Proto.Cluster;
using Proto.Remote;

namespace {{CsNamespace}}
{
    public static class Grains
    {
		{{#each Services}}	
        internal static Func<I{{Name}}> _{{Name}}Factory;

        public static void {{Name}}Factory(Func<I{{Name}}> factory) 
        {
            _{{Name}}Factory = factory;
            Remote.RegisterKnownKind(""{{Name}}"", Actor.FromProducer(() => new {{Name}}Actor()));
        } 

        public static {{Name}}Client {{Name}}(string id) => new {{Name}}Client(id);
		{{/each}}
    }

	{{#each Services}}	
    public interface I{{Name}}
    {
		{{#each Methods}}
        Task<{{OutputName}}> {{Name}}({{InputName}} request);
		{{/each}}
    }

    public class {{Name}}Client
    {
        private readonly string _id;

        public {{Name}}Client(string id)
        {
            _id = id;
        }

		{{#each Methods}}
        public async Task<{{OutputName}}> {{Name}}({{InputName}} request)
        {
            var pid = await Cluster.GetAsync(_id, ""{{../Name}}"");
            var gr = new GrainRequest
            {
                Method = ""{{Name}}"",
                MessageData = request.ToByteString()
            };
            var res = await pid.RequestAsync<object>(gr);
            if (res is GrainResponse grainResponse)
            {
                return {{OutputName}}.Parser.ParseFrom(grainResponse.MessageData);
            }
            if (res is GrainErrorResponse grainErrorResponse)
            {
                throw new Exception(grainErrorResponse.Err);
            }
            throw new NotSupportedException();
        }
		{{/each}}
    }

    public class {{Name}}Actor : IActor
    {
        private I{{Name}} _inner;

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                {
                    _inner = Grains._{{Name}}Factory();
                    break;
                }
                case GrainRequest request:
                {
                    switch (request.Method)
                    {
						{{#each Methods}}
                        case ""SayHello"":
                        {
                            var r = {{InputName}}.Parser.ParseFrom(request.MessageData);
                            try
                            {
                                var res = await _inner.{{Name}}(r);
                                var grainResponse = new GrainResponse
                                {
                                    MessageData = res.ToByteString(),
                                };
                                context.Respond(grainResponse);
                            }
                            catch (Exception x)
                            {
                                var grainErrorResponse = new GrainErrorResponse
                                {
                                    Err = x.ToString()
                                };
                                context.Respond(grainErrorResponse);
                            }

                            break;
                        }
						{{/each}}
                    }

                    break;
                }
            }
        }
    }
	{{/each}}	
}
";
    }
}
