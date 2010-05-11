using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace EasyMongo
{
    internal interface IMongoDatabase
    {
        void Open();
        void Dispose();
        IMongoCollection this[string collName] { get; }
    }
}
