﻿// -----------------------------------------------------------------------
// <copyright file="SharedFutureTests.cs" company="Asynkron AB">
//      Copyright (C) 2015-2021 Asynkron AB All rights reserved
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Proto.Future;
using Xunit;

namespace Proto.Tests
{
    public class SharedFutureTests : BaseFutureTests
    {
        private readonly SharedFutureProcess _sharedFutureProcess;

        public SharedFutureTests() => _sharedFutureProcess = new SharedFutureProcess(System, BatchSize);

        protected override IFuture GetFuture() => _sharedFutureProcess.TryCreateHandle() ?? throw new Exception("No futures available");

        [Fact]
        public async Task Should_reuse_completed_futures()
        {
            // first test should use all available futures, and should return them when done
            await Futures_should_map_to_correct_response();

            //After they are returned, they should be available for the second test.
            await Futures_should_map_to_correct_response();
        }

        [Fact]
        public void Should_not_give_out_more_futures_than_size_allows()
        {
            using var process = new SharedFutureProcess(System, 5);

            for (int i = 0; i < 5; i++)
            {
                process.TryCreateHandle().Should().NotBeNull();
            }

            process.TryCreateHandle().Should().BeNull();
        }
    }
}