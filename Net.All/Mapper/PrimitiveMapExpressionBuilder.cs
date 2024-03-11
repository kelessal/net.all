using Net.Reflection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Net.Mapper
{
    static class PrimitiveMapExpressionBuilder
    {
        static MethodInfo toStringMi = typeof(Object).GetMethod("ToString");
        static TDest MapJson<TDest>(JValue jobj)
        {
            var value = jobj?.Value;
            if (value == null) return default;
            if (typeof(TDest) == typeof(TimeSpan)) 
            {
                var val = Convert.ToInt64(value);
                var netticks = 10000 * val;
                netticks = System.Math.Min(TimeSpan.MaxValue.Ticks, netticks);
                netticks = System.Math.Max(TimeSpan.MinValue.Ticks, netticks);
                value= new TimeSpan(netticks);
            }
            else
                 value = jobj.Value.As(typeof(TDest));
            return (TDest)value;
        }
        public static LambdaExpression Create(TypePair pair)
        {
            var parameter = Expression.Parameter(pair.SrcType,pair.SrcType.Name.ToLowerInvariant());
            if (pair.SrcType == typeof(JValue))
            {
                var mi = typeof(PrimitiveMapExpressionBuilder).GetMethod(nameof(MapJson), BindingFlags.NonPublic | BindingFlags.Static);
                var gmi = mi.MakeGenericMethod(new Type[] { pair.DestType });
                var callExp = Expression.Call(null, gmi, parameter);
                return Expression.Lambda(callExp, parameter);
            }
            if (pair.IsSameTypes)
                return Expression.Lambda(parameter, parameter);
            else if (pair.DestType == typeof(string))
                return Expression.Lambda(Expression.Call(parameter, toStringMi), parameter);
            else
                return Expression.Lambda(Expression.Convert(parameter, pair.DestType), parameter);
        }
    }
}
