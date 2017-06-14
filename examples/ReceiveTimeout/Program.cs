﻿// -----------------------------------------------------------------------
//  <copyright file="Program.cs" company="Asynkron HB">
//      Copyright (C) 2015-2017 Asynkron HB All rights reserved
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading;
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
        var c = 0;
        var props = Actor.FromFunc(context =>
        {
            switch (context.Message)
            {
                case Started _:
                    Console.WriteLine($"{DateTime.Now} Started");
                    context.SetReceiveTimeout(TimeSpan.FromSeconds(1));
                    break;
                case ReceiveTimeout _:
                    c++;
                    Console.WriteLine($"{DateTime.Now} ReceiveTimeout: {c}");
                    break;
                case NoInfluence _:
                    Console.WriteLine($"{DateTime.Now} Received a no-influence message");
                    break;
                case string s:
                    Console.WriteLine($"{DateTime.Now} Received message: {s}");
                    break;
            }
            return Actor.Done;
        });
        var pid = Actor.Spawn(props);
        for (var i = 0; i < 6; i++)
        {
            await pid.SendAsync("hello");
            Thread.Sleep(500);
        }

        Console.WriteLine("Hit [return] to send no-influence messages");
        Console.ReadLine();

        for (var i = 0; i < 6; i++)
        {
            await pid.SendAsync(new NoInfluence());
            Thread.Sleep(500);
        }

        Console.WriteLine("Hit [return] to send a message to cancel the timeout");
        Console.ReadLine();

        await pid.SendAsync("cancel");

        Console.WriteLine("Hit [return] to finish");
        Console.ReadLine();
    }
}

internal class NoInfluence : INotInfluenceReceiveTimeout
{
}