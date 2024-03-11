using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Net.All.Mapper;
using System.Reflection;
using Net.Extensions;
using Net.Reflection;
using System;
using Newtonsoft.Json.Linq;

namespace Net.Mapper
{
    static class CollectionMapExpressionBuilder
    {
        static TDest[] MapJArrayToArray<TDest>(JArray json)
        {
           return MapJArrayToList<TDest>(json)?.ToArray(); 
        }
        static HashSet<TDest> MapJArrayToHashSet<TDest>(JArray json)
        {
            return MapJArrayToList<TDest>(json)?.ToHashSet();
        }
        static JArray MapJArrayToJArray<TDest>(JArray json)
        {
            if (json == null) return default;
            var result = new JArray(json);
            return result;
        }
        static List<TDest> MapJArrayToList<TDest>(JArray json)
        {
            if (json == null) return default;
            var result =new List<TDest>();
            if (json.IsEmpty()) return result;
            foreach(var item in json) 
            { 
                result.Add(item.AsCloned<TDest>());
            }
            return result; 
        }
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
        static HashSet<TDest> MapToHashSet<TSrc, TDest>(IEnumerable<TSrc> src)
        {
            return MapToList<TSrc, TDest>(src)?.ToHashSet();
        }
        public static LambdaExpression Create(TypePair pair)
        {
            var parameter = Expression.Parameter(pair.SrcType,pair.SrcType.Name.ToLowerInvariant());
            var srcInfo = pair.SrcType.GetInfo();
            var destInfo = pair.DestType.GetInfo();
            if(srcInfo.Type==typeof(JArray))
            {
                if (destInfo.Type == typeof(JArray))
                {
                    var mi = typeof(CollectionMapExpressionBuilder).GetMethod(nameof(MapJArrayToJArray), BindingFlags.NonPublic | BindingFlags.Static);
                    var callExp = Expression.Call(null, mi, parameter);
                    return Expression.Lambda(callExp, parameter);
                }
                else if (!destInfo.ElementTypeInfo.IsNull())
                {
                    var methodName = "";
                    if (destInfo.Type.IsArray)
                        methodName = nameof(MapJArrayToArray);
                    else if (destInfo.Type == typeof(HashSet<>).MakeGenericType(destInfo.ElementTypeInfo.Type))
                        methodName = nameof(MapJArrayToHashSet);
                    else
                        methodName = nameof(MapJArrayToList);
                    
                    var mi = typeof(CollectionMapExpressionBuilder).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
                    var gmi = mi.MakeGenericMethod(new[] { destInfo.ElementTypeInfo.Type });
                    var callExp = Expression.Call(null, gmi, parameter);
                    return Expression.Lambda(callExp, parameter);
                    
                }
                else throw new NotImplementedException("Mapping is not available");
            }
            else
            {
                var methodName = "";
                if (destInfo.Type.IsArray)
                    methodName = nameof(MapToArray);
                else if (destInfo.Type == typeof(HashSet<>).MakeGenericType(destInfo.ElementTypeInfo.Type))
                    methodName = nameof(MapToHashSet);
                else
                    methodName = nameof(MapToList);
                var mi = typeof(CollectionMapExpressionBuilder).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
                var gmi = mi.MakeGenericMethod(new[] { srcInfo.ElementTypeInfo.Type, destInfo.ElementTypeInfo.Type });
                var callExp = Expression.Call(null, gmi, parameter);
                return Expression.Lambda(callExp, parameter);

            }

        }
    }
}
