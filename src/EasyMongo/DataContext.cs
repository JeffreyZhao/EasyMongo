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

        private Dictionary<object, EntityState> m_stateLoaded;
        private void EnsureStateLoadedCreated()
        {
            if (this.m_stateLoaded == null)
            {
                this.m_stateLoaded = new Dictionary<object, EntityState>();
            }
        }

        internal int Count<T>(Expression predicate)
        {
            var mapper = this.m_mappingSource.GetEntityMapper<T>();
            var predicateDoc = mapper.GetPredicate(predicate);
            var coll = mapper.GetCollection(this.m_database);
            return (int)coll.Count(predicateDoc);
        }

        public void InsertOnSubmit<T>(T item) where T : class
        {
            if (item == null) throw new ArgumentNullException();

            var mapper = this.m_mappingSource.GetEntityMapper<T>();
            if (mapper == null) throw new ArgumentException(typeof(T).FullName + " is not supported.");

            this.EnsureItemsToInsertCreated();
            this.m_itemsToInsert.Add(mapper, item);
        }

        private HashBag<EntityMapper, object> m_itemsToInsert;
        private void EnsureItemsToInsertCreated()
        {
            if (this.m_itemsToInsert == null)
            {
                this.m_itemsToInsert = new HashBag<EntityMapper, object>();
            }
        }

        public void DeleteOnSubmit<T>(T item) where T : class
        {
            if (item == null) throw new ArgumentNullException();

            var mapper = this.m_mappingSource.GetEntityMapper<T>();
            if (mapper == null) throw new ArgumentException(typeof(T).FullName + " is not supported.");

            this.EnsureItemsToDeleteCreated();
            this.m_itemsToDelete.Add(mapper, item);
        }

        private HashBag<EntityMapper, object> m_itemsToDelete;
        private void EnsureItemsToDeleteCreated()
        {
            if (this.m_itemsToDelete == null)
            {
                this.m_itemsToDelete = new HashBag<EntityMapper, object>();
            }
        }

        private void DeleteEntities()
        {
            if (this.m_itemsToDelete == null) return;

            foreach (var mapper in this.m_itemsToDelete.Keys)
            {
                foreach (var entity in this.m_itemsToDelete[mapper])
                {
                    var identityDoc = mapper.GetIdentity(entity);
                    var coll = mapper.GetCollection(this.m_database);
                    coll.Delete(identityDoc);

                    this.m_stateLoaded.Remove(entity);
                }
            }

            this.m_itemsToDelete = null;
        }

        public void SubmitChanges()
        {
            this.m_database.Open();

            this.DeleteEntities();

            this.UpdateEntites();

            this.InsertEntities();
        }

        private void InsertEntities()
        {
            if (this.m_itemsToInsert == null) return;

            foreach (var mapper in this.m_itemsToInsert.Keys.ToList())
            {
                var entitiesToAdd = this.m_itemsToInsert[mapper];
                var documents = entitiesToAdd.Select(mapper.GetDocument);

                var coll = mapper.GetCollection(this.m_database);
                coll.Insert(documents);

                foreach (var entity in entitiesToAdd)
                {
                    this.TrackEntityState(mapper, entity);
                }

                this.m_itemsToInsert.RemoveAll(mapper);
            }

            this.m_itemsToInsert = null;
        }

        private void UpdateEntites()
        {
            if (this.m_stateLoaded == null) return;

            foreach (var pair in this.m_stateLoaded.ToList())
            {
                var entity = pair.Key;
                var originalState = pair.Value;
                var mapper = originalState.Mapper;
                var currentState = mapper.GetEntityState(entity);

                var docToUpdate = mapper.GetStateChanged(entity, originalState, currentState);
                if (docToUpdate.Count == 0) continue;

                var predicate = mapper.GetIdentity(entity);
                var coll = mapper.GetCollection(this.m_database);

                coll.Update(docToUpdate, predicate);
                this.m_stateLoaded[entity] = currentState;
            }
        }

        private void TrackEntityState(EntityMapper mapper, object entity)
        {
            if (!this.EntityTrackingEnabled) return;

            var state = mapper.GetEntityState(entity);
            this.EnsureStateLoadedCreated();
            this.m_stateLoaded.Add(entity, state);
        }

        public void Attach<T>(T entity) where T : class
        {
            if (entity == null) throw new ArgumentNullException();

            var mapper = this.m_mappingSource.GetEntityMapper<T>();
            this.TrackEntityState(mapper, entity);
        }

        public void Dispose()
        {
            this.m_database.Dispose();
        }
    }
}
