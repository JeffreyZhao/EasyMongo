using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace EasyMongo
{
    internal class SortOrder
    { 
        public SortOrder(Expression keySelector, bool desc)
        {
            this.KeySelector = keySelector;
            this.Descending = desc;
        }

        public Expression KeySelector { get; set; }

        public bool Descending { get; set; }
    }

    internal class QueryHint
    {
        public QueryHint(Expression keySelector, bool desc)
        {
            this.KeySelector = keySelector;
            this.Descending = desc;
        }

        public Expression KeySelector { get; set; }

        public bool Descending { get; set; }
    }

    public class MongoQuery<T> where T : class
    {
        internal MongoQuery(DataContext db)
        {
            this.m_db = db;
        }

        private DataContext m_db;
        private Expression m_predicate;
        private int m_skip;
        private int? m_limit;
        private Expression m_selector;

        private List<SortOrder> m_sortOrders = new List<SortOrder>();
        private List<QueryHint> m_hints = new List<QueryHint>();

        public MongoQuery<T> Where(Expression<Func<T, bool>> predicate)
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

        public MongoQuery<T> Skip(int n)
        {
            this.m_skip = n;
            return this;
        }

        public MongoQuery<T> Take(int n)
        {
            this.m_limit = n;
            return this;
        }

        public MongoQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return this.OrderBy(keySelector, false);
        }

        public MongoQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return this.OrderBy(keySelector, true);
        }

        public MongoQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool desc)
        {
            this.m_sortOrders.Add(new SortOrder(keySelector.Body, desc));
            return this;
        }

        public MongoQuery<T> Select(Expression<Func<T, T>> selector)
        {
            this.m_selector = selector.Body;
            return this;
        }

        public MongoQuery<T> Hint<TKey>(Expression<Func<T, TKey>> selector, bool desc)
        { 
            this.m_hints.Add(new QueryHint(selector.Body, desc));

            return this;
        }

        public int Count()
        {
            return this.m_db.Count<T>(this.m_predicate);
        }

        public List<T> Load()
        {
            return this.m_db.List<T>(
                this.m_predicate,
                this.m_skip,
                this.m_limit,
                this.m_sortOrders,
                this.m_hints,
                this.m_selector);
        }
    }
}
