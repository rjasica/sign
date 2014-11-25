using System;
using System.Threading;

namespace MainProgram
{
    [Example( typeof( SignAssembly.Writer ) )]
    public class TestClass
    {
        [Example( typeof( SignAssembly.Writer ) )]
        private int field;

        [Example( typeof( SignAssembly.Writer ) )]
        public event EventHandler Event;

        [Example( typeof( SignAssembly.Writer ) )]
        public int Property { get; set; }

        [Example( typeof( SignAssembly.Writer ) )]
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