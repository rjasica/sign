using System;
using System.Linq;
using System.Reflection;
using DependLibrary;

namespace MainProgram
{
    public class Program
    {
        public static void Main( string[] args )
        {
            var embededClassType = typeof( ExampleClass.EmbededClass );
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

        private static void TestAttribute( ExampleAttribute attribute )
        {
            var instance = (ExternalClass)Activator.CreateInstance( attribute.Property );
            instance.WriteStatus();
        }
    }
}