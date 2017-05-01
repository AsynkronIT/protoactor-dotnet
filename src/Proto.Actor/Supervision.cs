﻿// -----------------------------------------------------------------------
//  <copyright file="Supervision.cs" company="Asynkron HB">
//      Copyright (C) 2015-2016 Asynkron HB All rights reserved
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Proto
{
    public enum SupervisorDirective
    {
        Resume,
        Restart,
        Stop,
        Escalate
    }

    public interface ISupervisor
    {
        IReadOnlyCollection<PID> Children { get; }
        void EscalateFailure(PID who, Exception reason);
        void RestartChildren(params PID[] pids);
        void StopChildren(params PID[] pids);
        void ResumeChildren(params PID[] pids);
    }

    public static class Supervision
    {
        public static ISupervisorStrategy DefaultStrategy { get; } =
            new OneForOneStrategy((who, reason) => SupervisorDirective.Restart, 10, TimeSpan.FromSeconds(10));
    }

    public interface ISupervisorStrategy
    {
        Task HandleFailure(ISupervisor supervisor, PID child, RestartStatistics rs, Exception cause);
    }

    public delegate SupervisorDirective Decider(PID pid, Exception reason);

    public class OneForOneStrategy : ISupervisorStrategy
    {
        private readonly int _maxNrOfRetries;
        private readonly TimeSpan? _withinTimeSpan;
        private readonly Decider _decider;
        private static readonly ILogger Logger = Log.CreateLogger<OneForOneStrategy>();

        public OneForOneStrategy(Decider decider, int maxNrOfRetries, TimeSpan? withinTimeSpan)
        {
            _decider = decider;
            _maxNrOfRetries = maxNrOfRetries;
            _withinTimeSpan = withinTimeSpan;
        }

        public Task HandleFailure(ISupervisor supervisor, PID child, RestartStatistics rs, Exception reason)
        {
            var directive = _decider(child, reason);
            switch (directive)
            {
                case SupervisorDirective.Resume:
                    supervisor.ResumeChildren(child);
                    break;
                case SupervisorDirective.Restart:
                    if (RequestRestartPermission(rs))
                    {
                        Logger.LogInformation($"Restarting {child.ToShortString()} Reason {reason}");
                        supervisor.RestartChildren(child);
                    }
                    else
                    {
                        Logger.LogInformation($"Stopping {child.ToShortString()} Reason { reason}");
                        supervisor.StopChildren(child);
                    }
                    break;
                case SupervisorDirective.Stop:
                    Logger.LogInformation($"Stopping {child.ToShortString()} Reason {reason}");
                    supervisor.StopChildren(child);
                    break;
                case SupervisorDirective.Escalate:
                    supervisor.EscalateFailure(child, reason);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return Actor.Done;
        }

        private bool RequestRestartPermission(RestartStatistics rs)
        {
            if (_maxNrOfRetries == 0)
            {
                return false;
            }
            rs.Fail();
            if (_withinTimeSpan == null || rs.IsWithinDuration(_withinTimeSpan.Value))
            {
                return rs.FailureCount <= _maxNrOfRetries;
            }
            rs.Reset();
            return true;
        }
    }
}