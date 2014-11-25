using System;
using System.Reflection;

using Tests.Common;

namespace MainProgram
{
    public class Program
    {
        public static void Main( string[] args )
        {
            try
            {
                var classAttributes = typeof( TestClass ).GetAllAttributes<TypeParameterAttribute>();

                foreach( var attr in classAttributes )
                {
                    TestAttribute( attr );
                }
            }
            catch( Exception er )
            {
                Console.WriteLine( "Error {0}", er );
            }
        }

        private static void TestAttribute( TypeParameterAttribute attribute )
        {
            var instance = (SignAssembly.Writer)Activator.CreateInstance( attribute.Property );
            instance.Write();
        }
    }
}
