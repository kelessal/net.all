using Net.All.Mapper;
using Net.Expressions;
using Net.Extensions;
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
    static class ComplexMapExpressionBuilder
    {
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
        public static LambdaExpression Create(TypePair pair)
        {
            var parameter = Expression.Parameter(pair.SrcType, pair.SrcType.Name.ToLowerInvariant());
            var srcInfo = pair.SrcType.GetInfo();
            var destInfo = pair.DestType.GetInfo();
            var mappableSourceProperties = srcInfo.GetAllProperties().Where(p=>!p.HasAttribute<NoMapAttribute>()).ToArray();
            if (destInfo.Kind == TypeKind.Complex)
            {
                var writableDestinationProperties = destInfo.GetAllProperties().Where(p => p.Raw.CanWrite);
                List<MemberBinding> bindings = new List<MemberBinding>();
                foreach (var destProp in writableDestinationProperties)
                {
                    if (destProp.HasAttribute<NoMapAttribute>()) continue;
                    if (!srcInfo.HasProperty(destProp.Name)) continue;
                    var srcProp = srcInfo[destProp.Name];
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
            else if (destInfo.Kind == TypeKind.Dynamic)
            {
                var mi = typeof(ComplexMapExpressionBuilder).GetMethod(nameof(MapClassToExpando), BindingFlags.NonPublic | BindingFlags.Static);
                var gmi = mi.MakeGenericMethod(new Type[] { pair.SrcType });
                var mappingPropsExps = Expression.Constant(mappableSourceProperties);
                var callExp = Expression.Call(null, gmi, parameter, mappingPropsExps);
                return Expression.Lambda(callExp, parameter);
            }
            else if (destInfo.Type == typeof(Dictionary<string, object>))
            {
                var mi = typeof(ComplexMapExpressionBuilder).GetMethod(nameof(MapClassToDictionary), BindingFlags.NonPublic | BindingFlags.Static);
                var gmi = mi.MakeGenericMethod(new Type[] { pair.SrcType });
                var mappingPropsExps = Expression.Constant(mappableSourceProperties);
                var callExp = Expression.Call(null, gmi, parameter, mappingPropsExps);
                return Expression.Lambda(callExp, parameter);
            }
            else
                throw new NotImplementedException("Mapping is not available");


        }
    }
}
