using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace EasyMongo
{
    public class ChangeConflictException : Exception
    { 

    }

    public class ChangeConflictException<TEntity> : ChangeConflictException
    {
        public ChangeConflictException(IList<TEntity> conflicts)
        {
            this.Conflicts = new ReadOnlyCollection<TEntity>(conflicts);
        }

        public ReadOnlyCollection<TEntity> Conflicts { get; private set; }
    }
}
