using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace EasyMongo
{
    public class MongoDatabase : IMongoDatabase, IDisposable
    {
        public MongoDatabase(string host, int port, string dbName)
        {
            this.m_dbName = dbName;
            this.m_mongo = new Mongo(host, port);
        }

        private string m_dbName;
        private Mongo m_mongo;
        private Database m_db;

        public bool IsOpen { get; private set; }

        public void Open()
        {
            if (this.IsOpen) return;

            this.m_mongo.Connect();
            this.m_db = this.m_mongo[this.m_dbName];

            this.IsOpen = true;
        }

        public void Dispose()
        {
            this.m_mongo.Dispose();
        }

        public string Name
        {
            get
            {
                return this.m_dbName;
            }
        }

        public IMongoCollection this[string collName]
        {
            get
            {
                return this.m_db[collName];
            }
        }
    }
}
