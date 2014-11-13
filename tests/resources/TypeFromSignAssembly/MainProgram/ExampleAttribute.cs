using System;

namespace MainProgram
{
    [AttributeUsage( AttributeTargets.Class )]
    public class ExampleAttribute : Attribute
    {
        public ExampleAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; private set; }
    }
}