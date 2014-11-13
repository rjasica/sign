using System;
using System.Reflection;

namespace MainProgram
{
    public class Program
    {
        static void Main( string[] args )
        {
            Test();
        }

        private static void Test()
        {
            try
            {
                var attribute = typeof( TestClass ).GetCustomAttribute<ExampleAttribute>();

                var instance = Activator.CreateInstance( attribute.Type );

                attribute.Type.GetMethod( "Write" ).Invoke( instance, null );
            }
            catch( Exception er )
            {
                Console.WriteLine( "Error {0}", er );
            }
        }
    }
}
