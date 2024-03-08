using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Net.All.Mapper;
using System.Reflection;
using Net.Extensions;
using Net.Reflection;
using System;

namespace Net.Mapper
{
    static class CollectionMapExpressionBuilder
    {
        static List<TDest> MapToList<TSrc, TDest>(IEnumerable<TSrc> src)
        {
            if (src == null) return null;
            if (src.IsEmpty()) return new List<TDest>();
           var result=new List<TDest>();
            foreach (var item in src)
            {
                result.Add(item.AsCloned<TDest>());
            }
            return result;
        }
        static TDest[] MapToArray<TSrc, TDest>(IEnumerable<TSrc> src)
        {
            return MapToList<TSrc,TDest>(src)?.ToArray();
        }

        public static LambdaExpression Create(TypePair pair)
        {
            var parameter = Expression.Parameter(pair.SrcType,pair.SrcType.Name.ToLowerInvariant());
            var srcInfo = pair.SrcType.GetInfo();
            var destInfo = pair.DestType.GetInfo();
            if (destInfo.Type.IsArray)
            {
                var mi = typeof(CollectionMapExpressionBuilder).GetMethod(nameof(MapToArray), BindingFlags.NonPublic | BindingFlags.Static);
                var gmi = mi.MakeGenericMethod(new[] { srcInfo.ElementTypeInfo.Type, destInfo.ElementTypeInfo.Type });
                var callExp = Expression.Call(null, gmi, parameter);
                return Expression.Lambda(callExp, parameter);
            }
            else if (destInfo.Type == typeof(List<>).MakeGenericType(destInfo.ElementTypeInfo.Type) || destInfo.Type == typeof(IEnumerable<>).MakeGenericType(destInfo.ElementTypeInfo.Type))
            {
                var mi = typeof(CollectionMapExpressionBuilder).GetMethod(nameof(MapToList), BindingFlags.NonPublic | BindingFlags.Static);
                var gmi = mi.MakeGenericMethod(new[] { srcInfo.ElementTypeInfo.Type, destInfo.ElementTypeInfo.Type });
                var callExp = Expression.Call(null, gmi, parameter);
                return Expression.Lambda(callExp, parameter);
            }
            else
                throw new NotImplementedException("Mapping is not available");

        }
    }
}
