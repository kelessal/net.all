using Net.Json;
using Net.Proxy;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Xunit;

namespace Net.Mapper.Test
{
    public class MapperTests
    {

        [Fact]
        public void IsLogicalEqualTest()
        {
            var a = new MappingClassA() { Name="Salih"};
            var b = new MappingClassA() { Name="Salihk"};
            var result=a.IsLogicalEqual(b);

        }

        [Fact]
        public void MappingSameTypes()
        {
            var obj1 = new MappingClassA() { Name = "John", Age = 40, AgeList = new List<int> { 1, 2 } };
            var cloneObj = obj1.ObjectMap(typeof(MappingClassA));
            Assert.NotEqual(obj1, cloneObj);
            var obj2 = cloneObj as MappingClassA;
            Assert.Equal(obj1.Name, obj2.Name);
            Assert.Equal(obj1.Age, obj2.Age);
            Assert.NotStrictEqual(obj1.AgeList, obj2.AgeList);
        }
       
        
    }
}
