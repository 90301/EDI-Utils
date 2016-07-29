using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleApplication1
{
    class Person
    {
        public String name;
        public int age;

        public string outputInfo()
        {
            return name + " : " + age;
        }


        public override string ToString()
        {
            return outputInfo();
        }
    }
}
