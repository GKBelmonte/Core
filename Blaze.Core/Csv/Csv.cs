using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using Blaze.Core.Collections;
using System.Diagnostics;
using System.Dynamic;

namespace Blaze.Core
{
    public enum DataSourceResult
    {
        Ok,
        Added,
        Updated,
        Modified,
        Failed,
        Deleted,
        CollectionNotFound,
        ObjectNotFound,
        ObjectWrongType
    }

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

        public DataSourceResult SaveObject(object entry, object entryId = null, object collectionId = null)
        {
            collectionId = collectionId ?? _DefaultCollectionKey;
            Collection collection;
            if (!_Data.TryGetValue(collectionId, out collection))
            {
                collection = new Collection(collectionId);
                _Data.Add(collectionId, collection);
            }
            DataEntry de = new DataEntry(entry, collection, entryId);

            //If the entry object already has a DE associated with it
            // Change it instead, return updated.
            // If the key has a DE associated with it, change it and
            // return modified
            //if (entryId != null)
            //{ 
            //    if (collection.TryGetValue(entryId, out de))
            //    {
                    
            //    }
            //}

            collection.Add(de.Key, de);
            return DataSourceResult.Added;
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
                    dataEntry.Value.SerializeEntry(serializedCsv, lineNumber + index++);

                for (; index < maxCollectionCount; ++index)
                    col.SerializeEmptyEntry(serializedCsv, lineNumber + index);
            }

            //Remove extra ',' which implies a final empty entry
            foreach (StringBuilder sb in serializedCsv)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            System.IO.File.WriteAllLines(_Path, serializedCsv.Select(sb => sb.ToString()));

            return true;
        }

        internal static StringBuilder GetLine(List<StringBuilder> serialized, int number)
        {
            while (serialized.Count <= number)
            {
                serialized.Add(new StringBuilder());
            }
            return serialized[number];
        }

        internal static void UpdateLine(List<StringBuilder> serialized, int number, string val)
        {
            var line = GetLine(serialized, number);
            line.Append(val);
        }

        public bool Load()
        {
            if (!System.IO.File.Exists(_Path))
                throw new System.IO.FileNotFoundException($"File not found: {_Path}", _Path);

            _Data.Clear();

            string[] lines = System.IO.File.ReadAllLines(_Path);

            DeserializeCollectionInfo(lines);

            List<string> collectionHeaders = CsvSplitLine(lines[1]);
            foreach (var kvp in _Data)
            {
                Collection col = kvp.Value;
                col.DeserializeHeader(collectionHeaders);
            }

            for (int i = 2; i < lines.Length; ++i)
            {
                List<string> row = CsvSplitLine(lines[i]);
                foreach (var kvp in _Data)
                {
                    Collection col = kvp.Value;
                    var entry = new DataEntry(col);
                    entry.DeserializeEntry(row);
                    //entry might be empty for a given collection
                    if (entry.Any())
                        col.Add(entry.Key, entry);
                }
            }

            return true;
        }

        private void DeserializeCollectionInfo(string[] lines)
        {
            List<string> collectionInfos = CsvSplitLine(lines[0]);
            int numberOfCols = collectionInfos.Count;
            int firstIndexOfCol = 0;
            for (int colIndex = 1; colIndex <= numberOfCols; ++colIndex)
            {
                string key = colIndex < numberOfCols
                    ? collectionInfos[colIndex]
                    : string.Empty;
                if (key == null)
                    continue;
                //end of collection, start of new one
                string colKey = collectionInfos[firstIndexOfCol];
                var col = new Collection(colKey, colIndex - firstIndexOfCol, firstIndexOfCol);
                _Data.Add(colKey, col);
                firstIndexOfCol = colIndex;
            }
        }

        internal static List<string> CsvSplitLine(string line)
        {
            var split = new List<string>();
            for (int i = 0; i < line.Length; ++i)
            {
                bool isString = line[i] == '"';
                if (isString)
                    i++;
                int quoteCount = 0;
                int j;
                for (j = i; j < line.Length; ++j)
                {
                    bool isQuote = line[j] == '"';
                    if (isQuote)
                        quoteCount++;

                    if (isString)
                    {
                        if (isQuote)
                        {
                            if (j + 1 < line.Length
                                && line[j + 1] == ','
                                //Need an odd number of quotes, if its the final quote
                                && quoteCount % 2 == 1)
                                break;
                            else if (j + 1 == line.Length)
                                break;
                            continue;
                        }
                        continue;
                    }
                    else if (!isString
                        && line[j] == ',')
                        break;
                }

                if (i == j)
                {
                    //Null means an empty entry
                    if (isString)
                        split.Add(string.Empty);
                    else
                        split.Add(null); 
                }
                else
                {
                    string splitLine = line.Substring(i, j - i);
                    splitLine = splitLine.Replace("\"\"", "\"");
                    split.Add(splitLine);
                }

                if (isString)
                    j++;

                i = j;
            }

            if (line.Last() == ',')
                split.Add(null);

            return split;
        }

        public T GetObject<T>(object key, object collectionKey = null)
        {
            TryGetObject<T>(key, out T res, collectionKey);
            return res;
        }

        public object GetObject(Type type, object key, object collectionKey = null)
        {
            MethodInfo genericMethod = GetType().GetMethod(nameof(TryGetObject));
            MethodInfo genericMethodFilled = genericMethod.MakeGenericMethod(type);
            var parameters = new object[] { key, null, collectionKey };
            var dsr = genericMethodFilled.Invoke(this, parameters);
            return parameters[1];
        }

        public DataSourceResult TryGetObject<T>(object key, out T obj, object collectionKey = null)
        {
            obj = default(T);
            DataSourceResult res = TryGetDataEntry(key, out DataEntry de, collectionKey);
            if (res != DataSourceResult.Ok)
                return res;

            //is this true? how do I handle null entries? should I disallow them?
            Trace.Assert(de != null, "DataEntry should not be null if DataResult is ok");

            if (de.ClrObject != null)
            {
                if (de.ClrObject is T)
                {
                    obj = (T)de.ClrObject;
                    return DataSourceResult.Ok;
                }
                else
                {
                    return DataSourceResult.ObjectWrongType;
                }
            }

            de.LoadObjectFromData<T>();
            obj = (T)de.ClrObject;

            return DataSourceResult.Ok;
        }

        public object GetObject(object key, object collectionKey = null)
        {
            DataEntry de = GetDataEntry(key, collectionKey);
            if (de == null)
                return null;

            if (de.ClrObject != null)
                return de.ClrObject;

            var expando = new ExpandoObject();
            var dicExpando = (IDictionary<string,object>)expando;
            foreach (var kvp in expando)
                dicExpando.Add(kvp.Key, kvp.Value);

            return expando;
        }

        internal DataEntry GetDataEntry(object key, object collectionKey = null)
        {
            DataEntry de;
            TryGetDataEntry(key, out de, collectionKey);
            return de;
        }

        internal DataSourceResult TryGetDataEntry(object key, out DataEntry dataEntry, object collectionKey = null)
        {
            dataEntry = null;
            collectionKey = collectionKey ?? _DefaultCollectionKey;
            Collection collection;
            if (_Data.TryGetValue(collectionKey, out collection))
            {
                if (collection.TryGetValue(key, out dataEntry))
                    return DataSourceResult.Ok;
                return DataSourceResult.ObjectNotFound;
            }

            return DataSourceResult.CollectionNotFound;
        }

        //public object GetObjects(object collectionKey)
        //{

        //}

        internal class DataEntry : Dictionary<string, object>
        {
            private const string _NullRepresentation = "{null}";
            private const string _PrimitiveValuePropertyKey = "$Value";
            private PropertyInfo[] _PropertyInfos;

            internal object ClrObject { get; set; }
            internal Collection ParentCollection { get; }
            internal object Key { get; }
            internal int PropertyCount { get; }

            internal DataEntry(object original, Collection collection, object key = null)
            {
                ClrObject = original;
                ParentCollection = collection;
                Type type = original.GetType();
                _PropertyInfos = type.GetProperties();
                if (!_PropertyInfos.Any() && !type.IsPrimitive)
                    throw new InvalidOperationException($"Object has no public properties and is not a primitive. Nothing to serialize.");

                if (key == null)
                {
                    key = GetObjectKey(original, _PropertyInfos);
                    if(key == null)
                        key = ParentCollection.GetKey();
                }
                Key = key;
            }

            internal DataEntry(Collection collection)
            {
                ParentCollection = collection;
                Key = ParentCollection.GetKey();
            }

            internal static object GetObjectKey(object obj, PropertyInfo[] propertyInfos = null)
            {
                propertyInfos = propertyInfos ?? obj.GetType().GetProperties();
                PropertyInfo entryKeyProperty = propertyInfos
                        .FirstOrDefault(pi => pi.GetCustomAttributes(typeof(EntryKeyAttribute), true).Any());
                return entryKeyProperty?.GetValue(obj);
            }

            internal void LoadDataFromObject()
            {
                Type type = ClrObject.GetType();
                if (_PropertyInfos.Any())
                {
                    foreach (var prop in _PropertyInfos)
                    {
                        if (!prop.CanRead)
                            continue;

                        object value = prop.GetValue(ClrObject);
                        this[prop.Name] = value;
                    }
                }
                else if (type.IsPrimitive)
                {
                    this[_PrimitiveValuePropertyKey] = ClrObject;
                }
                else
                {
                    //Missing cases where there's no read-only properties.
                    Debug.Assert(false, "No properties and is not a primitive");
                }
            }

            internal void SerializeEntry(List<StringBuilder> serializedCsv, int lineNumber)
            {
                StringBuilder sbuilder = GetLine(serializedCsv, lineNumber);
                foreach (KeyValuePair<string, int> kvp in ParentCollection.PropertyToWidthMap)
                {
                    object property;

                    if (TryGetValue(kvp.Key, out property))
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
                    builder.Append($"{_NullRepresentation},");
                    return;
                }

                Type type = propertyValue.GetType();
                
                if (type == typeof(string) || type == typeof(char))
                {
                    string propertyString = propertyValue.ToString().Replace("{", "{{").Replace("}","}}");
                    builder.Append($"\"{propertyString}\",");
                }
                else if (type.IsPrimitive)
                {
                    builder.Append($"{propertyValue},");
                }
                else // if(type.IsValueType && !type.IsPrimitive || type.IsClass)
                {
                    //Add other serialization handling such as
                    // ToString
                    // SupportCollection
                    //means we need to do something like {collectionKey;entryKey}
                    // also key serialization at some point
                    // Recursive? Serialize a csv... inside a csv? 
                    // (for lolz)
                    string serializedObj = Newtonsoft.Json
                        .JsonConvert
                        .SerializeObject(propertyValue)
                        .Replace("\"", "\"\"");
                    builder.Append($"\"{serializedObj}\",");
                }
            }

            internal void DeserializeEntry(List<string> row)
            {
                int propertyOffset = ParentCollection.CollectionOffset;
                int collectionEnd = ParentCollection.CollectionWidth + ParentCollection.CollectionOffset;
                foreach (KeyValuePair<string, int> kvp in ParentCollection.PropertyToWidthMap)
                {
                    string propertyName = kvp.Key;
                    int propertyWidth = kvp.Value;
                    ListSpan<string> propertyVals = row.GetSpan(propertyOffset, propertyOffset + propertyWidth);
                    DeserializeProperty(propertyName, propertyVals);
                    propertyOffset += propertyWidth;
                }
            }

            private void DeserializeProperty(string propertyName, ListSpan<string> propertyVals)
            {
                string firstPropertyVal = propertyVals.First();
                if (firstPropertyVal == null)
                    return; //Empty entry

                //Without an object we can't do much to interpret these
                // there's either raw or strings
                // raw could be strings, but might be numerical types.
                // strings might be strings, or objects
                if (propertyVals.Count == 1)
                {
                    if (firstPropertyVal == null)
                        return;
                    this[propertyName] = DeserializeString(firstPropertyVal);
                    return;
                }
                // Collections contain in the first element 
                // the number of elements

                int count;
                if (firstPropertyVal == _NullRepresentation)
                {
                    //Null collection
                    this[propertyName] = null;
                    return;
                }
                else if (!int.TryParse(firstPropertyVal, out count))
                {
                    throw new FormatException($"Property '{propertyName}' has more than one value, " +
                        $"but the first value does not represent a number. Value {firstPropertyVal}");
                }

                this[propertyName] = propertyVals
                    .Skip(1)
                    .Take(count)
                    .Select(DeserializeString)
                    .ToArray();
            }

            private object DeserializeString(string val)
            {
                if (val == _NullRepresentation)
                    return null;
                // if its a json object return the object.
                // could check to see if we know the primitive type, and return that too.:ee:\
                return val;
            }

            private object DeserializePropertyValue(Type propertyType, object val)
            {
                if (val == null)
                    return null;

                if (propertyType == null)
                    return val;

                Type savedType = val.GetType();
                if (savedType.IsArray)
                    return DeserializeArrayProperty(propertyType, val);

                string strVal = (string)val;

                if (propertyType == typeof(string))
                    return (string)val;
                if (propertyType == typeof(char))
                    return ((string)val)[0];

                if (propertyType.IsPrimitive)
                {
                    //assume that all primitive types have a parse method?
                    MethodInfo methodInfo = propertyType.GetMethod("Parse", new Type[] { typeof(string) });
                    if (methodInfo == null)
                        throw new NotSupportedException(
                            $"Don't know how to parse primitive type '{propertyType.Name}', " 
                            + $"with value val '{strVal}'");
                    return methodInfo.Invoke(null, new[] { strVal });
                }

                //if (propertyType.IsStruct() || propertyType.IsClass)
                return Newtonsoft.Json.JsonConvert.DeserializeObject(strVal, propertyType);
            }

            private object DeserializeArrayProperty(Type propertyType, object val)
            {
                Array valArray = (Array)val;
                //lets support IList, it does Array and any list
                //(Could crash if cast invalid)
                IList list = (IList)Activator.CreateInstance(propertyType, new object[] { valArray.Length });

                Type genericType;
                if (propertyType.IsArray)
                {
                    genericType = propertyType.GetElementType();
                }
                else
                {
                    genericType = propertyType
                    .GetGenericArguments()
                    .FirstOrDefault();
                }

                int ix = 0;
                foreach (var arrVal in valArray)
                {
                    object desArrVall = DeserializePropertyValue(genericType, arrVal);
                    if (propertyType.IsArray)
                        list[ix++] = desArrVall;
                    else
                        list.Add(desArrVall);
                }
                return list;
            }

            internal void LoadObjectFromData<T>()
            {
                Type type = typeof(T);
                T newInstance = ReflectionUtils.CreateInstance<T>();

                if (type.IsPrimitive)
                {
                    var val = this[_PrimitiveValuePropertyKey];
                    newInstance = (T)DeserializePropertyValue(type, val);
                }
                else
                {
                    _PropertyInfos = type.GetProperties();
                    foreach (var kvp in this)
                    {
                        PropertyInfo pi = _PropertyInfos
                            .FirstOrDefault(p => p.Name == kvp.Key);

                        if (pi == null)
                            continue; //Either polymorphic more base class or multi-type collection

                        object trueVal = DeserializePropertyValue(pi.PropertyType, kvp.Value);

                        ReflectionUtils.UnsafeSetProperty(newInstance, kvp.Key, trueVal);
                    }
                }
                ClrObject = newInstance;
            }
        }

        internal class Collection : IDictionary<object, DataEntry>
        {
            private Dictionary<object, DataEntry> _InternalCollection;
            private Dictionary<object, DataEntry> _ObjectToEntryMap;
            private int _CurrentIndex;
            private object _Key;
            internal int CollectionWidth { get; private set; }
            //Read-properties
            internal int CollectionOffset { get; private set; }
            /// <summary>
            /// The size of the property in number of columns
            /// </summary>
            internal Dictionary<string, int> PropertyToWidthMap { get; private set; }

            internal Collection(object key)
            {
                CommonInit(key);
            }

            internal Collection(object key, int collectionWidth, int collectionOffset)
            {
                CommonInit(key);
                CollectionWidth = collectionWidth;
                CollectionOffset = collectionOffset;
            }

            private void CommonInit(object key)
            {
                _Key = key;
                _CurrentIndex = 0;
                PropertyToWidthMap = new Dictionary<string, int>();
                _InternalCollection = new Dictionary<object, DataEntry>();
                _ObjectToEntryMap = new Dictionary<object, DataEntry>();
            }

            internal void IndexObject(DataEntry entry)
            {
                foreach (KeyValuePair<string, object> kvp in entry)
                    AddOrUpdatePropertySize(kvp.Key, kvp.Value);
            }

            internal void Reset()
            {
                PropertyToWidthMap.Clear();
                CollectionWidth = 0;
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
                foreach (KeyValuePair<string, int> kvp in PropertyToWidthMap)
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
                foreach (KeyValuePair<string, int> kvp in PropertyToWidthMap)
                {
                    sbuilder.Append($"{kvp.Key},");
                    sbuilder.Append(',', kvp.Value - 1);
                    CollectionWidth += kvp.Value;
                }
            }

            internal void SerializeEmptyEntry(List<StringBuilder> serializedCsv, int lineNumber)
            {
                StringBuilder sbuilder = GetLine(serializedCsv, lineNumber);
                sbuilder.Append(',', CollectionWidth);
            }

            /// <summary>
            /// Keep count of each property size to allow padding
            /// </summary>
            private void AddOrUpdatePropertySize(string propertyName, object propertyValue)
            {
                int propertySize = 0;
                if (!PropertyToWidthMap.TryGetValue(propertyName, out propertySize))
                    PropertyToWidthMap.Add(propertyName, 1);

                var propCol = propertyValue as ICollection;
                int currentPropertySize = propCol?.Count + 1 ?? 1;
                PropertyToWidthMap[propertyName] = Math.Max(propertySize, currentPropertySize);
            }

            internal void DeserializeHeader(List<string> collectionsHeaders)
            {
                int propertyOffset = CollectionOffset;
                int collectionEnd = CollectionWidth + CollectionOffset;
                for (int i = CollectionOffset + 1; i <= collectionEnd; ++i)
                {
                    string propertyName = i < collectionEnd
                        ? collectionsHeaders[i]
                        : string.Empty;
                    if (propertyName == null)
                        continue;
                    //new property
                    string lastPropertyName = collectionsHeaders[propertyOffset];
                    int propertyWidth = i - propertyOffset;
                    PropertyToWidthMap.Add(lastPropertyName, propertyWidth);
                    propertyOffset = i;
                }
            }

            public override string ToString()
            {
                return _Key.ToString();
            }

            #region Dictionary Impl
            private IDictionary<object, DataEntry> AsDictionary
            {
                get { return _InternalCollection; }
            }

            public bool ContainsKey(object key)
            {
                return AsDictionary.ContainsKey(key);
            }

            public void Add(object key, DataEntry value)
            {
                AsDictionary.Add(key, value);
                //if (value.ClrObject != null)
                //    _ObjectToEntryMap.Add(value.ClrObject, value);
            }

            public bool Remove(object key)
            {
                DataEntry de;
                if (!_InternalCollection.TryGetValue(key, out de))
                    return false;
                // _ObjectToEntryMap.Remove(de.ClrObject);
                return AsDictionary.Remove(key);
            }

            public bool TryGetValue(object key, out DataEntry value)
            {
                return AsDictionary.TryGetValue(key, out value);
            }

            public void Clear()
            {
                AsDictionary.Clear();
                //_ObjectToEntryMap.Clear();
            }

            public IEnumerator<KeyValuePair<object, DataEntry>> GetEnumerator()
            {
                return AsDictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return AsDictionary.GetEnumerator();
            }

            public ICollection<object> Keys => AsDictionary.Keys;

            public ICollection<DataEntry> Values => AsDictionary.Values;

            public int Count => AsDictionary.Count;

            public bool IsReadOnly => AsDictionary.IsReadOnly;

            public DataEntry this[object key]
            {
                get => AsDictionary[key];
                set => AsDictionary[key] = value;
            }
            #endregion

            #region ICollection Explicit Implementation
            void ICollection<KeyValuePair<object, DataEntry>>.Add(KeyValuePair<object, DataEntry> item)
            {
                Add(item.Key, item.Value);
            }

            bool ICollection<KeyValuePair<object, DataEntry>>.Contains(KeyValuePair<object, DataEntry> item)
            {
                return AsDictionary.Contains(item);
            }

            void ICollection<KeyValuePair<object, DataEntry>>.CopyTo(KeyValuePair<object, DataEntry>[] array, int arrayIndex)
            {
                AsDictionary.CopyTo(array, arrayIndex);
            }

            bool ICollection<KeyValuePair<object, DataEntry>>.Remove(KeyValuePair<object, DataEntry> item)
            {
                return Remove(item.Key);
            }
            #endregion
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EntryKeyAttribute : Attribute
    {
        object Key { get; }
    }
}
