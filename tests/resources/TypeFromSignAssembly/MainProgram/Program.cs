using System;
using System.Linq;
using System.Reflection;

namespace MainProgram
{
    public class Program
    {
        public static void Main( string[] args )
        {
            Test();
        }

        private static void Test()
        {
            try
            {
                var embededClassType = typeof( TestClass );
                var classAttributes = embededClassType.GetCustomAttributes<ExampleAttribute>();

                var fieldsAttributes = embededClassType.GetFields( 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                    .SelectMany( m => m.GetCustomAttributes<ExampleAttribute>() )
                    .Where( a => a != null );

                var memberAttributes = embededClassType.GetMembers()
                    .SelectMany( m => m.GetCustomAttributes<ExampleAttribute>() )
                    .Where( a => a != null );

                foreach( var attr in classAttributes.Concat( fieldsAttributes ).Concat( memberAttributes ) )
                {
                    TestAttribute( attr );
                }
            }
            catch( Exception er )
            {
                Console.WriteLine( "Error {0}", er );
            }
        }

        private static void TestAttribute( ExampleAttribute attribute )
        {
            var instance = (SignAssembly.Writer)Activator.CreateInstance( attribute.Type );
            instance.Write();
        }
    }
}
