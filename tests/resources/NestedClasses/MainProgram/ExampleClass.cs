using System;
using System.Threading;

using DependLibrary;

namespace MainProgram
{
    public class ExampleClass
    {
        [Example( typeof( ExternalClass ) )]
        public class EmbededClass
        {
            [Example( typeof( ExternalClass ) )]
            private int field;

            [Example( typeof( ExternalClass ) )]
            public event EventHandler Event;

            [Example( typeof( ExternalClass ) )]
            public int Property { get; set; }

            [Example( typeof( ExternalClass ) )]
            public void Method()
            {
                this.field++;
                var @event = Interlocked.CompareExchange( ref this.Event, null, this.Event );
                if( @event != null )
                {
                    @event( this, new EventArgs() );
                }

                Console.WriteLine( this.field );
            }
        }
    }
}
