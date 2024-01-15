using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Net.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace Net.Mapper
{
    static class CollectionMapExpressionBuilder
    {
      
        static List<TDest> ConvertToList<TSrc,TDest>(IEnumerable<TSrc> items, Func<TSrc, TDest> selector)
        {
            if (items == null) return null;
            return items.Select(selector).ToList();
        }
        static TDest[] ConvertToArray<TSrc,TDest>(IEnumerable<TSrc> items, Func<TSrc, TDest> selector)
        {
            if (items == null) return null;
            return items.Select(selector).ToArray();
        }
        static IEnumerable<TDest> ConvertToEnumerable<TSrc, TDest>(IEnumerable<TSrc> items, Func<TSrc, TDest> selector)
        {
            if (items == null) return null;
            return items.Select(selector);
        }
        public static LambdaExpression Create(TypePair pair)
        {
            var parameter = Expression.Parameter(pair.SrcType,pair.SrcType.Name.ToLowerInvariant());
            var srcInfo = pair.SrcType.GetInfo();
            var destInfo = pair.DestType.GetInfo();
            var elementPair = new TypePair(srcInfo.ElementTypeInfo.Type, destInfo.ElementTypeInfo.Type);
            if (MappingExtensions.HasLock(elementPair)) return null;
            var mapper = elementPair.GetMapper();
            if (!mapper.CanMappable) return null;
            if (pair.DestType.IsArray)
            {
                var miArray = typeof(CollectionMapExpressionBuilder).FindMethod(nameof(ConvertToArray), _=>true, srcInfo.ElementTypeInfo.Type, destInfo.ElementTypeInfo.Type);
                var convertExp = Expression.Call(miArray, parameter, mapper.LambdaExpression);
                return Expression.Lambda(convertExp, parameter);
            }
            else if (pair.DestType==typeof(List<>).MakeGenericType(destInfo.ElementTypeInfo.Type))
            {

                var miList = typeof(CollectionMapExpressionBuilder).FindMethod(nameof(ConvertToList), _ => true, srcInfo.ElementTypeInfo.Type, destInfo.ElementTypeInfo.Type);
                var convertExp = Expression.Call(miList, parameter, mapper.LambdaExpression);
                return Expression.Lambda(convertExp, parameter);
            }
            else if (pair.DestType.IsAssignableTo(typeof(IEnumerable<>).MakeGenericType(destInfo.ElementTypeInfo.Type)))
            {

                var miEnumerable = typeof(CollectionMapExpressionBuilder).FindMethod(nameof(ConvertToEnumerable),  _=>true, srcInfo.ElementTypeInfo.Type, destInfo.ElementTypeInfo.Type);
                var convertExp = Expression.Call(miEnumerable, parameter, mapper.LambdaExpression);
                return Expression.Lambda(convertExp, parameter);
            }
            else
                throw new NotImplementedException();

        }
    }
}
