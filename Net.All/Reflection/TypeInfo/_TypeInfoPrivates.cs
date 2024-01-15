using Net.Json;
using Net.Proxy;
using Net.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Reflection
{
    partial class TypeInfo
    {
        static readonly HashSet<Type> primitiveTypes = new HashSet<Type>(new[]
      {
            typeof(string),
            typeof(int),
            typeof(int?),
            typeof(double),
            typeof(double?),
            typeof(float),
            typeof(float?),
            typeof(decimal),
            typeof(decimal?),
            typeof(long),
            typeof(long?),
            typeof(Guid),
            typeof(Guid?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(bool),
            typeof(bool?),
            //typeof(TimeSpan),
            //typeof(TimeSpan?),
            typeof(byte)
        });
        bool IsCollectionType(Type type)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(type)) return false;
            if (type.IsArray && type.GetArrayRank() == 1) return true;
            if (!type.IsGenericType) return false;
            if (type.GetGenericArguments().Length != 1) return false;
            return typeof(IEnumerable<>).MakeGenericType(type.GetCollectionElementType()).IsAssignableFrom(type);
        }

        bool IsDictionaryType(Type type)
        {
            if (type == typeof(ExpandoObject)) return true;
            if (!typeof(IDictionary).IsAssignableFrom(type)) return false;
            if (!type.IsGenericType) return false;
            var args = type.GetGenericArguments();
            if (args.Length != 2) return false;
            if (!IsPrimitiveType(args[0])) return false;
            return typeof(IDictionary<,>).MakeGenericType(args).IsAssignableFrom(type);
        }
        TypeKind GetTypeKind(Type type)
        {
            if (type.Name.Contains("AnonymousType")) return TypeKind.Complex;
            if (IsPrimitiveType(type)) return TypeKind.Primitive;
            if (IsDictionaryType(type)) return TypeKind.Dictionary;
            if (IsCollectionType(type)) return TypeKind.Collection;
            if (IsComplexType(type)) return TypeKind.Complex;
            return TypeKind.Unknown;
        }
        bool IsComplexType(Type type)
        {
            if (type.IsGenericTypeDefinition) return false;
            if (type.IsClass) return true;
            if (type.IsInterface && type.IsAssignableTo(typeof(IProxyData))) return true;
            return false;
        }
        bool IsPrimitiveType(Type type)
            => primitiveTypes.Contains(type) || type.IsEnum;
        void ParseProperties(Dictionary<Type, TypeInfo> workingTypes)
        {
            foreach (var propInfo in this.Type.FindProperties())
            {
                var prop = TypePropertyInfo.Create(this, propInfo, workingTypes);
                this._allProperties[propInfo.Name] = prop;
                this._camelCaseProperties[prop.CamelName] = prop;
            }
        }
        void ParseDictionary()
        {
            if (this.Type == typeof(ExpandoObject))
            {
                this.KeyTypeInfo = typeof(string).GetInfo();
                this.ElementTypeInfo=typeof(Object).GetInfo();
            }
            else
            {
                var args = this.Type.GetGenericArguments();
                this.KeyTypeInfo = args[0].GetInfo();
                this.ElementTypeInfo = args[1].GetInfo();
            }
            
        }
    }
}
