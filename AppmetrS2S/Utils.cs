using System.Net.Mail;

namespace AppmetrS2S
{
    #region using directives

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Web.Script.Serialization;
    using Actions;
    using log4net;
    using log4net.Repository.Hierarchy;
    using Persister;
	using log4net;

    #endregion

    internal class Utils
    {
        private static readonly ILog Log = LogUtils.GetLogger(typeof(FileBatchPersister));

        private static JavaScriptSerializer serializer;
        private static readonly ILog Log = LogManager.GetLogger(typeof(Utils));

        static Utils()
        {
            serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] {new BatchJsonConverter()});
        }

        public static long GetNowUnixTimestamp()
        {
            return (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }

        public static byte[] SerializeBatch(Batch batch)
        {
            var json = serializer.Serialize(batch);
            byte[] data = Encoding.UTF8.GetBytes(json);
            return data;
        }

        public static void WriteData(Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static void WriteBatch(Stream stream, Batch batch)
        {
            Log.DebugFormat("Starting serialize batch with id={0}", batch.GetBatchId());
            var json = serializer.Serialize(batch);
            Log.DebugFormat("Get bytes from serialized batch with id={0}", batch.GetBatchId());
            byte[] data = Encoding.UTF8.GetBytes(json);
            Log.DebugFormat("Write bytes to stream. Batch id={0}", batch.GetBatchId());
            stream.Write(data, 0, data.Length);
            Log.DebugFormat("Flush stream. Batch id={0}", batch.GetBatchId());
            stream.Flush();
        }

        public static bool TryReadBatch(Stream stream, out Batch batch)
        {
            try
            {
                batch = serializer.Deserialize<Batch>(new StreamReader(stream).ReadToEnd());
                Log.InfoFormat("Successfully read the batch {0}", batch.GetBatchId());
                return true;
            }
            catch (Exception e)
            {
<<<<<<< HEAD
                if (Log.IsErrorEnabled)
                {
                    Log.Error("Error while deserialization batch", e);    
                }
                
=======
                Log.Error("Error deserializing batch", e);
>>>>>>> 5cb464b3c8bd888746bce2e13ed730af203fa4da
                batch = null;
                return false;
            }
        }

        /// <summary>
        /// If you want to add new Object types for this serializer, you should add this type to <see cref="SupportedTypes"/>, and write a little bit of code in <see cref="ConvertDictionaryToObject"/> method
        /// </summary>
        internal class BatchJsonConverter : JavaScriptConverter
        {
            private const string TypeFieldName = "___type";
            //We couldn't use __ prefix, cause this prefix are used for DataContractSerializer and Deserialize method throw Exception

            public override object Deserialize(IDictionary<string, object> dictionary, Type type,
                JavaScriptSerializer serializer)
            {
                return ConvertDictionaryToObject(dictionary, type);
            }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                if (ReferenceEquals(obj, null)) return null;

                Type objType = obj.GetType();
                if (Attribute.GetCustomAttribute(objType, typeof (DataContractAttribute)) == null) return null;

                var result = new Dictionary<string, object>() {{TypeFieldName, objType.AssemblyQualifiedName}};

                ProcessFieldsAndProperties(obj,
                    (attribute, info) =>
                    {
                        if (info.GetValue(obj) != null) result.Add(attribute.Name, info.GetValue(obj));
                    },
                    (attribute, info) => 
                    {
                        if (info.GetValue(obj) != null) result.Add(attribute.Name, info.GetValue(obj));
                    });

                return result;
            }

            public override IEnumerable<Type> SupportedTypes
            {
                get { return new[] {typeof (Batch), typeof (AppMetrAction)}; }
            }

            private static object ConvertDictionaryToObject(IDictionary<string, object> dictionary, Type type)
            {
                var objType = GetSerializedObjectType(dictionary);
                if (objType == null) return null;

                ConstructorInfo constructor = objType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new Type[0], null);
                if (constructor == null)
                    constructor = objType.GetConstructor(BindingFlags.Instance | BindingFlags.Public,
                        null, new Type[0], null);

                var result = constructor.Invoke(null);

                Action<DataMemberAttribute, MemberInfo> action = (attribute, info) =>
                {
                    Type fieldType = info is FieldInfo
                        ? (info as FieldInfo).FieldType
                        : info is PropertyInfo ? (info as PropertyInfo).PropertyType : null;
                    MethodInfo setValue = info.GetType()
                        .GetMethod("SetValue", new Type[] {typeof (object), typeof (object)});

                    if (fieldType == null || setValue == null) return;

                    object value = GetValue(dictionary, attribute.Name);

                    if (typeof (ICollection<AppMetrAction>).IsAssignableFrom(fieldType))
                    {
                        var serializedActions = value as ArrayList;

                        if (serializedActions != null)
                        {
                            var actions = (ICollection<AppMetrAction>) Activator.CreateInstance(fieldType);
                            foreach (var val in serializedActions)
                            {
                                if (val is IDictionary<string, object>)
                                    actions.Add(
                                        (AppMetrAction)
                                            ConvertDictionaryToObject(val as IDictionary<string, object>,
                                                GetSerializedObjectType(dictionary)));
                            }
                            setValue.Invoke(info, new object[] {result, actions});
                        }
                    }
                    else
                    {
                        setValue.Invoke(info, new object[] {result, value});
                    }
                };

                ProcessFieldsAndProperties(result,
                    action,
                    action);

                return result;
            }

            private static Type GetSerializedObjectType(IDictionary<string, object> dictionary)
            {
                object typeName;
                if (!dictionary.TryGetValue(TypeFieldName, out typeName) || typeName as string == null)
                    return null;

                return Type.GetType(typeName as string);
            }

            private static object GetValue(IDictionary<string, object> dictionary, string key)
            {
                object value;
                dictionary.TryGetValue(key, out value);

                return value;
            }

            private static void ProcessFieldsAndProperties(object obj,
                Action<DataMemberAttribute, FieldInfo> fieldProcessor,
                Action<DataMemberAttribute, PropertyInfo> propertiesProcessor)
            {
                Type objType = obj.GetType();


                const BindingFlags bindingFlags =
                    BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public |
                    BindingFlags.NonPublic;
                while (!(typeof (object) == objType))
                {
                    foreach (FieldInfo field in objType.GetFields(bindingFlags))
                    {
                        var dataMemberAttribute =
                            (DataMemberAttribute) field.GetCustomAttribute(typeof (DataMemberAttribute));
                        if (dataMemberAttribute != null)
                        {
                            fieldProcessor.Invoke(dataMemberAttribute, field);
                        }
                    }

                    foreach (PropertyInfo property in objType.GetProperties(bindingFlags))
                    {
                        var dataMemberAttribute =
                            (DataMemberAttribute) property.GetCustomAttribute(typeof (DataMemberAttribute));
                        if (dataMemberAttribute != null)
                        {
                            propertiesProcessor.Invoke(dataMemberAttribute, property);
                        }
                    }

                    objType = objType.BaseType;
                }
            }
        }
    }
}