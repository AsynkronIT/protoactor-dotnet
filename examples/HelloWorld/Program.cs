﻿// -----------------------------------------------------------------------
//  <copyright file="Program.cs" company="Asynkron HB">
//      Copyright (C) 2015-2017 Asynkron HB All rights reserved
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Proto;

class Program
{
    static void Main(string[] args)
    {
        Main2().GetAwaiter().GetResult();
    }
    
    public static async Task Main2()
    {
        var props = Actor.FromProducer(() => new HelloActor());
        var pid = Actor.Spawn(props);
        await pid.SendAsync(new Hello
        {
            Who = "ProtoActor"
        });
        Console.ReadLine();
    }

    internal class Hello
    {
        public string Who;
    }

    internal class HelloActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            var msg = context.Message;
            if (msg is Hello r)
            {
                Console.WriteLine($"Hello {r.Who}");
            }
            return Actor.Done;
        }
    }
}