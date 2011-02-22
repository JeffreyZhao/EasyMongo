using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace EasyMongo
{
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
}
