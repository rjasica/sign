using DependLibrary;

namespace MainProgram
{
    public class ExampleClass
    {
        [Example( typeof( ExternalClass ) )]
        public class EmbededClass
        {
        }
    }
}
