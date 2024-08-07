using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.TAE.Animation;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Temporary class use to hold the AnimMiniHeader properties, 
/// which are then applied directly to a new AnimMiniHeader and added to the actually Animation object.
/// Needed due to the structure of AnimMiniHeader
/// </summary>
public class TemporaryAnimHeader
{
    public MiniHeaderType CurrentType { get; set; }

    public bool IsLoopByDefault { get; set; }

    public bool AllowDelayLoad { get; set; }

    public bool ImportsHKX { get; set; }

    public int ImportHKXSourceAnimID { get; set; }

    public int ImportFromAnimID { get; set; }

    public int Unknown { get; set; }

    public TemporaryAnimHeader() { }

    public TemporaryAnimHeader Clone()
    {
        return MemberwiseClone() as TemporaryAnimHeader;
    }
}
