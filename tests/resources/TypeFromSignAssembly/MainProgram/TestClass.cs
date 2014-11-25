using System;
using System.Threading;

using Tests.Common;

namespace MainProgram
{
    [TypeParameter( typeof( SignAssembly.Writer ) )]
    public class TestClass
    {
        [TypeParameter( typeof( SignAssembly.Writer ) )]
        private int field;

        [TypeParameter( typeof( SignAssembly.Writer ) )]
        public event EventHandler Event;

        [TypeParameter( typeof( SignAssembly.Writer ) )]
        public int Property { get; set; }

        [TypeParameter( typeof( SignAssembly.Writer ) )]
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