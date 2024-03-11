using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Net.Reflection.Test
{
    public enum TestEnum
    {
        A=0, B=1, C=2, D=3, E=24, F=25, G=26,H=125

    }
    class TestObject
    {
        public IList<NestedTestObject> List { get; set; }
        public string Name0 { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Name4 { get; set; }
        public string Name5 { get; set; }
        public string Name6 { get; set; }
        public string Name7 { get; set; }
        public string Name8 { get; set; }
        public string Name9 { get; set; }
        public string Name10 { get; set; }
        public string Name11 { get; set; }
        public string Name12 { get; set; }
        public string Name13 { get; set; }
        public string Name14 { get; set; }
        public string Name15 { get; set; }
        public string Name16 { get; set; }
        public string Name17 { get; set; }
        public string Name18 { get; set; }
        public string Name19 { get; set; }
        public string Name20 { get; set; }
        public string Name21 { get; set; }
        public string Name22 { get; set; }
        public string Name23 { get; set; }

        public int Age0 { get; set; }
        public int Age1 { get; set; }
        public int Age2 { get; set; }
        public int Age3 { get; set; }
        public int Age4 { get; set; }
        public int Age5 { get; set; }
        public int Age6 { get; set; }
        public int Age7 { get; set; }
        public int Age8 { get; set; }
        public int Age9 { get; set; }
        public int Age10 { get; set; }
        public int Age11 { get; set; }
        public int Age12 { get; set; }
        public int Age13 { get; set; }
        public int Age14 { get; set; }
        public int Age15 { get; set; }
        public int Age16 { get; set; }
        public int Age17 { get; set; }
        public int Age18 { get; set; }
        public int Age19 { get; set; }
        public int Age20 { get; set; }
        public int Age21 { get; set; }
        public int Age22 { get; set; }
        public int Age23 { get; set; }
        public NestedTestObject NTO { get; private set; } = new NestedTestObject();
        public Dictionary<string, NestedTestObject> NTODic { get; private set; } = new Dictionary<string, NestedTestObject>();

    }
    public class NestedTestObject
    {
        public string Id { get; set; }
        public string Name0 { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Name4 { get; set; }
        public string Name5 { get; set; }
        public string Name6 { get; set; }
        public string Name7 { get; set; }
        public string Name8 { get; set; }
        public string Name9 { get; set; }
        public string Name10 { get; set; }
        public string Name11 { get; set; }
        public string Name12 { get; set; }
        public string Name13 { get; set; }
        public string Name14 { get; set; }
        public string Name15 { get; set; }
        public string Name16 { get; set; }
        public string Name17 { get; set; }
        public string Name18 { get; set; }
        public string Name19 { get; set; }
        public string Name20 { get; set; }
        public string Name21 { get; set; }
        public string Name22 { get; set; }
        public string Name23 { get; set; }

        public int Age0 { get; set; }
        public int Age1 { get; set; }
        public int Age2 { get; set; }
        public int Age3 { get; set; }
        public int Age4 { get; set; }
        public int Age5 { get; set; }
        public int Age6 { get; set; }
        public int Age7 { get; set; }
        public int Age8 { get; set; }
        public int Age9 { get; set; }
        public int Age10 { get; set; }
        public int Age11 { get; set; }
        public int Age12 { get; set; }
        public int Age13 { get; set; }
        public int Age14 { get; set; }
        public int Age15 { get; set; }
        public int Age16 { get; set; }
        public int Age17 { get; set; }
        public int Age18 { get; set; }
        public int Age19 { get; set; }
        public int Age20 { get; set; }
        public int Age21 { get; set; }
        public int Age22 { get; set; }
        public int Age23 { get; set; }
    }
    public interface TestInterface
    {
        public IList<NestedTestObject> List { get; set; }
        public string Name0 { get; set; }
        public int Age0 { get; set; }
        public TestEnum Age1 { get; set; }
        public TestEnum TestEnum { get; set; }

    }
    class TestSmallObject
    {
        public IList<object> List { get; set; }
        public TestEnum Age1 { get; set; }
        public string Name0 { get; set; }
        public int Age0 { get; set;}
        //public Dictionary<int,NestedTestSmallObject> NTODic { get; private set; }
        public Dictionary<string,object> NTO { get; set; }
    }
    class NestedTestSmallObject
    {
        public string Name0 { get; set; }
        public int Age0 { get; set; }

    }
}
