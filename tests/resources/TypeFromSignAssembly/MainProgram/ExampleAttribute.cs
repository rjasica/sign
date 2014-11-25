using System;

namespace MainProgram
{
    [AttributeUsage( AttributeTargets.All )]
    public class ExampleAttribute : Attribute
    {
        public ExampleAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; private set; }
    }
}