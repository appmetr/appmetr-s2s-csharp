using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using AppmetrS2S.Actions;
using AppmetrS2S.Persister;

namespace AppmetrS2S.Serializations
{
    public class JavaScriptJsonSerializer : IJsonSerializer
    {
        private static readonly JavaScriptSerializer Serializer;

        static JavaScriptJsonSerializer()
        {
            Serializer = new JavaScriptSerializer();
            Serializer.MaxJsonLength = 20*1024*1024; // 20 MB
            Serializer.RegisterConverters(new[] {new BatchJsonConverter()});
        }

        public string Serialize(object obj)
        {
            var json = Serializer.Serialize(obj);
            return json;
        }

        public T Deserialize<T>(string json)
        {
            var result = Serializer.Deserialize<T>(json);
            return result;
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
                if (Attribute.GetCustomAttribute(objType, typeof(DataContractAttribute)) == null) return null;

                var result = new Dictionary<string, object> { { TypeFieldName, objType.AssemblyQualifiedName } };

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
                get { return new[] { typeof(Batch), typeof(AppMetrAction) }; }
            }

            private static object ConvertDictionaryToObject(IDictionary<string, object> dictionary, Type type)
            {
                var objType = GetSerializedObjectType(dictionary);
                if (objType == null) return null;

                var constructor =
                    objType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null) ??
                    objType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);

                var result = constructor.Invoke(null);

                Action<DataMemberAttribute, MemberInfo> action = (attribute, info) =>
                {
                    Type fieldType = info is FieldInfo
                        ? (info as FieldInfo).FieldType
                        : info is PropertyInfo ? (info as PropertyInfo).PropertyType : null;
                    MethodInfo setValue = info.GetType()
                        .GetMethod("SetValue", new[] { typeof(object), typeof(object) });

                    if (fieldType == null || setValue == null) return;

                    object value = GetValue(dictionary, attribute.Name);

                    if (typeof(ICollection<AppMetrAction>).IsAssignableFrom(fieldType))
                    {
                        var serializedActions = value as ArrayList;

                        if (serializedActions != null)
                        {
                            var actions = (ICollection<AppMetrAction>)Activator.CreateInstance(fieldType);
                            foreach (var val in serializedActions)
                            {
                                if (val is IDictionary<string, object>)
                                    actions.Add(
                                        (AppMetrAction)
                                            ConvertDictionaryToObject(val as IDictionary<string, object>,
                                                GetSerializedObjectType(dictionary)));
                            }
                            setValue.Invoke(info, new[] { result, actions });
                        }
                    }
                    else
                    {
                        setValue.Invoke(info, new[] { result, value });
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
                if (!dictionary.TryGetValue(TypeFieldName, out typeName) || !(typeName is string))
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
                while (!(typeof(object) == objType))
                {
                    foreach (FieldInfo field in objType.GetFields(bindingFlags))
                    {
                        var dataMemberAttribute =
                            (DataMemberAttribute)field.GetCustomAttribute(typeof(DataMemberAttribute));
                        if (dataMemberAttribute != null)
                        {
                            fieldProcessor.Invoke(dataMemberAttribute, field);
                        }
                    }

                    foreach (PropertyInfo property in objType.GetProperties(bindingFlags))
                    {
                        var dataMemberAttribute =
                            (DataMemberAttribute)property.GetCustomAttribute(typeof(DataMemberAttribute));
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
