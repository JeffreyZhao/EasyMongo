using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;
using EasyMongo.Reflection;
using MongoDB.Bson;

namespace EasyMongo
{
    internal class EntityMapper<TEntity> where TEntity : class, new()
    {
        public EntityMapper(IEntityDescriptor<TEntity> descriptor)
        {
            this.Descriptor = descriptor;

            var identityCount = descriptor.Properties.Count(d => d.IsIdentity);
            if (identityCount <= 0)
            {
                throw new ArgumentException("No identity specified.");
            }
            else
            {
                this.m_allProperties = descriptor.Properties.ToDictionary(
                    d => d.Property,
                    d => new PropertyMapper(d, d.IsIdentity ? (identityCount == 1) : false));
            }

            this.m_identities = this.m_allProperties.Values
                .Where(m => m.Descriptor.IsIdentity)
                .ToDictionary(p => p.Descriptor.Property);

            this.m_version = this.m_allProperties.Values.SingleOrDefault(p => p.Descriptor.IsVersion);
        }

        private Dictionary<PropertyInfo, PropertyMapper> m_allProperties;
        private Dictionary<PropertyInfo, PropertyMapper> m_identities;
        private PropertyMapper m_version;

        public IEntityDescriptor<TEntity> Descriptor { get; private set; }

        public bool Versioning { get { return this.m_version != null; } }

        public MongoCollection<BsonDocument> GetCollection(MongoDatabase database)
        {
            return database.GetCollection(this.Descriptor.CollectionName);
        }

        public QueryDocument GetPredicate(Expression predicateExpr)
        {
            var propPredicates = new PredicateCollector().Collect(predicateExpr);

            var predicateDoc = new QueryDocument();
            foreach (var predicate in propPredicates)
            {
                var propertyMapper = this.m_allProperties[predicate.Property];
                predicate.Fill(propertyMapper, predicateDoc);
            }

            return predicateDoc;
        }

        public BsonDocument GetDocument(TEntity entity)
        {
            var doc = new BsonDocument();
            foreach (var mapper in this.m_allProperties.Values.Where(p => !p.Descriptor.IsVersion))
            {
                mapper.PutValue(doc, entity);
            }

            if (this.m_version != null)
            {
                this.m_version.PutDefaultVersion(doc);
            }

            return doc;
        }

        public FieldsDocument GetFields(Expression selector)
        {
            var doc = new FieldsDocument();

            if (selector == null)
            {
                foreach (var mapper in this.m_allProperties.Values.Where(mp => !mp.IsReadOnly))
                {
                    mapper.PutField(doc, true);
                }

                foreach (var mapper in this.m_identities.Values)
                {
                    mapper.PutField(doc, true);
                }

                if (this.m_version != null)
                {
                    this.m_version.PutField(doc, true);
                }
            }
            else
            {
                var methodCall = (MethodCallExpression)selector;
                var include = methodCall.Method.Name == "PartialWith";

                var argsExpr = (NewArrayExpression)(methodCall).Arguments[1];
                foreach (UnaryExpression expr in argsExpr.Expressions)
                {
                    var lambdaExpr = (LambdaExpression)expr.Operand;
                    var propExpr = (MemberExpression)((lambdaExpr.Body is UnaryExpression) ?
                        ((UnaryExpression)lambdaExpr.Body).Operand : lambdaExpr.Body);
                    var propInfo = (PropertyInfo)propExpr.Member;

                    if (!include)
                    {
                        if (this.m_identities.ContainsKey(propInfo)) continue;
                        if (this.m_version != null && this.m_version.Descriptor.Property == propInfo) continue;
                    }

                    this.m_allProperties[propInfo].PutField(doc, include);
                }

                if (include)
                {
                    foreach (var mapper in this.m_identities.Values)
                    {
                        mapper.PutField(doc, true);
                    }

                    if (this.m_version != null)
                    {
                        this.m_version.PutField(doc, true);
                    }
                }
            }

            return doc;
        }

        public EntityState GetEntityState(TEntity entity)
        {
            var state = new EntityState();
            foreach (var mapper in this.m_allProperties.Values.Where(mp => !mp.IsReadOnly))
            {
                mapper.PutState(state, entity);
            }

            return state;
        }

        public QueryDocument GetIdentity(object entity)
        {
            var doc = new QueryDocument();

            foreach (var mapper in this.m_identities.Values)
            {
                mapper.PutValue(doc, entity);
            }

            if (this.m_version != null)
            {
                this.m_version.PutValue(doc, entity);
            }

            return doc;
        }

        public TEntity GetEntity(BsonDocument doc)
        {
            var entity = new TEntity();
            foreach (var mapper in this.m_allProperties.Values.Where(mp => !mp.IsReadOnly))
            {
                mapper.SetValue(entity, doc);
            }

            foreach (var mapper in this.m_identities.Values)
            {
                mapper.SetValue(entity, doc);
            }

            if (this.m_version != null)
            {
                this.m_version.SetValue(entity, doc);
            }

            return entity;
        }

        public UpdateDocument GetStateChanged(object entity, EntityState original, EntityState current)
        {
            var updateDoc = new UpdateDocument();
            var changedSet = new HashSet<PropertyInfo>();

            foreach (var mapper in this.m_allProperties.Values.Where(p => !p.IsReadOnly))
            {
                if (mapper.IsStateChanged(original, current))
                {
                    changedSet.Add(mapper.Descriptor.Property);
                    mapper.PutStateUpdate(updateDoc, original, current);
                }
            }

            foreach (var mapper in this.m_allProperties.Values.Where(p => p.IsReadOnly))
            {
                if (mapper.Descriptor.ChangeWithProperties.Any(p => changedSet.Contains(p)))
                {
                    mapper.PutValueUpdate(updateDoc, entity);
                }
            }

            if (this.m_version != null && updateDoc.ElementCount > 0)
            {
                this.m_version.PutNextVersion(updateDoc, entity);
            }

            return updateDoc;
        }

        public SortByDocument GetSortOrders(List<SortOrder> sortOrders)
        {
            var result = new SortByDocument();

            foreach (var order in sortOrders)
            {
                var propExpr = (MemberExpression)order.KeySelector;
                var propInfo = (PropertyInfo)propExpr.Member;
                this.m_allProperties[propInfo].PutSortOrder(result, order.Descending);
            }

            return result;
        }

        public UpdateDocument GetUpdates(Expression updateExpr)
        {
            var propUpdates = new UpdateCollector().Collect(updateExpr);

            var updateDoc = new UpdateDocument();
            foreach (var update in propUpdates)
            {
                var propertyMapper = this.m_allProperties[update.Property];
                update.Fill(propertyMapper, updateDoc);
            }

            if (this.m_version != null)
            {
                this.m_version.PutVersionUpdate(updateDoc);
            }

            return updateDoc;
        }

        public BsonDocument GetHints(List<QueryHint> hints)
        {
            var result = new BsonDocument();

            foreach (var h in hints)
            {
                var propExpr = (MemberExpression)h.KeySelector;
                var propInfo = (PropertyInfo)propExpr.Member;
                this.m_allProperties[propInfo].PutHint(result, h.Descending);
            }

            return result;
        }

        public FieldsDocument GetVersionField()
        { 
            var fieldsDoc = new FieldsDocument();
            this.m_version.PutField(fieldsDoc, true);
            return fieldsDoc;
        }


        internal void UpdateVersion(object entity, BsonDocument sourceDoc)
        {
            this.m_version.SetValue(entity, sourceDoc);
        }
    }
}
