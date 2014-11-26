using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tests.Common
{
    public static class AttributeExtensions
    {
        public static IEnumerable<T> GetAllAttributes<T>( this Type type ) where T: Attribute
        {
            var classAttributes = type.GetCustomAttributes<T>();

            var fieldsAttributes = type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                .SelectMany( m => m.GetCustomAttributes<T>() )
                .Where( a => a != null );

            var memberAttributes = type.GetMembers()
                .SelectMany( m => m.GetCustomAttributes<T>() )
                .Where( a => a != null );

            return classAttributes.Concat( fieldsAttributes ).Concat( memberAttributes );
        }
    }
}
