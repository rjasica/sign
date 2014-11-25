using System;

namespace MainProgram
{
    [AttributeUsage( AttributeTargets.All )]
    public class ExampleAttribute : Attribute
    {
        public ExampleAttribute( Type property )
        {
            this.Property = property;
        }

        public Type Property { get; private set; }
    }
}