using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;
using EasyMongo.Collections;
using System.Reflection;

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

        internal DataContext(IMongoDatabase database, MappingSource mappingSource, bool entityTrackingEnabled)
        {
            this.m_mappingSource = mappingSource;
            this.m_collections = new Dictionary<Type, object>();

            this.EntityTrackingEnabled = entityTrackingEnabled;
            this.m_database = database;
        }

        public bool EntityTrackingEnabled { get; private set; }
        public MongoDatabase Database { get { return (MongoDatabase)this.m_database; } }

        public T Get<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var mapper = this.m_mappingSource.GetEntityMapper<T>();
            var predicateDoc = mapper.GetPredicate(predicate.Body);
            var fieldsDoc = mapper.GetFields(null);

            this.m_database.Open();
            var coll = mapper.GetCollection(this.m_database);

            var doc = coll.Find(predicateDoc, 1, 0, fieldsDoc).Documents.SingleOrDefault();
            if (doc == null) return default(T);

            var entity = mapper.GetEntity(doc);
            this.TrackEntityState(mapper, entity);
            return (T)entity;
        }

        public Query<T> Query<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return this.Query<T>().Where(predicate);
        }

        public Query<T> Query<T>() where T : class
        {
            return new Query<T>(this);
        }

        internal List<T> List<T>(Expression predicate, int skip, int? limit, List<SortOrder> sortOrders, Expression selector) where T : class
        {
            var mapper = this.m_mappingSource.GetEntityMapper<T>();
            var predicateDoc = mapper.GetPredicate(predicate);
            var fieldsDoc = mapper.GetFields(selector);
            var sortDoc = mapper.GetSortOrders(sortOrders);

            this.m_database.Open();
            var coll = mapper.GetCollection(this.m_database);

            var mongoQuery = coll.Find(predicateDoc).Fields(fieldsDoc).Skip(skip).Sort(sortDoc);
            if (limit.HasValue) mongoQuery = mongoQuery.Limit(limit.Value);

            var docList = mongoQuery.Documents.ToList();

            var result = new List<T>(docList.Count);
            foreach (var doc in docList)
            {
                var entity = mapper.GetEntity(doc);

                this.TrackEntityState(mapper, entity);

                result.Add((T)entity);
            }

            return result;
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

            this.UpdateEntityState();

            this.SaveEntityAdded();
        }

        private void SaveEntityAdded()
        {
            if (this.m_itemAdded == null) return;

            foreach (var mapper in this.m_itemAdded.Keys.ToList())
            {
                var entitiesToAdd = this.m_itemAdded[mapper];
                var documents = entitiesToAdd.Select(mapper.GetDocument);

                mapper.GetCollection(this.m_database).Insert(documents);

                foreach (var entity in entitiesToAdd)
                {
                    this.TrackEntityState(mapper, entity);
                }

                this.m_itemAdded.RemoveAll(mapper);
            }

            this.m_itemAdded = null;
        }

        private void UpdateEntityState()
        {
            if (this.m_stateLoaded == null) return;

            this.m_database.Open();

            foreach (var pair in this.m_stateLoaded.ToList())
            {
                var entity = pair.Key;
                var originalState = pair.Value;
                var mapper = originalState.Mapper;
                var currentState = mapper.GetEntityState(entity);

                var docToUpdate = mapper.GetStateChanged(originalState, currentState);
                var predicate = mapper.GetIdentity(entity);
                var coll = mapper.GetCollection(this.m_database);

                coll.Update(docToUpdate, predicate);
                this.m_stateLoaded[entity] = currentState;
            }
        }

        private Dictionary<object, EntityState> m_stateLoaded;
        private void EnsureStateLoadedCreated()
        {
            if (this.m_stateLoaded == null)
            {
                this.m_stateLoaded = new Dictionary<object, EntityState>();
            }
        }

        private void TrackEntityState(EntityMapper mapper, object entity)
        {
            if (!this.EntityTrackingEnabled) return;

            var state = mapper.GetEntityState(entity);
            this.EnsureStateLoadedCreated();
            this.m_stateLoaded.Add(entity, state);
        }

        public void Attach<T>(T entity)
        {
            var mapper = this.m_mappingSource.GetEntityMapper<T>();
            this.TrackEntityState(mapper, entity);
        }

        public void Dispose()
        {
            this.m_database.Dispose();
        }
    }
}
