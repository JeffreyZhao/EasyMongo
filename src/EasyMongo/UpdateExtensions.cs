using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo
{
    public static class UpdateExtensions
    {
        public static TArray Push<TArray, T>(this TArray array, params T[] itemsToAdd)
            where TArray : IList<T>
        {
            foreach (var item in itemsToAdd)
            {
                array.Add(item);
            }

            return array;
        }
    }
}
