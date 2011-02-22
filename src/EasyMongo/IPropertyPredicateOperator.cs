using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EasyMongo
{
    internal interface IPropertyPredicateOperator
    {
        void PutEqualPredicate(QueryDocument doc, object value);
        void PutGreaterThanPredicate(QueryDocument doc, object value);
        void PutGreaterThanOrEqualPredicate(QueryDocument doc, object value);
        void PutLessThanPredicate(QueryDocument doc, object value);
        void PutLessThanOrEqualPredicate(QueryDocument doc, object value);
        void PutNotEqualPredicate(QueryDocument doc, object value);
        void PutContainsPredicate(QueryDocument doc, object value);
        void PutInPredicate(QueryDocument doc, IEnumerable<object> collection);
        void PutRegexMatchPredicate(QueryDocument doc, string pattern, string options);
    }
}
