using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;
using MongoDB.Bson;
using System.Collections;
using System.IO;
using System.Reflection;
using EasyMongo.Expressions;

namespace EasyMongo
{
    public class EntityCollection<TEntity> : ICountableQueryableCollection<TEntity> where TEntity : class, new()
    {
        private static EntityMapperCache<TEntity> s_mapperCache = new EntityMapperCache<TEntity>();

        public EntityCollection(MongoDatabase database, IEntityDescriptor<TEntity> descriptor, bool entityTrackingEnabled)
        {
            this.Descriptor = descriptor;
            this.Database = database;
            this.EntityTrackingEnabled = entityTrackingEnabled;

            this.m_mapper = s_mapperCache.Get(descriptor);
        }

        public TextWriter Log { get; set; }

        public bool EntityTrackingEnabled { get; private set; }

        public MongoDatabase Database { get; private set; }

        public IEntityDescriptor<TEntity> Descriptor { get; private set; }

        private EntityMapper<TEntity> m_mapper;

        public TEntity Get(Expression<Func<TEntity, bool>> predicate)
        {
            var mapper = this.m_mapper;

            var predicateDoc = mapper.GetPredicate(predicate.Body);
            var fieldsDoc = mapper.GetFields((Expression)null);
            var collection = mapper.GetCollection(this.Database);

            this.Log.WriteQuery(collection, predicateDoc, fieldsDoc, null, null, 0, 1);

            var doc = collection.Find(predicateDoc).SetFields(fieldsDoc).SetLimit(1).FirstOrDefault();
            if (doc == null) return default(TEntity);

            var entity = mapper.GetEntity(doc);
            this.TrackEntityState(entity);
            return (TEntity)entity;
        }

        public void InsertOnSubmit(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException();

            this.EnsureItemsToInsertCreated();
            this.m_itemsToInsert.Add(entity);
        }

        public void Attach(TEntity entity)
        {
            this.TrackEntityState(entity);
        }

        public void DeleteOnSubmit(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException();

            if (this.m_itemsToInsert != null && this.m_itemsToInsert.Remove(entity))
            {
                return;
            }

            if (this.m_stateLoaded != null)
            {
                this.m_stateLoaded.Remove(entity);
            }

            this.EnsureItemsToDeleteCreated();
            this.m_itemsToDelete.Add(entity);
        }

        public void SubmitChanges()
        {
            using (this.Database.Server.RequestStart(this.Database))
            {
                this.DeleteEntities();
                this.UpdateEntites();
                this.InsertEntities();
            }
        }

        public void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            var predicateDoc = this.m_mapper.GetPredicate(predicate.Body);
            var collection = this.m_mapper.GetCollection(this.Database);
            collection.Remove(predicateDoc, RemoveFlags.None);
        }

        public void Update(
            Expression<Func<TEntity, TEntity>> updateSpec,
            Expression<Func<TEntity, bool>> predicate)
        {
            var mapper = this.m_mapper;

            var updateDoc = mapper.GetUpdates(updateSpec.Body);
            var predicateDoc = mapper.GetPredicate(predicate.Body);
            var collection = mapper.GetCollection(this.Database);

            collection.Update(predicateDoc, updateDoc);
        }

        public void UpdateVersion(Expression<Func<TEntity, bool>> predicate)
        {
            var mapper = this.m_mapper;

            var updateDoc = mapper.GetUpdates(null);
            var predicateDoc = mapper.GetPredicate(predicate.Body);
            var collection = mapper.GetCollection(this.Database);

            collection.Update(predicateDoc, updateDoc);
        }

        private void InsertEntities()
        {
            if (this.m_itemsToInsert == null) return;

            var documents = this.m_itemsToInsert.Select((Func<TEntity, BsonDocument>)this.m_mapper.GetDocument).ToList();
            var collection = this.m_mapper.GetCollection(this.Database);
            collection.InsertBatch(documents);

            foreach (var entity in this.m_itemsToInsert)
            {
                this.TrackEntityState(entity);
            }

            this.m_itemsToInsert = null;
        }

        private void UpdateEntites()
        {
            if (this.m_stateLoaded == null) return;

            var conflicts = new List<TEntity>();

            foreach (var pair in this.m_stateLoaded.ToList())
            {
                var entity = pair.Key;
                var originalState = pair.Value;
                var mapper = this.m_mapper;

                var currentState = mapper.GetEntityState(entity);
                var updateDoc = mapper.GetStateChanged(entity, originalState, currentState);
                if (updateDoc.ElementCount == 0) continue;

                var identityDoc = mapper.GetIdentity(entity);                
                var collection = mapper.GetCollection(this.Database);

                if (mapper.Versioning)
                {
                    var fieldsDoc = mapper.GetVersionField();

                    try
                    {
                        var result = collection.FindAndModify(identityDoc, null, updateDoc, fieldsDoc, true);
                        mapper.UpdateVersion(entity, result.ModifiedDocument);
                    }
                    catch (MongoCommandException)
                    {
                        conflicts.Add(entity);
                    }
                }
                else
                {
                    this.Log.WriteUpdate(collection, identityDoc, updateDoc);
                    collection.Update(identityDoc, updateDoc);
                }

                this.m_stateLoaded[entity] = currentState;
            }

            if (conflicts.Count > 0)
            { 
                throw new ChangeConflictException<TEntity>(conflicts);
            }
        }

        private void DeleteEntities()
        {
            if (this.m_itemsToDelete == null) return;

            var conflicts = new List<TEntity>();

            foreach (var entity in this.m_itemsToDelete)
            {
                var identityDoc = this.m_mapper.GetIdentity(entity);
                var collection = this.m_mapper.GetCollection(this.Database);
                
                var result = collection.Remove(identityDoc, RemoveFlags.Single, SafeMode.True);
                if (result.DocumentsAffected <= 0)
                {
                    conflicts.Add(entity);
                }
            }

            this.m_itemsToDelete.RemoveAll(e => !conflicts.Contains(e));

            if (conflicts.Count > 0)
            {
                throw new ChangeConflictException<TEntity>(conflicts);
            }
        }

        private void TrackEntityState(TEntity entity)
        {
            if (!this.EntityTrackingEnabled) return;

            this.EnsureStateLoadedCreated();

            var state = this.m_mapper.GetEntityState(entity);
            this.m_stateLoaded.Add(entity, state);
        }

        private List<TEntity> m_itemsToDelete;
        private void EnsureItemsToDeleteCreated()
        {
            if (this.m_itemsToDelete == null)
            {
                this.m_itemsToDelete = new List<TEntity>();
            }
        }

        private List<TEntity> m_itemsToInsert;
        private void EnsureItemsToInsertCreated()
        {
            if (this.m_itemsToInsert == null)
            {
                this.m_itemsToInsert = new List<TEntity>();
            }
        }

        private Dictionary<TEntity, EntityState> m_stateLoaded;
        private void EnsureStateLoadedCreated()
        {
            if (this.m_stateLoaded == null)
            {
                this.m_stateLoaded = new Dictionary<TEntity, EntityState>();
            }
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return Query<TEntity>.GetQuery(this).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal int Count(Expression predicate)
        {
            var predicateDoc = this.m_mapper.GetPredicate(predicate);
            var collection = this.m_mapper.GetCollection(this.Database);
            return collection.Count(predicateDoc);
        }

        internal List<TEntity> Load(Expression predicate, int skip, int? limit, List<SortOrder> sortOrders, List<QueryHint> hints, Expression selector)
        {
            var mapper = this.m_mapper;
            var predicateDoc = mapper.GetPredicate(predicate);
            var fieldsDoc = mapper.GetFields(selector);
            var sortDoc = mapper.GetSortOrders(sortOrders);

            var collection = mapper.GetCollection(this.Database);
            
            var mongoCursor = collection.Find(predicateDoc).SetFields(fieldsDoc).SetSortOrder(sortDoc).SetSkip(skip);
            
            if (limit.HasValue)
            {
                mongoCursor = mongoCursor.SetLimit(limit.Value);
            }

            var hintsDoc = (hints != null && hints.Count > 0) ? mapper.GetHints(hints) : null;
            if (hintsDoc != null) mongoCursor.SetHint(hintsDoc);

            this.Log.WriteQuery(collection, predicateDoc, fieldsDoc, sortDoc, hintsDoc, skip, limit);

            var docList = mongoCursor.ToList();

            var result = new List<TEntity>(docList.Count);
            foreach (var doc in docList)
            {
                var entity = mapper.GetEntity(doc);
                this.TrackEntityState(entity);
                result.Add(entity);
            }

            return result;
        }

        internal List<TResult> LoadTo<TResult>(Expression predicate, int skip, int? limit, List<SortOrder> sortOrders, List<QueryHint> hints, Expression selector)
        {
            Func<Dictionary<PropertyInfo, object>, TResult> generator;
            var propertyExtractor = new PropertyExtractor();
            var properties = propertyExtractor.Extract<TResult>(selector, out generator);

            var mapper = this.m_mapper;
            var predicateDoc = mapper.GetPredicate(predicate);
            var fieldsDoc = this.m_mapper.GetFields(properties);
            var sortDoc = mapper.GetSortOrders(sortOrders);

            var collection = mapper.GetCollection(this.Database);

            var mongoCursor = collection.Find(predicateDoc).SetFields(fieldsDoc).SetSortOrder(sortDoc).SetSkip(skip);

            if (limit.HasValue)
            {
                mongoCursor = mongoCursor.SetLimit(limit.Value);
            }

            var hintsDoc = (hints != null && hints.Count > 0) ? mapper.GetHints(hints) : null;
            if (hintsDoc != null) mongoCursor.SetHint(hintsDoc);

            this.Log.WriteQuery(collection, predicateDoc, fieldsDoc, sortDoc, hintsDoc, skip, limit);

            var docList = mongoCursor.ToList();

            var result = new List<TResult>(docList.Count);
            foreach (var doc in docList)
            {
                var values = mapper.GetValues(properties, doc);
                result.Add(generator(values));
            }

            return result;
        }
    }
}
