using Microsoft.IdentityModel.Tokens;
using Net.Extensions;
using Net.Mapper;
using Net.Reflection;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Net.All.Mapper
{
    static class DictionaryMapExpressionBuilder
    {
        static Dictionary<TDestKey,TDestValue> MapToDictionaryFn<TSrcKey,TSrcValue,TDestKey,TDestValue>(Dictionary<TSrcKey,TSrcValue> src)
        {
            if (src == null) return null;
            var result=new Dictionary<TDestKey, TDestValue>();
            foreach (var item in src)
            {
                var keyDest = item.Key.As<TDestKey>();
                result[keyDest] = src[item.Key].AsCloned<TDestValue>();
            }
            return result;
        }
        static  TDestValue MapToDictionaryToClass<TSrcValue, TDestValue>(Dictionary<string, TSrcValue> src, TypePropertyInfo[] mappingProps)
        {
            if (src == null) return default;
            var result =Activator.CreateInstance<TDestValue>();
            if (mappingProps.IsEmpty()) return result;
            var props = typeof(TDestValue).GetInfo().GetAllProperties();
            foreach (var prop in props)
            {
                var value = src.ContainsKey(prop.Name) ? src[prop.Name]:src.GetSafeValue(prop.CamelName);
                prop.SetValue(result, value.AsCloned(prop.Type));
            }
            return result;
        }
        static ExpandoObject MapToDictionaryToExpando<TSrcValue>(Dictionary<string, TSrcValue> src)
        {
            if (src == null) return default;
            var result = new ExpandoObject() as IDictionary<string,object>;
            foreach (var item in src)
            {
                if(item.Value==null) continue;
                result[item.Key] = item.Value.AsCloned();
            }
            return (ExpandoObject) result;
        }
        public static LambdaExpression Create(TypePair pair)
        {
            var parameter = Expression.Parameter(pair.SrcType, pair.SrcType.Name.ToLowerInvariant());
            var srcInfo = pair.SrcType.GetInfo();
            var destInfo = pair.DestType.GetInfo();
            if (destInfo.Kind==TypeKind.Dictionary)
            {
                var mi = typeof(DictionaryMapExpressionBuilder).GetMethod(nameof(MapToDictionaryFn), BindingFlags.NonPublic | BindingFlags.Static);
                var genericArgs = pair.SrcType.GetGenericArguments().Concat(pair.DestType.GetGenericArguments()).ToArray();
                var gmi = mi.MakeGenericMethod(genericArgs);
                var callExp = Expression.Call(null, gmi, parameter);
                return Expression.Lambda(callExp, parameter);
            }
            else if (destInfo.Kind == TypeKind.Complex)
            {
                var mi = typeof(DictionaryMapExpressionBuilder).GetMethod(nameof(MapToDictionaryToClass), BindingFlags.NonPublic | BindingFlags.Static);
                var genericArgs = new Type[] { pair.SrcType.GetGenericArguments()[0], pair.DestType };
                var gmi = mi.MakeGenericMethod(genericArgs);
                var typeInfo = pair.DestType.GetInfo();
                var mappingProps = typeInfo.GetAllProperties().Where(p => !p.HasAttribute<NoMapAttribute>()).ToArray();
                var mappingPropsExp = Expression.Constant(mappingProps);
                var callExp = Expression.Call(null, gmi, parameter, mappingPropsExp);
                return Expression.Lambda(callExp, parameter);
            }
            else if (destInfo.Kind == TypeKind.Dynamic)
            {
                var mi = typeof(DictionaryMapExpressionBuilder).GetMethod(nameof(MapToDictionaryToExpando), BindingFlags.NonPublic | BindingFlags.Static);
                var genericArgs = pair.SrcType.GetGenericArguments();
                var gmi = mi.MakeGenericMethod(new Type[] { genericArgs[1] });
                var callExp = Expression.Call(null, gmi, parameter);
                return Expression.Lambda(callExp, parameter);
            }
            else
                throw new NotImplementedException("Mapping is not available");
           
        }
    }
}
