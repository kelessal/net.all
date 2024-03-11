using Net.All.Mapper;
using Net.Expressions;
using Net.Extensions;
using Net.Proxy;
using Net.Reflection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Net.Mapper
{
    static class ComplexMapExpressionBuilder
    {
      
        static TDest MapJson<TDest>(JObject jobj, TypePropertyInfo[] mappingProps)
        {
            
            if (jobj == null) return default;
            var result=Activator.CreateInstance<TDest>();
            if(mappingProps.IsEmpty()) return result;
            foreach(var prop in mappingProps )
            {
                    
                var value = jobj.ContainsKey(prop.Name) ? jobj[prop.Name] : jobj[prop.CamelName];
                prop.SetValue(result, value.AsCloned(prop.Type));
                   
            }
            return result;
            
        }
        static ExpandoObject MapJsonToExpando(JObject jobj)
        {

            if (jobj == null) return default;
            return jobj.ToObject<ExpandoObject>();

        }
        static Dictionary<string,object> MapClassToDictionary<TSrcValue>(TSrcValue src, TypePropertyInfo[] mappingProps)
        {
            if (src == null) return default;
            var result = new Dictionary<string, object> ();
            if (mappingProps.IsEmpty()) return result;
            var props = typeof(TSrcValue).GetInfo().GetAllProperties();
            foreach (var prop in props)
            {
                var value = prop.GetValue(src);
                result[prop.Name]= value.AsCloned();
            }
            return result;
        }
        
        static ExpandoObject MapClassToExpando<TSrcValue>(TSrcValue src, TypePropertyInfo[] mappingProps)
        {
            if (src == null) return default;
            var result = new ExpandoObject() as IDictionary<string, object>;
            if (mappingProps.IsEmpty()) return (ExpandoObject) result;
            foreach (var prop in mappingProps)
            {
                var value = prop.GetValue(src);
                result[prop.Name] = value.AsCloned();
            }
            return (ExpandoObject)result;
        }
        static TDest MapClassToClass<TSrc,TDest>(TSrc src, Dictionary<TypePropertyInfo,TypePropertyInfo> mappingProps)
        {
            if (src == null) return default;
            
            var result = Activator.CreateInstance<TDest>();
            foreach (var item in mappingProps)
            {
                var value = item.Key.GetValue(src);
                item.Value.SetValue(result, value.AsCloned(item.Value.Type));
               
            }
            return result;
        }
        public static LambdaExpression Create(TypePair pair)
        {
            var parameter = Expression.Parameter(pair.SrcType, pair.SrcType.Name.ToLowerInvariant());
            var srcInfo = pair.SrcType.GetInfo();
            var destInfo = pair.DestType.GetInfo();
            
            if (pair.SrcType == typeof(JObject))
            {
                if (pair.DestType == typeof(Object))
                {

                    var mi = typeof(ComplexMapExpressionBuilder).GetMethod(nameof(MapJsonToExpando), BindingFlags.NonPublic | BindingFlags.Static);
                    var callExp = Expression.Call(null, mi, parameter);
                    return Expression.Lambda(callExp, parameter);
                }
                else
                {
                    var mappableDestProps = destInfo.GetAllProperties().Where(p => !p.HasAttribute<NoMapAttribute>()).ToArray();

                    var mi = typeof(ComplexMapExpressionBuilder).GetMethod(nameof(MapJson), BindingFlags.NonPublic | BindingFlags.Static);
                    var gmi = mi.MakeGenericMethod(new Type[] { destInfo.Type });
                    var mappingPropsExps = Expression.Constant(mappableDestProps);
                    var callExp = Expression.Call(null, gmi, parameter, mappingPropsExps);
                    return Expression.Lambda(callExp, parameter);
                }
            
            }
            if (pair.DestType == typeof(Object))
                destInfo = srcInfo;
            var mappableSourceProperties = srcInfo.GetAllProperties().Where(p => !p.HasAttribute<NoMapAttribute>()).ToArray();

            if (destInfo.Kind == TypeKind.Complex)
            {
                var writableDestinationProperties = destInfo.GetAllProperties().Where(p => p.Raw.CanWrite &&  !p.HasAttribute<NoMapAttribute>());
                List<MemberBinding> bindings = new List<MemberBinding>();
                //foreach (var destProp in writableDestinationProperties)
                //{
                //    if (destProp.HasAttribute<NoMapAttribute>()) continue;
                //    if (!srcInfo.HasProperty(destProp.Name)) continue;
                //    var srcProp = srcInfo[destProp.Name];
                //    var propPair = new TypePair(srcProp.Type, destProp.Type);
                //    if (MappingExtensions.HasLock(propPair)) continue;
                //    var mapper = propPair.GetMapper();
                //    if (mapper == null) continue;
                //    if (!mapper.CanMappable) continue;
                //    var srcPropExp = Expression.Property(parameter, srcProp.Raw);
                //    var lambda = mapper.LambdaExpression.ReplaceParameter(mapper.LambdaExpression.Parameters[0], srcPropExp) as LambdaExpression;
                //    var newBinding = Expression.Bind(destProp.Raw, lambda.Body);
                //    bindings.Add(newBinding);
                //}
                //var newExp = Expression.New(destInfo.Type);
                //var memInitExp = Expression.MemberInit(newExp, bindings.ToArray());
                //return Expression.Lambda(memInitExp, parameter);
                var mappingProps=new Dictionary<TypePropertyInfo,TypePropertyInfo>();
                foreach (var destProp in writableDestinationProperties)
                {
                    if (!srcInfo.HasProperty(destProp.Name)) continue;
                    var srcProp = srcInfo[destProp.Name];
                    var propPair = new TypePair(srcProp.Type, destProp.Type);
                    var mapper = propPair.GetMapper();
                    if (mapper == null) continue;
                    if (!mapper.CanMappable) continue;
                    mappingProps[srcProp] = destProp;
                }
                var mi = typeof(ComplexMapExpressionBuilder).GetMethod(nameof(MapClassToClass), BindingFlags.NonPublic | BindingFlags.Static);
                var gmi = mi.MakeGenericMethod(new Type[] { srcInfo.Type,destInfo.Type });
                var mappingPropsExps = Expression.Constant(mappingProps);
                var callExp = Expression.Call(null, gmi, parameter, mappingPropsExps);
                return Expression.Lambda(callExp, parameter);
               
            }
            else if (destInfo.Kind == TypeKind.Dynamic)
            {
                var mi = typeof(ComplexMapExpressionBuilder).GetMethod(nameof(MapClassToExpando), BindingFlags.NonPublic | BindingFlags.Static);
                var gmi = mi.MakeGenericMethod(new Type[] { srcInfo.Type });
                var mappingPropsExps = Expression.Constant(mappableSourceProperties);
                var callExp = Expression.Call(null, gmi, parameter, mappingPropsExps);
                return Expression.Lambda(callExp, parameter);
            }
            else if (destInfo.Type == typeof(Dictionary<string, object>))
            {
                var mi = typeof(ComplexMapExpressionBuilder).GetMethod(nameof(MapClassToDictionary), BindingFlags.NonPublic | BindingFlags.Static);
                var gmi = mi.MakeGenericMethod(new Type[] { srcInfo.Type });
                var mappingPropsExps = Expression.Constant(mappableSourceProperties);
                var callExp = Expression.Call(null, gmi, parameter, mappingPropsExps);
                return Expression.Lambda(callExp, parameter);
            }
            else
                throw new NotImplementedException("Mapping is not available");


        }
    }
}
