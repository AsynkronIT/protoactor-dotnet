﻿// -----------------------------------------------------------------------
//  <copyright file="MongoDBProvider.cs" company="Asynkron HB">
//      Copyright (C) 2015-2017 Asynkron HB All rights reserved
//  </copyright>
// -----------------------------------------------------------------------

using MongoDB.Driver;

namespace Proto.Persistence.MongoDB
{
    public class MongoDBProvider : IProvider
    {
        private readonly IMongoDatabase _mongoDB;

        public MongoDBProvider(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public IEventState GetEventState()
        {
            return new MongoDBProviderState(_mongoDB);
        }

        public ISnapshotState GetSnapshotState()
        {
            return new MongoDBProviderState(_mongoDB);
        }
    }
}