using Net.All.Mapper;
using Net.Json;
using Net.Proxy;
using Net.Reflection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;

namespace Net.Mapper
{
    public class Mapper
    {

        readonly TypePair TypePair;
        public readonly LambdaExpression LambdaExpression;
        private Delegate CompiledDelegate;
        public object Map(object instance)
        {
            if (instance == null) return default;
            if (!this.CanMappable) return default;
            if (this.CompiledDelegate == null)
            {
                this.CompiledDelegate = this.LambdaExpression.Compile();
            }
            try
            {

            return this.CompiledDelegate.DynamicInvoke(instance);
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }
        public bool CanMappable { get; private set; }

        Type GetInterfaceType(Type type)
        {
            if (type.IsGenericType && type.IsAssignableTo(typeof(IEnumerable))) return typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]);
            return InterfaceType.GetProxyType(type);
        }
        internal Mapper(TypePair pair)
        {

            this.TypePair = new TypePair(pair.SrcType, pair.DestType.IsInterface ?
                GetInterfaceType(pair.DestType): pair.DestType);

            this.CanMappable =this.IsMappableOf(this.TypePair.SrcType,this.TypePair.DestType);
            if (!this.CanMappable)  return;
            this.LambdaExpression = CreateExpression();
        }
        internal bool IsMappableOf(Type source, Type dest)
        {
            if (source == dest) return true;
            var srcInfo = source.GetInfo();
            var destInfo = dest.GetInfo();
            if (srcInfo.Kind == destInfo.Kind) return true;
            if (srcInfo.Kind == TypeKind.Complex)
            {
                if (destInfo.Type == typeof(JObject)) return true;
                if (destInfo.Kind == TypeKind.Dynamic) return true;
                if (destInfo.Kind == TypeKind.Dictionary && destInfo.Type == typeof(Dictionary<string, object>)) return true;
                return false;
            }
            if (srcInfo.Kind == TypeKind.Collection)
            {
                if (destInfo.Type == typeof(JArray)) return true;
                return destInfo.Kind == TypeKind.Collection;
            }
            if (srcInfo.Kind == TypeKind.Dictionary)
            {
                if(destInfo.Type==typeof(JObject)) return true; 
                if (destInfo.Kind == TypeKind.Complex) return true;
                if (destInfo.Kind == TypeKind.Dynamic) return true;
                return false;
            }
            if(srcInfo.Kind == TypeKind.Dynamic)
            {
                if(destInfo.Type==typeof(JObject)) return true;
                if(destInfo.Kind==TypeKind.Complex) return true;
                if (destInfo.Kind == TypeKind.Dictionary && destInfo.Type == typeof(Dictionary<string, object>)) return true;
                return false;
            }
            

            return srcInfo.Kind != TypeKind.Unknown;

        }

        internal Mapper(TypePair pair,LambdaExpression expression)
        {
            this.TypePair = pair;
            this.CanMappable = true;
            this.LambdaExpression = expression;
        }

        private LambdaExpression CreateExpression()
        {
            if (!this.CanMappable) return default;
            var typeKind = this.TypePair.SrcType.GetTypeKind();
            //if (this.TypePair.IsSameTypes)
            //   return PrimitiveMapExpressionBuilder.Create(this.TypePair);
            switch (typeKind)
            {
                case TypeKind.Primitive:
                   return PrimitiveMapExpressionBuilder.Create(this.TypePair);
                case TypeKind.Complex:
                   return ComplexMapExpressionBuilder.Create(this.TypePair);
                case TypeKind.Collection:
                  return CollectionMapExpressionBuilder.Create(this.TypePair);
                case TypeKind.Dictionary:
                    return DictionaryMapExpressionBuilder.Create(this.TypePair);
                case TypeKind.Dynamic:
                    return DynamicMapExpressionBuilder.Create(this.TypePair);
                default:
                    return null;
            }
        }
        

    }
}
