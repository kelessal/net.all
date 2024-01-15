using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Net.Mapper.Test
{
    public class MappingClassA
    {
        public Dictionary<int, int> Dic { get; set; }=new Dictionary<int, int>();
        public string Name { get; set; }
        public long Age { get; set; }
        //public TestAA Nested { get; set; }
        public int[] AgeArray { get; set; }
        public List<int> AgeList { get; set; }
    }

 
    public class TestAA
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
