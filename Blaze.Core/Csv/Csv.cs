using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;

namespace Blaze.Core
{
    public class Csv
    {
        private string _Path;

        private Dictionary<object, Collection> _Data;

        private const string _DefaultCollectionKey = "DefaultCollection";

        public Csv(string filePath)
        {
            _Path = filePath;
            _Data = new Dictionary<object, Collection>();
        }

        public void SaveObject(object entry, object entryId = null, object collectionId = null)
        {
            collectionId = collectionId ?? _DefaultCollectionKey;
            Collection collection;
            if (!_Data.TryGetValue(collectionId, out collection))
            {
                collection = new Collection(collectionId);
                _Data.Add(collectionId, collection);
            }

            var de = new DataEntry(entry, collection, entryId);
            collection.Add(de.Key, de);
        }

        public bool Save(string filePath)
        {
            _Path = filePath;
            return Save();
        }

        public bool Save()
        {
            var serializedCsv = new List<StringBuilder>();
            
            int maxCollectionCount = _Data.Values.Max(c => c.Count);
            foreach (KeyValuePair<object, Collection> data in _Data)
            {
                int lineNumber = 0;
                Collection col = data.Value;
                col.Reset();
                foreach (KeyValuePair<object, DataEntry> dataEntry in col)
                {
                    DataEntry dataEntryObj = dataEntry.Value;
                    dataEntryObj.LoadDataFromObject();
                    col.IndexObject(dataEntryObj);
                }
                col.SerializeCollectionInfo(serializedCsv, lineNumber++);
                col.SerializeHeader(serializedCsv, lineNumber++);

                int index = 0;
                foreach (KeyValuePair<object, DataEntry> dataEntry in col)
                    col.SerializeEntry(serializedCsv, dataEntry.Value, lineNumber + index++);

                for (; index < maxCollectionCount; ++index)
                    col.SerializeEmptyEntry(serializedCsv, lineNumber + index);
            }

            System.IO.File.WriteAllLines(_Path, serializedCsv.Select(sb => sb.ToString()));

            return true;
        }

        public static StringBuilder GetLine(List<StringBuilder> serialized, int number)
        {
            while (serialized.Count <= number)
            {
                serialized.Add(new StringBuilder());
            }
            return serialized[number];
        }

        public static void UpdateLine(List<StringBuilder> serialized, int number, string val)
        {
            var line = GetLine(serialized, number);
            line.Append(val);
        }

        internal class DataEntry : Dictionary<string, object>
        {
            PropertyInfo[] _PropertyInfos;
            object OriginalObject { get; }
            
            internal Collection ParentCollection { get; }
            internal object Key { get; }
            internal int PropertyCount { get; }

            internal DataEntry(object original, Collection collection, object key = null)
            {
                OriginalObject = original;
                ParentCollection = collection;
                Type type = original.GetType();
                _PropertyInfos = type.GetProperties();

                if (key == null)
                { 
                    PropertyInfo entryKeyProperty = _PropertyInfos
                        .FirstOrDefault(pi => pi.GetCustomAttributes(typeof(EntryKeyAttribute), true).Any());

                    if (entryKeyProperty != null)
                        key = entryKeyProperty.GetValue(original);
                    else
                        key = ParentCollection.GetKey();
                }
                Key = key;
            }

            internal void LoadDataFromObject()
            {
                foreach (var prop in _PropertyInfos)
                {
                    if (!prop.CanRead)
                        continue;

                    object value = prop.GetValue(OriginalObject);
                    this[prop.Name] = value;
                }
            }

            internal void LoadObjectFromData()
            {
                
            }
        }

        internal class Collection : Dictionary<object, DataEntry>
        {
            /// <summary>
            /// The size of the property in number of columns
            /// </summary>
            private Dictionary<string, int> _PropertySizeMap;
            private int _CurrentIndex;
            private readonly object _Key;
            private int _CollectionWidth;
            internal Collection(object key)
            {
                _Key = key;
                _CurrentIndex = 0;
                _PropertySizeMap = new Dictionary<string, int>();
            }

            internal void IndexObject(DataEntry entry)
            {
                foreach (KeyValuePair<string, object> kvp in entry)
                    AddOrUpdatePropertySize(kvp.Key, kvp.Value);
            }

            internal void Reset()
            {
                _PropertySizeMap.Clear();
                _CollectionWidth = 0;
            }

            internal int GetKey()
            {
                return _CurrentIndex++;
            }

            internal virtual void SerializeCollectionInfo(List<StringBuilder> serializedCsv, int lineNumber)
            {
                StringBuilder sbuilder = GetLine(serializedCsv, lineNumber);
                bool first = true;
                sbuilder.Append($"{_Key},");
                foreach (KeyValuePair<string, int> kvp in _PropertySizeMap)
                {
                    if(first)
                        sbuilder.Append(',', kvp.Value - 1);
                    else
                        sbuilder.Append(',', kvp.Value);
                    first = false;
                }
                
            }

            internal void SerializeHeader(List<StringBuilder> serializedCsv, int lineNumber)
            {
                StringBuilder sbuilder = GetLine(serializedCsv, lineNumber);
                foreach (KeyValuePair<string, int> kvp in _PropertySizeMap)
                {
                    sbuilder.Append($"{kvp.Key},");
                    sbuilder.Append(',', kvp.Value - 1);
                    _CollectionWidth += kvp.Value;
                }
            }

            internal void SerializeEntry(List<StringBuilder> serializedCsv, DataEntry dataEntry, int lineNumber)
            {
                StringBuilder sbuilder = GetLine(serializedCsv, lineNumber);
                foreach (KeyValuePair<string, int> kvp in _PropertySizeMap)
                {
                    object property;

                    if (dataEntry.TryGetValue(kvp.Key, out property))
                    {
                        ICollection propCol = property as ICollection;
                        if (propCol == null)
                        {
                            SerializeProperty(sbuilder, property);
                            sbuilder.Append(',', kvp.Value - 1);
                        }
                        else
                        {
                            sbuilder.Append($"{propCol.Count},");
                            foreach (var obj in propCol)
                                SerializeProperty(sbuilder, obj);
                            //pad the rest (-1 for the size value)
                            sbuilder.Append(',', kvp.Value - propCol.Count - 1);
                        }
                    }
                    else
                    {
                        sbuilder.Append(',', kvp.Value);
                    }
                }
            }

            private static void SerializeProperty(StringBuilder builder, object propertyValue)
            {
                //typeof(List<>).IsAssignableFrom(typeof(ICollection))
                //false
                //typeof(ICollection).IsAssignableFrom(typeof(List<>))
                //true
                if (propertyValue == null)
                {
                    builder.Append($"null,");
                    return;
                }

                Type type = propertyValue.GetType();
                if (type == typeof(string) || type == typeof(char))
                    builder.Append($"\"{propertyValue}\",");
                else if (type.IsPrimitive)
                    builder.Append($"{propertyValue},");
                else // if(type.IsValueType && !type.IsPrimitive || type.IsClass)
                {
                    string serializedObj = Newtonsoft.Json
                        .JsonConvert
                        .SerializeObject(propertyValue)
                        .Replace("\"", "\"\"");
                    builder.Append($"\"{serializedObj}\",");
                }
            }

            internal void SerializeEmptyEntry(List<StringBuilder> serializedCsv, int lineNumber)
            {
                StringBuilder sbuilder = GetLine(serializedCsv, lineNumber);
                sbuilder.Append(',', _CollectionWidth);
            }

            /// <summary>
            /// Keep count of each property size to allow padding
            /// </summary>
            private void AddOrUpdatePropertySize(string propertyName, object propertyValue)
            {
                int propertySize = 0;
                if (!_PropertySizeMap.TryGetValue(propertyName, out propertySize))
                    _PropertySizeMap.Add(propertyName, 1);

                var propCol = propertyValue as ICollection;
                int currentPropertySize = propCol?.Count + 1 ?? 1;
                _PropertySizeMap[propertyName] = Math.Max(propertySize, currentPropertySize);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EntryKeyAttribute : Attribute
    {
        object Key { get; }
    }
}
