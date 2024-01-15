using System;
using System.Collections.Generic;
using System.Text;

namespace Net.Mapper.Test
{
    public class MappingClassB
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public MappingClassBNested Nested { get; set; }
    }
    public class MappingClassBNested
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
   
}
