using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.TAE.Animation;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Wrapper class that holds AnimMiniHeader properties for committing.
/// This is done so the user can switch between the anim header types without losing data.
/// </summary>
public class TransientAnimHeader
{
    public MiniHeaderType CurrentType { get; set; }

    public bool IsLoopByDefault { get; set; }

    public bool AllowDelayLoad { get; set; }

    public bool ImportsHKX { get; set; }

    public int ImportHKXSourceAnimID { get; set; }

    public int ImportFromAnimID { get; set; }

    public int Unknown { get; set; }

    public TransientAnimHeader() { }

    public TransientAnimHeader Clone()
    {
        return MemberwiseClone() as TransientAnimHeader;
    }
}
