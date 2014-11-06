using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InternalLibrary;

namespace MainProgram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var external = new ExampleClass();

            external.WriteStatus();
        }
    }
}
