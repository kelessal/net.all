using Net.Proxy;
using Net.Reflection;
using System;
using System.Linq.Expressions;

namespace Net.Mapper
{
    public class Mapper
    {
        static bool IsMappableOf(Type source, Type dest)
        {
            if (source == dest) return true;
            var srcInfo = source.GetInfo();
            var destInfo = source.GetInfo();
            if (srcInfo.Kind != destInfo.Kind) return false;
            return srcInfo.Kind != TypeKind.Unknown;

        }
        readonly TypePair TypePair;
        public readonly LambdaExpression LambdaExpression;
        private Delegate CompiledDelegate;
        public object Map(object instance)
        {
            if (!this.CanMappable) throw new NotSupportedException($"{this.TypePair} is not mappable");
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
            this.CanMappable = IsMappableOf(this.TypePair.SrcType, this.TypePair.DestType);
            if (!this.CanMappable)  return;
            this.LambdaExpression = CreateExpression();
        }
        
       
        internal Mapper(TypePair pair,LambdaExpression expression)
        {
            this.TypePair = pair;
            this.CanMappable = true;
            this.LambdaExpression = expression;
        }

        private LambdaExpression CreateExpression()
        {
            var srcTypeInfo = this.TypePair.SrcType.GetInfo();
            //if (this.TypePair.IsSameTypes)
            //   return PrimitiveMapExpressionBuilder.Create(this.TypePair);
            switch (srcTypeInfo.Kind)
            {
                case TypeKind.Primitive:
                   return PrimitiveMapExpressionBuilder.Create(this.TypePair);
                case TypeKind.Complex:
                   return ComplexMapExpressionBuilder.Create(this.TypePair);
                case TypeKind.Collection:
                  return CollectionMapExpressionBuilder.Create(this.TypePair);
                default:
                    return null;
            }
        }
        

    }
}
