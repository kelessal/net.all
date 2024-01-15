using Net.Expressions;
using Net.Extensions;
using Net.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;

namespace Net.Mapper
{
    static class DictionaryMapExpressionBuilder
    {
        static ExpandoObject ConvertExpandoToExpandoMapping(ExpandoObject src)
        {
            if(src == null) return null;
            var result=new ExpandoObject() as IDictionary<string,object>;
            var input=src as IDictionary<string,object>;
            foreach(var kv in input)
            {
                var value = kv.Value;
                if (value == null) continue;
                result[kv.Key] = value.ObjectMap(kv.Value.GetType());
            }
            return result as ExpandoObject;

        }
        static T ConvertComplexToExpandoMapping<T>(IDictionary<string,object> src)
            where T:class
        {
            if(src == null) return null;
            var result = Activator.CreateInstance<T>();
            var destTypeInfo = typeof(T).GetInfo();
            foreach(var kv in src)
            {
                if (kv.Value == null) continue;
                var propInfo = destTypeInfo[kv.Key];
                if (propInfo.IsNull()) continue;
                propInfo.SetValue(result, kv.Value.ObjectMap(propInfo.Type));
            }
            return result;
        }
        static T ConvertComplexToExpandoMapping<T>(IDictionary<string, object> src)
            where T : class
        {
            if (src == null) return null;
            var result = Activator.CreateInstance<T>();
            var destTypeInfo = typeof(T).GetInfo();
            foreach (var kv in src)
            {
                if (kv.Value == null) continue;
                var propInfo = destTypeInfo[kv.Key];
                if (propInfo.IsNull()) continue;
                propInfo.SetValue(result, kv.Value.ObjectMap(propInfo.Type));
            }
            return result;
        }
        static Dictionary<string,object> ConvertExpandoToDictionaryMapping<T>(ExpandoObject src)
            where T : class
        {
            if (src == null) return null;
            var result = new Dictionary<string, object>();
            var input = src as IDictionary<string, object>;
            foreach (var kv in input)
            {
                var value = kv.Value;
                if (value == null) continue;
                result[kv.Key] = value.ObjectMap(kv.Value.GetType());
            }
            return result;
        }
        public static LambdaExpression Create(TypePair pair)
        {
            var parameter = Expression.Parameter(pair.SrcType, pair.SrcType.Name.ToLowerInvariant());
            var srcInfo = pair.SrcType.GetInfo();
            var destInfo = pair.DestType.GetInfo();
            if(destInfo.Kind==TypeKind.Complex)
            {
                var writableDestinationProperties = destInfo.GetAllProperties().Where(p => p.Raw.CanWrite);
                List<MemberBinding> bindings = new List<MemberBinding>();
                foreach (var destProp in writableDestinationProperties)
                {
                    if (destProp.HasAttribute<NoMapAttribute>()) continue;
                    var mapPropName = destProp.GetAttribute<PropertyMapAttribute>()?.MappedPropertyName ?? destProp.Name;
                    
                    
                    if (!srcInfo.HasProperty(mapPropName)) continue;
                    
                    var srcProp = srcInfo[mapPropName];
                    var propPair = new TypePair(srcProp.Type, destProp.Type);
                    if (MappingExtensions.HasLock(propPair)) continue;
                    var mapper = propPair.GetMapper();
                    if (mapper == null) continue;
                    if (!mapper.CanMappable) continue;
                    var srcPropExp = Expression.Property(parameter, srcProp.Raw);
                    var lambda = mapper.LambdaExpression.ReplaceParameter(mapper.LambdaExpression.Parameters[0], srcPropExp) as LambdaExpression;
                    var newBinding = Expression.Bind(destProp.Raw, lambda.Body);
                    bindings.Add(newBinding);
                }
                var newExp = Expression.New(destInfo.Type);
                var memInitExp = Expression.MemberInit(newExp, bindings.ToArray());
                return Expression.Lambda(memInitExp, parameter);

            }
           
        }
    }
}
