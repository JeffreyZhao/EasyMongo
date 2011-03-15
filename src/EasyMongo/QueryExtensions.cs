using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace EasyMongo
{
    public static class QueryExtensions
    {
        public static int Count<TEntity>(this ICountableCollection<TEntity> queryable) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).Count();
        }

        public static int Count<TEntity>(this ICountableCollection<TEntity> queryable, Expression<Func<TEntity, bool>> predicate) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).Where(predicate).Count();
        }

        public static IQueryableCollection<TEntity> Where<TEntity>(this IQueryableCollection<TEntity> queryable, Expression<Func<TEntity, bool>> predicate) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).WhereInternal(predicate);
        }

        public static ICountableQueryableCollection<TEntity> Where<TEntity>(this ICountableQueryableCollection<TEntity> queryable, Expression<Func<TEntity, bool>> predicate) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).WhereInternal(predicate);
        }

        public static IQueryableCollection<TEntity> Select<TEntity>(this IQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TEntity>> selector) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).SelectInternal(selector);
        }

        //public static IEnumerable<TProperty> Select<TEntity, TProperty>(this IQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TProperty>> selector) where TEntity : class, new()
        //{
        //    return Query<TEntity>.GetQuery(queryable).SelectInternal(selector);
        //}

        public static IQueryableCollection<TEntity> Hint<TEntity, TKey>(this IQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TKey>> selector, bool desc) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).HintInternal(selector, desc);
        }

        public static ICountableQueryableCollection<TEntity> Hint<TEntity, TKey>(this ICountableQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TKey>> selector, bool desc) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).HintInternal(selector, desc);
        }

        public static ICountableQueryableCollection<TEntity> Select<TEntity>(this ICountableQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TEntity>> selector) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).SelectInternal(selector);
        }

        public static List<TResult> SelectTo<TEntity, TResult>(this IQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TResult>> selector) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).SelectToInternal<TResult>(selector);
        }

        public static IQueryableCollection<TEntity> Skip<TEntity>(this IQueryableCollection<TEntity> queryable, int num) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).SkipInternal(num);
        }

        public static IQueryableCollection<TEntity> Take<TEntity>(this IQueryableCollection<TEntity> queryable, int num) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).TakeInternal(num);
        }

        public static IQueryableCollection<TEntity> OrderBy<TEntity, TKey>(this IQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TKey>> selector) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).OrderByInternal(selector, false);
        }

        public static IQueryableCollection<TEntity> OrderByDescending<TEntity, TKey>(this IQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TKey>> selector) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).OrderByInternal(selector, true);
        }

        public static IQueryableCollection<TEntity> OrderBy<TEntity, TKey>(this IQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TKey>> selector, bool desc) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).OrderByInternal(selector, desc);
        }

        public static ICountableQueryableCollection<TEntity> OrderBy<TEntity, TKey>(this ICountableQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TKey>> selector) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).OrderByInternal(selector, false);
        }

        public static ICountableQueryableCollection<TEntity> OrderByDescending<TEntity, TKey>(this ICountableQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TKey>> selector) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).OrderByInternal(selector, true);
        }

        public static ICountableQueryableCollection<TEntity> OrderBy<TEntity, TKey>(this ICountableQueryableCollection<TEntity> queryable, Expression<Func<TEntity, TKey>> selector, bool desc) where TEntity : class, new()
        {
            return Query<TEntity>.GetQuery(queryable).OrderByInternal(selector, desc);
        }
    }
}
