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

        public string DatabaseName
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
                return descriptor.IsIdentity || descriptor.ChangeWithProperties.Count > 0;
            }
        }

        public void PutValue(Document target, object sourceEntity)
        {
            var value = this.Accessor.GetValue(sourceEntity);
            target.Append(this.DatabaseName, this.TypeProcessor.ToDocumentValue(value));
        }

        public void PutField(Document doc, bool include)
        {
            var name = this.DatabaseName;
            if (!doc.Contains(name))
            {
                doc.Append(name, include ? 1 : 0);
            }
        }

        public void PutState(Dictionary<IPropertyDescriptor, object> targetState, object sourceEntity)
        {
            var value = this.Accessor.GetValue(sourceEntity);
            var stateValue = this.TypeProcessor.ToStateValue(value);
            targetState.Add(this.Descriptor, stateValue);
        }

        public void SetValue(object targetEntity, Document sourceDoc)
        {
            var name = this.DatabaseName;

            object value;
            if (sourceDoc.Contains(name))
            {
                var docValue = sourceDoc[name];
                if (docValue == MongoDBNull.Value) docValue = null;
                value = this.TypeProcessor.FromDocumentValue(docValue);
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

        public void PutValueUpdate(Document targetDoc, object sourceEntity)
        {
            var value = this.Accessor.GetValue(sourceEntity);
            ((IPropertyUpdateOperator)this).PutConstantUpdate(targetDoc, value);
        }

        public void PutStateUpdate(
            Document targetDoc,
            Dictionary<IPropertyDescriptor, object> originalState,
            Dictionary<IPropertyDescriptor, object> currentState)
        {
            var original = originalState[this.Descriptor];
            var current = currentState[this.Descriptor];

            var arrayProcessor = this.TypeProcessor as IArrayProcessor;
            if (arrayProcessor != null)
            {
                var itemsToPush = arrayProcessor.GetItemsToPush(original, current);
                if (itemsToPush != null)
                {
                    this.AppendOperation(targetDoc, "$pushAll", itemsToPush);
                    return;
                }
            }

            var value = this.TypeProcessor.FromStateValue(current);
            ((IPropertyUpdateOperator)this).PutConstantUpdate(targetDoc, value);
        }

        private void AppendOperation(Document doc, string op, object docValue)
        {
            Document innerDoc;
            if (doc.Contains(op))
            {
                innerDoc = (Document)doc[op];
            }
            else
            {
                innerDoc = new Document();
                doc.Append(op, innerDoc);
            }

            innerDoc.Append(this.DatabaseName, docValue);
        }

        public void PutSortOrder(Document doc, bool descending)
        {
            doc.Append(this.DatabaseName, descending ? -1 : 1);
        }

        #region IPropertyPredicateOperator members

        void IPropertyPredicateOperator.PutEqualPredicate(Document doc, object value)
        {
            var name = this.DatabaseName;
            if (doc.Contains(name))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "this document should not contain {0} field.", name));
            }

            doc.Append(name, this.TypeProcessor.ToDocumentValue(value));
        }

        void IPropertyPredicateOperator.PutNotEqualPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$ne", value);
        }

        private void PutInnerPredicate(Document doc, string op, object value)
        {
            var name = this.DatabaseName;
            Document innerDoc;

            if (doc.Contains(name))
            {
                innerDoc = doc[name] as Document;
                if (innerDoc == null)
                {
                    throw new InvalidOperationException("Should have nothing or Document object");
                }
            }
            else
            {
                innerDoc = new Document();
                doc.Append(name, innerDoc);
            }

            innerDoc.Append(op, this.TypeProcessor.ToDocumentValue(value));
        }

        void IPropertyPredicateOperator.PutGreaterThanPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$gt", value);
        }

        void IPropertyPredicateOperator.PutGreaterThanOrEqualPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$gte", value);
        }

        void IPropertyPredicateOperator.PutLessThanPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$lt", value);
        }

        void IPropertyPredicateOperator.PutLessThanOrEqualPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$lte", value);
        }

        void IPropertyPredicateOperator.PutContainsPredicate(Document doc, object value)
        {
            var arrayProcessor = this.TypeProcessor as ArrayProcessor;
            if (arrayProcessor != null) 
            {
                value = arrayProcessor.Create(value);
            }

            var array = (Array)this.TypeProcessor.ToDocumentValue(value);
            if (array.Length != 1)
            {
                throw new NotSupportedException("Noly support one single element in Constains method");
            }

            doc.Append(this.DatabaseName, array.GetValue(0));
        }

        void IPropertyPredicateOperator.PutContainedInPredicate(Document doc, IEnumerable<object> collection)
        {
            var name = this.DatabaseName;
            if (doc.Contains(name))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "this document should not contain {0} field.", name));
            }

            var array = collection.Select(this.TypeProcessor.ToDocumentValue).ToArray();
            doc.Append(name, new Document().Append("$in", array));
        }

        void IPropertyPredicateOperator.PutRegexMatchPredicate(Document doc, string expression, string options)
        {
            var name = this.DatabaseName;
            if (doc.Contains(name))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "this document should not contain {0} field.", name));
            }

            doc.Append(name, new MongoRegex(expression, options));
        }

        #endregion

        #region IPropertyUpdateOperator members

        void IPropertyUpdateOperator.PutConstantUpdate(Document doc, object value)
        {
            this.AppendOperation(doc, "$set", this.TypeProcessor.ToDocumentValue(value));
        }

        void IPropertyUpdateOperator.PutAddUpdate(Document doc, object value)
        {
            this.AppendOperation(doc, "$inc", this.TypeProcessor.ToDocumentValue(value));
        }

        void IPropertyUpdateOperator.PutSubtractUpdate(Document doc, object value)
        {
            ((IPropertyUpdateOperator)this).PutAddUpdate(doc, -(int)value);
        }

        private object CreateArray(IEnumerable<object> items)
        {
            var array = (IList)Activator.CreateInstance(this.Descriptor.Property.PropertyType);
            foreach (var e in items) array.Add(e);
            return array;
        }

        void IPropertyUpdateOperator.PutPushUpdate(Document doc, IEnumerable<object> items)
        {
            var array = this.CreateArray(items);
            this.AppendOperation(doc, "$push", this.TypeProcessor.ToDocumentValue(array));
        }

        void IPropertyUpdateOperator.PutAddToSetUpdate(Document doc, IEnumerable<object> items)
        {
            var array = this.CreateArray(items);
            this.AppendOperation(doc, "$addToSet", new Document().Append("$each", this.TypeProcessor.ToDocumentValue(items)));
        }


        #endregion
    }
}
