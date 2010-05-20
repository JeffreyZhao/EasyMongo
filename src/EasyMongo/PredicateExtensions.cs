using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo
{
    public static class PredicateExtensions
    {
        public static bool ContainedIn<T>(this T item, IEnumerable<T> container)
        {
            return container.Contains(item);
        }
    }
}
