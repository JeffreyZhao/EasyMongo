using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;
using EasyMongo.Collections;

namespace EasyMongo
{
    public class DataContext : IDisposable
    {
        private MappingSource m_mappingSource;
        private Dictionary<Type, object> m_collections;
        private IMongoDatabase m_database;

        public DataContext(MongoDatabase database, MappingSource mappingSource)
            : this(database, mappingSource, true)
        { }

        public DataContext(MongoDatabase database, MappingSource mappingSource, bool objectTrackingEnabled)
            : this((IMongoDatabase)database, mappingSource, objectTrackingEnabled)
        { }

        internal DataContext(IMongoDatabase database, MappingSource mappingSource, bool objectTrackingEnabled)
        {
            this.m_mappingSource = mappingSource;
            this.m_collections = new Dictionary<Type, object>();

            this.ObjectTrackingEnabled = objectTrackingEnabled;
            this.m_database = database;
        }

        public bool ObjectTrackingEnabled { get; private set; }
        public MongoDatabase Database { get { return (MongoDatabase)this.m_database; } }

        public T Get<T>(Expression<Func<T, bool>> predicate)
        {
            var mapper = this.m_mappingSource.GetEntityMapper<T>();
            var predicateDoc = mapper.GetPredicate(predicate.Body);

            return default(T);
        }

        private HashBag<EntityMapper, object> m_itemAdded;
        private void EnsureItemAddedCreated()
        {
            if (this.m_itemAdded == null)
            {
                this.m_itemAdded = new HashBag<EntityMapper, object>();
            }
        }

        public void Add<T>(T item)
        {
            var mapper = this.m_mappingSource.GetEntityMapper<T>();
            if (mapper == null) throw new ArgumentException(typeof(T).FullName + " is not supported.");

            this.EnsureItemAddedCreated();
            this.m_itemAdded.Add(mapper, item);
        }

        public void SubmitChanges()
        {
            this.m_database.Open();

            this.SaveItemAdded();
        }

        private void SaveItemAdded()
        {
            if (this.m_itemAdded == null) return;

            foreach (var mapper in this.m_itemAdded.Keys.ToList())
            {
                var entities = this.m_itemAdded[mapper];
                var documents = entities.Select(mapper.GetDocument);

                mapper.GetCollection(this.m_database).Insert(documents);

                // TODO: copy to loaded set
                this.m_itemAdded.RemoveAll(mapper);
            }

            this.m_itemAdded = null;
        }

        public void Dispose()
        {
            this.m_database.Dispose();
        }
    }
}
