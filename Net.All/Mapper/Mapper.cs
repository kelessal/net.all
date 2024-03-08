using Net.All.Mapper;
using Net.Proxy;
using Net.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Net.Mapper
{
    public class Mapper
    {

        readonly TypePair TypePair;
        public readonly LambdaExpression LambdaExpression;
        private Delegate CompiledDelegate;
        public object Map(object instance)
        {
            if (!this.CanMappable) return default;
            if(this.CompiledDelegate == null)
            {
                this.CompiledDelegate = this.LambdaExpression.Compile();
            }
            return this.CompiledDelegate.DynamicInvoke(instance);
        }
        public bool CanMappable { get; private set; }
        internal Mapper(TypePair pair)
        {

            this.TypePair = new TypePair(pair.SrcType, pair.DestType.IsInterface ?
                InterfaceType.GetProxyType(pair.DestType): pair.DestType);
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
                if (destInfo.Kind == TypeKind.Dynamic) return true;
                if (destInfo.Kind == TypeKind.Dictionary && destInfo.Type == typeof(Dictionary<string, object>)) return true;
                return false;
            }
            if (srcInfo.Kind == TypeKind.Dictionary)
            {
                if (destInfo.Kind == TypeKind.Complex) return true;
                if (destInfo.Kind == TypeKind.Dynamic) return true;
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
                default:
                    return null;
            }
        }
        

    }
}
