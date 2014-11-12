using System;
using System.Reflection;
using DependLibrary;

namespace MainProgram
{
    public class Program
    {
        public static void Main( string[] args )
        {
            var embededClassType = typeof( ExampleClass.EmbededClass );
            var attribute = embededClassType.GetCustomAttribute<ExampleAttribute>();

            var instance = ( ExternalClass )Activator.CreateInstance( attribute.Property );

            instance.WriteStatus();
        }
    }
}