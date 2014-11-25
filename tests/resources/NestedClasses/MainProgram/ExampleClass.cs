using System;
using System.Threading;

using DependLibrary;

using Tests.Common;

namespace MainProgram
{
    public class ExampleClass
    {
        [TypeParameter( typeof( ExternalClass ) )]
        public class EmbededClass
        {
            [TypeParameter( typeof( ExternalClass ) )]
            private int field;

            [TypeParameter( typeof( ExternalClass ) )]
            public event EventHandler Event;

            [TypeParameter( typeof( ExternalClass ) )]
            public int Property { get; set; }

            [TypeParameter( typeof( ExternalClass ) )]
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
