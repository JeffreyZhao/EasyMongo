using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo
{
    public interface IArray<T>
    {
        void Add(T item);
        void Remove(T item);
        int Length { get; }
    }
}
