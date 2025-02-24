﻿using Azure.Data.Tables;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;

namespace Dev.Data.Tables
{
    public static class EntityMapExtensions
    {
        private static readonly ConcurrentDictionary<string, PropertyInfo[]> TypeProperties = new();

        /// <summary>
        /// Threadsafe caching PropertyInfo[] because the check for the IgnoreDataMemberAttribute slows property lookup
        /// </summary>
        /// <param name="type">Type that implements ITableEntity and new() </param>
        /// <returns><seealso cref="PropertyInfo[]"/></returns>
        private static PropertyInfo[] GetProperties(Type type)
        {
            return TypeProperties.GetOrAdd(type.FullName,
                (name) => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
                .Where(w => w.GetCustomAttribute(typeof(IgnoreDataMemberAttribute)) == null)
                .ToArray());
        }

        public static T MapTableEntity<T>(this TableEntity dte) where T : ITableEntity, new()
        {
            T t = new();
            foreach (var prop in GetProperties(typeof(T)))
            {
                if (dte.TryGetValue(prop.Name, out object obj))
                {
                    prop.SetValue(t, obj);
                }
            }

            return t;
        }
    }
}
