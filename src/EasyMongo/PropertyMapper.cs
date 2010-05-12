using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;

namespace EasyMongo
{
    internal class PropertyMapper
    {
        public PropertyMapper(IPropertyDescriptor descriptor)
        {
            this.Descriptor = descriptor;
        }
        
        public IPropertyDescriptor Descriptor { get; private set; }

        public bool IsReadOnly { get { return this.Descriptor.ChangeWithProperties.Count > 0; } }

        public void PutEqualPredicate(Document doc, object value)
        {
            var name = this.Descriptor.Name;
            if (doc.Contains(name))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "this document should not contain {0} field.", name));
            }

            doc.Append(name, value);
        }

        public void PutGreaterThanPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$gt", value);
        }

        private void PutInnerPredicate(Document doc, string op, object value)
        {
            var name = this.Descriptor.Name;
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

            innerDoc.Append(op, value);
        }

        public void PutGreaterThanOrEqualPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$gte", value);
        }

        public void PutLessThanPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$lt", value);
        }

        public void PutLessThanOrEqualPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$lte", value);
        }

        public void PutEntityValue(Document target, object sourceEntity)
        {
            object docValue;

            var property = this.Descriptor.Property;
            var type = property.PropertyType;
            var value = property.GetValue(sourceEntity, null);

            if (typeof(IList).IsAssignableFrom(type))
            {
                docValue = value == null ? null : ((IList)value).Cast<object>().ToArray();
            }
            else if (type.IsEnum)
            {
                if (type.IsDefined(typeof(FlagsAttribute), false))
                {
                    docValue = value.ToString().Split(new[] { ", " }, StringSplitOptions.None);
                }
                else
                {
                    docValue = value.ToString();
                }
            }
            else
            {
                docValue = value;
            }

            target.Append(this.Descriptor.Name, docValue);
        }

        /*public void SetEntityState(object targetEntity, Dictionary<PropertyInfo, object> sourceState)
        {
            var property = this.Descriptor.Property;
            var type = property.PropertyType;

            var value = sourceState[property];
            if (typeof(IList).IsAssignableFrom(type)) // is array
            {
                var list = (IList)Activator.CreateInstance(type);
                foreach (var item in (List<object>)value) list.Add(item);
                value = list;
            }

            this.Descriptor.Property.SetValue(targetEntity, value, null);
        }

        public void PutEntityState(Dictionary<PropertyInfo, object> targetState, Document sourceDoc)
        {
            var name = this.Descriptor.Name;

            object docValue;
            if (sourceDoc.Contains(name))
            {
                docValue = sourceDoc[name];
                if (docValue == MongoDBNull.Value) docValue = null;
            }
            else if (this.Descriptor.HasDefaultValue)
            {
                docValue = this.Descriptor.DefaultValue;
            }
            else
            {
                throw new ArgumentException("Missing the value of " + name);
            }

            var property = this.Descriptor.Property;
            var type = property.PropertyType;
            object value;

            if (typeof(IList).IsAssignableFrom(type)) // is array
            {
                if (docValue == null)
                {
                    value = null;
                }
                else
                {
                    value = (docValue is Document) ? // empty array
                        new List<object>() :
                        ((IEnumerable)docValue).Cast<object>().ToList();
                }
            }
            else if (type.IsEnum)
            {
                if (docValue == null)
                {
                    throw new ArgumentException("Enum value cannot be assigned to null for " + name);
                }

                if (type.IsDefined(typeof(FlagsAttribute), false))
                {
                    if (docValue is Document) // empty array;
                    {
                        value = Enum.Parse(type, "");
                    }
                    else
                    {
                        var array = ((IEnumerable)docValue).Cast<string>().ToArray();
                        value = Enum.Parse(type, String.Join(", ", array));
                    }
                }
                else
                {
                    value = Enum.Parse(type, docValue.ToString());
                }
            }
            else
            {
                value = docValue;
            }

            targetState.Add(this.Descriptor.Property, value);
        }*/

        public void PutField(Document doc)
        {
            doc.Append(this.Descriptor.Name, 1);
        }

        public void PutEntityState(Dictionary<PropertyInfo, object> targetState, object sourceEntity)
        {
            var name = this.Descriptor.Name;
            var property = this.Descriptor.Property;
            var type = property.PropertyType;

            object value = property.GetValue(sourceEntity, null);
            if (typeof(IList).IsAssignableFrom(type)) // is array
            {
                value = ((IList)value).Cast<object>().ToList();
            }

            targetState.Add(this.Descriptor.Property, value);
        }

        public void SetEntityValue(object targetEntity, Document sourceDoc)
        {
            var name = this.Descriptor.Name;

            object docValue;
            if (sourceDoc.Contains(name))
            {
                docValue = sourceDoc[name];
                if (docValue == MongoDBNull.Value) docValue = null;
            }
            else if (this.Descriptor.HasDefaultValue)
            {
                docValue = this.Descriptor.DefaultValue;
            }
            else
            {
                throw new ArgumentException("Missing the value of " + name);
            }

            var property = this.Descriptor.Property;
            var type = property.PropertyType;
            object value;

            if (typeof(IList).IsAssignableFrom(type)) // is array
            {
                if (docValue == null)
                {
                    value = null;
                }
                else
                {
                    var list = (IList)Activator.CreateInstance(type);
                    if (!(docValue is Document)) // not empty array
                    {
                        foreach (var item in ((IEnumerable)docValue)) list.Add(item);
                    }

                    value = list;
                }
            }
            else if (type.IsEnum)
            {
                if (docValue == null)
                {
                    throw new ArgumentException("Enum value cannot be assigned to null for " + name);
                }

                if (type.IsDefined(typeof(FlagsAttribute), false))
                {
                    if (docValue is Document) // empty array;
                    {
                        value = Enum.Parse(type, "");
                    }
                    else
                    {
                        var array = ((IEnumerable)docValue).Cast<string>().ToArray();
                        value = Enum.Parse(type, String.Join(", ", array));
                    }
                }
                else
                {
                    value = Enum.Parse(type, docValue.ToString());
                }
            }
            else
            {
                value = docValue;
            }

            property.SetValue(targetEntity, value, null);
        }
    }
}
