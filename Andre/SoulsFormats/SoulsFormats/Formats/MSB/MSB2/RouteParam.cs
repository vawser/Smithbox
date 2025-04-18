using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB2
    {
        private class RouteParam : Param<Entry>
        {
            internal override int Version => 5;
            internal override string Name => "ROUTE_PARAM_ST";

            public RouteParam() { }

            internal override Entry ReadEntry(BinaryReaderEx br)
            {
                throw new NotSupportedException("Route param should always be empty in DS2.");
            }

            public override List<Entry> GetEntries()
            {
                throw new NotSupportedException("Route param should always be empty in DS2.");
            }
        }
    }
}
