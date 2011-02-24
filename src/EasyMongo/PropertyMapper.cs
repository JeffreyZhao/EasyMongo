using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;
using EasyMongo.Types;
using EasyMongo.Reflection;
using MongoDB.Bson;

namespace EasyMongo
{
    internal class PropertyMapper : IPropertyUpdateOperator, IPropertyPredicateOperator
    {
        public PropertyMapper(IPropertyDescriptor descriptor, bool isDefaultIdentity)
        {
            this.Descriptor = descriptor;
            this.IsDefaultIdentity = isDefaultIdentity;

            this.TypeProcessor = this.Descriptor.GetTypeProcessor();
            if (this.TypeProcessor == null)
            {
                var type = descriptor.Property.PropertyType;
                if (type.IsEnum)
                {
                    this.TypeProcessor = new EnumProcessor(descriptor.Property);
                }
                else if (typeof(IList).IsAssignableFrom(type))
                {
                    this.TypeProcessor = new ArrayProcessor(descriptor.Property);
                }
                else
                {
                    this.TypeProcessor = new BasicProcessor();
                }
            }

            this.Accessor = new PropertyAccessor(descriptor.Property);
        }

        public ITypeProcessor TypeProcessor { get; private set; }
        
        public IPropertyDescriptor Descriptor { get; private set; }

        public PropertyAccessor Accessor { get; private set; }

        public bool IsDefaultIdentity { get; private set; }

        public string NameInDatabase
        {
            get
            {
                if (this.IsDefaultIdentity) return "_id";

                return this.Descriptor.Name;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                var descriptor = this.Descriptor;
                return descriptor.IsIdentity || descriptor.IsVersion || descriptor.ChangeWithProperties.Count > 0;
            }
        }

        public void PutValue(BsonDocument target, object sourceEntity)
        {
            var value = this.Accessor.GetValue(sourceEntity);
            target.Add(this.NameInDatabase, this.TypeProcessor.ToBsonValue(value));
        }

        public void PutField(BsonDocument doc, bool include)
        {
            var name = this.NameInDatabase;
            if (!doc.Contains(name))
            {
                doc.Add(name, include ? 1 : 0);
            }
        }

        public void PutState(Dictionary<IPropertyDescriptor, object> targetState, object sourceEntity)
        {
            var value = this.Accessor.GetValue(sourceEntity);
            var stateValue = this.TypeProcessor.ToStateValue(value);
            targetState.Add(this.Descriptor, stateValue);
        }

        public void SetValue(object targetEntity, BsonDocument sourceDoc)
        {
            var name = this.NameInDatabase;

            object value;
            if (sourceDoc.Contains(name))
            {
                var docValue = sourceDoc[name];
                value = this.TypeProcessor.FromBsonValue(docValue);
            }
            else if (this.Descriptor.HasDefaultValue)
            {
                value = this.Descriptor.GetDefaultValue();
            }
            else
            {
                throw new ArgumentException("Missing the value of " + name);
            }

            this.Accessor.SetValue(targetEntity, value);
        }

        public bool IsStateChanged(
            Dictionary<IPropertyDescriptor, object> originalState,
            Dictionary<IPropertyDescriptor, object> currentState)
        {
            var original = originalState[this.Descriptor];
            var current = currentState[this.Descriptor];

            return this.TypeProcessor.IsStateChanged(original, current);
        }

        public void PutValueUpdate(BsonDocument targetDoc, object sourceEntity)
        {
            var value = this.Accessor.GetValue(sourceEntity);
            ((IPropertyUpdateOperator)this).PutConstantUpdate(targetDoc, value);
        }

        public void PutStateUpdate(
            BsonDocument targetDoc,
            Dictionary<IPropertyDescriptor, object> originalState,
            Dictionary<IPropertyDescriptor, object> currentState)
        {
            var original = originalState[this.Descriptor];
            var current = currentState[this.Descriptor];

            var arrayProcessor = this.TypeProcessor as IArrayProcessor;
            if (arrayProcessor != null)
            {
                var itemsToPush = arrayProcessor.GetPushingValues(original, current);
                if (itemsToPush != null)
                {
                    this.AppendOperation(targetDoc, "$pushAll", itemsToPush);
                    return;
                }
            }

            var value = this.TypeProcessor.FromStateValue(current);
            ((IPropertyUpdateOperator)this).PutConstantUpdate(targetDoc, value);
        }

        private void AppendOperation(BsonDocument doc, string op, BsonValue bsonValue)
        {
            BsonDocument innerDoc;
            if (doc.Contains(op))
            {
                innerDoc = (BsonDocument)doc[op];
            }
            else
            {
                innerDoc = new BsonDocument();
                doc.Add(op, innerDoc);
            }

            innerDoc.Add(this.NameInDatabase, bsonValue);
        }

        public void PutSortOrder(BsonDocument doc, bool descending)
        {
            doc.Add(this.NameInDatabase, descending ? -1 : 1);
        }

        public void PutHint(BsonDocument doc, bool desc)
        {
            doc.Add(this.NameInDatabase, desc ? -1 : 1);
        }

        public void PutDefaultVersion(BsonDocument doc)
        {
            BsonValue value;

            var type = this.Descriptor.Property.PropertyType;
            if (type == typeof(int))
            { 
                value = new BsonInt32(0);
            }
            else if (type == typeof(long))
            {
                value = new BsonInt64(0);
            }
            else if (type == typeof(DateTime))
            {
                value = new BsonDateTime(DateTime.UtcNow);
            }
            else
            {
                throw new NotSupportedException(String.Format("Don't support type {0} as version.", type));
            }

            doc.Add(this.NameInDatabase, value);
        }

        public void PutNextVersion(UpdateDocument updateDoc, object entity)
        {
            var version = this.Accessor.GetValue(entity);
            object nextVersion;

            var type = this.Descriptor.Property.PropertyType;
            if (type == typeof(int))
            {
                nextVersion = (int)version + 1;
            }
            else if (type == typeof(long))
            {
                nextVersion = (long)version + 1;
            }
            else if (type == typeof(DateTime))
            {
                nextVersion = DateTime.UtcNow;
            }
            else
            {
                throw new NotSupportedException(String.Format("Don't support type {0} as version.", type));
            }

            ((IPropertyUpdateOperator)this).PutConstantUpdate(updateDoc, nextVersion);
        }

        public void PutVersionUpdate(BsonDocument updateDoc)
        {
            var optr = (IPropertyUpdateOperator)this;

            var type = this.Descriptor.Property.PropertyType;
            if (type == typeof(int))
            {
                optr.PutAddUpdate(updateDoc, 1);
            }
            else if (type == typeof(long))
            {
                optr.PutAddUpdate(updateDoc, 1L);
            }
            else if (type == typeof(DateTime))
            {
                optr.PutAddUpdate(updateDoc, DateTime.UtcNow);
            }
            else
            {
                throw new NotSupportedException(String.Format("Don't support type {0} as version.", type));
            }
        }

        #region IPropertyPredicateOperator members

        void IPropertyPredicateOperator.PutEqualPredicate(QueryDocument doc, object value)
        {
            var name = this.NameInDatabase;
            if (doc.Contains(name))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "this document should not contain {0} field.", name));
            }

            doc.Add(name, this.TypeProcessor.ToBsonValue(value));
        }

        void IPropertyPredicateOperator.PutNotEqualPredicate(QueryDocument doc, object value)
        {
            this.PutInnerPredicate(doc, "$ne", value);
        }

        private void PutInnerPredicate(QueryDocument doc, string op, object value)
        {
            var name = this.NameInDatabase;
            BsonDocument innerDoc;

            if (doc.Contains(name))
            {
                innerDoc = doc[name] as BsonDocument;
                if (innerDoc == null)
                {
                    throw new InvalidOperationException("Should have nothing or BsonDocument object");
                }
            }
            else
            {
                innerDoc = new BsonDocument();
                doc.Add(name, innerDoc);
            }

            innerDoc.Add(op, this.TypeProcessor.ToBsonValue(value));
        }

        void IPropertyPredicateOperator.PutGreaterThanPredicate(QueryDocument doc, object value)
        {
            this.PutInnerPredicate(doc, "$gt", value);
        }

        void IPropertyPredicateOperator.PutGreaterThanOrEqualPredicate(QueryDocument doc, object value)
        {
            this.PutInnerPredicate(doc, "$gte", value);
        }

        void IPropertyPredicateOperator.PutLessThanPredicate(QueryDocument doc, object value)
        {
            this.PutInnerPredicate(doc, "$lt", value);
        }

        void IPropertyPredicateOperator.PutLessThanOrEqualPredicate(QueryDocument doc, object value)
        {
            this.PutInnerPredicate(doc, "$lte", value);
        }

        void IPropertyPredicateOperator.PutContainsPredicate(QueryDocument doc, object value)
        {
            var arrayProcessor = this.TypeProcessor as IArrayProcessor;
            if (arrayProcessor == null)
            {
                throw new NotSupportedException("Only IArrayProcessor instance support Contains predicate.");
            }

            var containingValue = arrayProcessor.GetContainingValue(value);
            doc.Add(this.NameInDatabase, containingValue);
        }

        void IPropertyPredicateOperator.PutInPredicate(QueryDocument doc, IEnumerable<object> collection)
        {
            var name = this.NameInDatabase;
            if (doc.Contains(name))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "this document should not contain {0} field.", name));
            }

            var array = new BsonArray();
            foreach (var item in collection)
            {
                var value = this.TypeProcessor.ToBsonValue(item);
                array.Add(value);
            }

            doc.Add(name, new BsonDocument().Add("$in", array));
        }

        void IPropertyPredicateOperator.PutRegexMatchPredicate(QueryDocument doc, string expression, string options)
        {
            var name = this.NameInDatabase;
            if (doc.Contains(name))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "this document should not contain {0} field.", name));
            }

            doc.Add(name, new BsonRegularExpression(expression, options));
        }

        #endregion

        #region IPropertyUpdateOperator members

        void IPropertyUpdateOperator.PutConstantUpdate(BsonDocument doc, object value)
        {
            this.AppendOperation(doc, "$set", this.TypeProcessor.ToBsonValue(value));
        }

        void IPropertyUpdateOperator.PutAddUpdate(BsonDocument doc, object value)
        {
            this.AppendOperation(doc, "$inc", this.TypeProcessor.ToBsonValue(value));
        }

        //private object CreateArray(IEnumerable<object> items)
        //{
        //    var array = (IList)Activator.CreateInstance(this.Descriptor.Property.PropertyType);
        //    foreach (var e in items) array.Add(e);
        //    return array;
        //}

        private BsonArray CreateArray(IEnumerable<object> items)
        {
            var arrayProcessor = this.TypeProcessor as ArrayProcessor;
            if (arrayProcessor == null) throw new NotSupportedException();
            return arrayProcessor.GetValues(items);
        }

        void IPropertyUpdateOperator.PutPushUpdate(BsonDocument doc, IEnumerable<object> items)
        {
            this.AppendOperation(doc, "$push", this.CreateArray(items));
        }

        void IPropertyUpdateOperator.PutAddToSetUpdate(BsonDocument doc, IEnumerable<object> items)
        {
            this.AppendOperation(doc, "$addToSet", new BsonDocument().Add("$each", this.CreateArray(items)));
        }

        #endregion

    }
}
