using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo
{
    public interface ICountableCollection<T> : IEnumerable<T> { }

    public interface IQueryableCollection<T> : IEnumerable<T> { }

    public interface ICountableQueryableCollection<T> : IQueryableCollection<T>, ICountableCollection<T> { }
}
