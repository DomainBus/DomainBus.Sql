
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DomainBus.Sql
{
    public class NonPublicSettersResolver : DefaultContractResolver
    {

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {

            var list = base.GetSerializableMembers(objectType);
            var propertyInfos = objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(d => d.CanWrite && list.All(e => e.Name != d.Name)).ToArray();
            list.AddRange(propertyInfos);
            return list;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (prop.Writable) return prop;
            var pinfo = member as PropertyInfo;
            var setter = pinfo?.GetSetMethod(true);
            if (setter == null) return prop;
            prop.Writable = true;
            return prop;
        }
    }

    public static class Serializer
    {

        private static JsonSerializerSettings _settings;

        public static JsonSerializerSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new JsonSerializerSettings()
                    {

                        TypeNameHandling =
                            TypeNameHandling.Objects,
                        ContractResolver = new NonPublicSettersResolver(),
                        PreserveReferencesHandling =
                            PreserveReferencesHandling.Objects,
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                    };
                }
                return _settings;
            }
        }

        public static string Serialize<T>(this T data)
        {
            var rez = JsonConvert.SerializeObject(data, Settings);
            return rez;
        }

        public static T Deserialize<T>(this string data)
        {
            var tp = typeof (T);
            var rez = JsonConvert.DeserializeObject(data, typeof (T), Settings);
            if (tp.IsArray)
            {
                return rez.As<JArray>().ToObject<T>();
            }
            return (T) rez;
        }


    }

}