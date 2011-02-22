using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyMongo
{
    public static class PredicateExtensions
    {
        public static bool In<T>(this T item, IEnumerable<T> container)
        {
            return container.Contains(item);
        }

        public static bool Matches(this string s, string expression, string options)
        {
            return Regex.IsMatch(s, expression);
        }

        public static bool Contains(this Enum container, Enum item)
        {
            throw new NotSupportedException("Cannot execute directly.");
        }
    }
}
