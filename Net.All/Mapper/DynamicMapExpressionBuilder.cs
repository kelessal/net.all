using Net.All.Mapper;
using Net.Expressions;
using Net.Extensions;
using Net.Proxy;
using Net.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Net.Mapper
{
    static class DynamicMapExpressionBuilder
    {
      
        static Dictionary<string,object> MapToDictionary(ExpandoObject src)
        {
            if (src == null) return default;
            var result = new Dictionary<string, object>();  
            foreach (var item in src)
            {
                result[item.Key] = item.Value.AsCloned();
            }
            return result;
        }
        static ExpandoObject MapToExpando(ExpandoObject src)
        {
            if (src == null) return default;
            var result = new ExpandoObject() as IDictionary<string, object>;
            foreach (var item in src)
            {
                result[item.Key] = item.Value.AsCloned();
            }
            return (ExpandoObject)result;
        }
        static TDest MapToClass<TDest>(ExpandoObject src, TypePropertyInfo[] mappingProps)
        {
            if (src == null) return default;
            var result = Activator.CreateInstance<TDest>();
            if (mappingProps.IsEmpty()) return result;
            var dic = src as IDictionary<string, object>;
            foreach (var prop in mappingProps)
            {
                var value = dic.GetSafeValue(prop.Name)?? dic.GetSafeValue(prop.CamelName);
                prop.SetValue(result, value.AsCloned(prop.Type));
               
            }
            return result;
        }
        public static LambdaExpression Create(TypePair pair)
        {
            var parameter = Expression.Parameter(pair.SrcType, pair.SrcType.Name.ToLowerInvariant());
            var srcInfo = pair.SrcType.GetInfo();
            var destInfo = pair.DestType.GetInfo();
            if (destInfo.Type == typeof(Object))
                destInfo = srcInfo;
            if (destInfo.Kind == TypeKind.Complex)
            {
                var mappingProps = destInfo.GetAllProperties().Where(p => p.Raw.CanWrite && !p.HasAttribute<NoMapAttribute>())
                    .ToArray();
                var mi = typeof(DynamicMapExpressionBuilder).GetMethod(nameof(MapToClass), BindingFlags.NonPublic | BindingFlags.Static);
                var gmi = mi.MakeGenericMethod(new Type[] { destInfo.Type });
                var mappingPropsExps = Expression.Constant(mappingProps);
                var callExp = Expression.Call(null, gmi, parameter, mappingPropsExps);
                return Expression.Lambda(callExp, parameter);

            }
            else if (destInfo.Kind == TypeKind.Dynamic)
            {
                var mi = typeof(DynamicMapExpressionBuilder).GetMethod(nameof(MapToExpando), BindingFlags.NonPublic | BindingFlags.Static);
                var callExp = Expression.Call(null, mi, parameter);
                return Expression.Lambda(callExp, parameter);
            }
            else if (destInfo.Type == typeof(Dictionary<string, object>))
            {
                var mi = typeof(DynamicMapExpressionBuilder).GetMethod(nameof(MapToDictionary), BindingFlags.NonPublic | BindingFlags.Static);
                var callExp = Expression.Call(null, mi, parameter);
                return Expression.Lambda(callExp, parameter);
            }
            else
                throw new NotImplementedException("Mapping is not available");


        }
    }
}
