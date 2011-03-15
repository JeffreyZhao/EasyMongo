using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections;

namespace EasyMongo
{
    internal class Query<TEntity> : ICountableQueryableCollection<TEntity> where TEntity : class, new()
    {
        public static Query<TEntity> GetQuery(object queryable)
        {
            var collection = queryable as EntityCollection<TEntity>;
            if (collection != null)
            {
                return new Query<TEntity>(collection);
            }

            return (Query<TEntity>)queryable;
        }

        public Query(EntityCollection<TEntity> collection)
        {
            this.m_collection = collection;
        }

        private EntityCollection<TEntity> m_collection;
        private Expression m_predicate;
        private int m_skip;
        private int? m_limit;
        private Expression m_selector;

        private List<SortOrder> m_sortOrders = new List<SortOrder>();
        private List<QueryHint> m_hints = new List<QueryHint>();

        public Query<TEntity> WhereInternal(Expression<Func<TEntity, bool>> predicate)
        {
            if (this.m_predicate == null)
            {
                this.m_predicate = predicate.Body;
            }
            else
            {
                this.m_predicate = Expression.AndAlso(this.m_predicate, predicate.Body);
            }

            return this;
        }

        public Query<TEntity> SkipInternal(int n)
        {
            this.m_skip = n;
            return this;
        }

        public Query<TEntity> TakeInternal(int n)
        {
            this.m_limit = n;
            return this;
        }

        public Query<TEntity> OrderByInternal<TKey>(Expression<Func<TEntity, TKey>> keySelector, bool desc)
        {
            this.m_sortOrders.Clear();
            this.m_sortOrders.Add(new SortOrder(keySelector.Body, desc));
            return this;
        }

        public Query<TEntity> ThenByInternal<TKey>(Expression<Func<TEntity, TKey>> keySelector, bool desc)
        {
            this.m_sortOrders.Add(new SortOrder(keySelector.Body, desc));
            return this;
        }

        public Query<TEntity> SelectInternal(Expression<Func<TEntity, TEntity>> selector)
        {
            this.m_selector = selector.Body;
            return this;
        }

        public Query<TEntity> HintInternal<TKey>(Expression<Func<TEntity, TKey>> selector, bool desc)
        {
            this.m_hints.Add(new QueryHint(selector.Body, desc));

            return this;
        }

        public int Count()
        {
            return this.m_collection.Count(this.m_predicate);
        }

        public List<TEntity> Load()
        {
            return this.m_collection.Load(
                this.m_predicate,
                this.m_skip,
                this.m_limit,
                this.m_sortOrders,
                this.m_hints,
                this.m_selector);
        }

        public List<TResult> SelectToInternal<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return this.m_collection.LoadTo<TResult>(
                this.m_predicate,
                this.m_skip,
                this.m_limit,
                this.m_sortOrders,
                this.m_hints,
                selector.Body);
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return this.Load().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
