using System;

using DependLibrary;

using Tests.Common;

namespace MainProgram
{
    public class Program
    {
        public static void Main( string[] args )
        {
            var classAttributes = typeof( ExampleClass.EmbededClass ).GetAllAttributes<TypeParameterAttribute>();

            foreach( var attr in classAttributes )
            {
                TestAttribute( attr );
            }
        }

        private static void TestAttribute( TypeParameterAttribute attribute )
        {
            var instance = (ExternalClass)Activator.CreateInstance( attribute.Property );
            instance.WriteStatus();
        }
    }
}