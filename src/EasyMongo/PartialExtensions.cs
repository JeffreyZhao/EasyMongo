using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace EasyMongo
{
    public static class PartialExtensions
    {
        public static T PartialWith<T>(this T entity, params Expression<Func<T, object>>[] fieldSelectors)
            where T : new()
        {
            throw new NotImplementedException();
        }

        public static T PartialWithout<T>(this T entity, params Expression<Func<T, object>>[] fieldSelectors)
            where T : new()
        {
            throw new NotImplementedException();
        }
    }
}
