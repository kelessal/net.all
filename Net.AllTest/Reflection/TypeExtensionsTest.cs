using Net.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using Xunit;

namespace Net.Reflection.Test
{
    public class TypeExtensionsTest
    {
        [Fact]
        public void LogicalEqualTest()
        {
            var obj1 = new
            {
                Name = "hello",
                Address = new
                {
                    City = "İstanbul",
                    Countr = "Turkey"
                },
                Numbers =(int[]) null
            };
            var obj2 = new
            {
                Name = "hello",
                Address = new
                {
                    City = "İstanbul",
                    Countr = "Turkey"
                },
                Numbers = new[] { 1, 2, 3 }
            };
        }

        [Fact]
        public void ConvertToDictionaryTest()
        {
            var obj = new
            {
                Name = "hello",
                Address = new
                {
                    City = "İstanbul",
                    Countr = "Turkey"
                },
                Numbers=new[] {1,2,3}
            };
            obj.ConvertToExpando();
            var result= obj.ConvertToExpando();
        }
        [Fact]
        public void SetPathValueTest()
        {
            var item= new TestObject();
            item.SetPathValue("NTO.Index", -3);
            
        }
        [Fact]
        public void AsTest()
        {
            var item = new { name = "hello", age = "3" };

            var x=item.As<TestObject>();

        }
        [Fact]
        public void JsonTest()
        {
            var item = new { name = "hello", age = "3" };

            var x = item.As<JObject>();
            var result=x.GetPropValue("name");
        }
       
        [Fact]
        public void AsCloned2PerfomenceTest()
        {
            var testObj = this.CreateTestObject();
            var item = testObj.AsCloned<TestInterface>();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                var item2 = testObj.AsCloned<TestInterface>();
            }
            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;

        }

        [Fact]
        public void CamelCaseTest()
        {
            var item = new TestObject();
            var type=item.GetType().GetInfo();
            var prop = type.GetPropertyByPath("nto.propA");
        }
        int index;
        TestObject CreateTestObject()
        {
            var result=new TestObject();
            var typeInfo=typeof(TestObject).GetInfo();
            var list=new List<int>();  
           for(var i = 0; i < 100; i++)
            {
                list.Add(index++);
            }
            result.List = list.ToArray();
            foreach(var prop in typeInfo.GetAllProperties())
            {
                if (prop.Name.StartsWith("Name"))
                    prop.SetValue(result, (index++).ToString());
                else if (prop.Name.StartsWith("Age"))
                    prop.SetValue(result, index++);
                else if (prop.Name == "NTO")
                    prop.SetValue(result, CreateNestedTestObject());
                else if (prop.Name == "NTODic")
                {
                    var dic = new Dictionary<string, NestedTestObject>();
                    for(int i = 0; i < 1000; i++)
                    {
                        dic[i.ToString()] = CreateNestedTestObject();
                    }
                    prop.SetValue(result, dic);
                }
            }
            return result;
        }
        NestedTestObject CreateNestedTestObject()
        {
            var result = new NestedTestObject();
            var typeInfo = typeof(NestedTestObject).GetInfo();
            foreach (var prop in typeInfo.GetAllProperties())
            {
                if (prop.Name.StartsWith("Name"))
                    prop.SetValue(result, (index++).ToString());
                else if (prop.Name.StartsWith("Age"))
                    prop.SetValue(result, index++);
            }
            return result;
        }
    }
}
