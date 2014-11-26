using System;

namespace Tests.Common
{
    [AttributeUsage( AttributeTargets.All )]
    public class TypeParameterAttribute : Attribute
    {
        public TypeParameterAttribute( Type property )
        {
            this.Property = property;
        }

        public Type Property { get; private set; }
    }
}