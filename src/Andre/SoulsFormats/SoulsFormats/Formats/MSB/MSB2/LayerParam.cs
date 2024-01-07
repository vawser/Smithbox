using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB2
    {
        private class LayerParam : Param<Entry>
        {
            internal override int Version => 5;
            internal override string Name => "LAYER_PARAM_ST";

            public LayerParam() { }

            internal override Entry ReadEntry(BinaryReaderEx br)
            {
                throw new NotSupportedException("Layer param should always be empty in DS2.");
            }

            public override List<Entry> GetEntries()
            {
                throw new NotSupportedException("Layer param should always be empty in DS2.");
            }
        }
    }
}
