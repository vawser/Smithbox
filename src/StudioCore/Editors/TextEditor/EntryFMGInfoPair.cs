using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.ParticleEditor.ParticleBank;

namespace StudioCore.Editors.TextEditor;

/// <summary>
///     Value pair with an entry and the FMG it belongs to.
/// </summary>
public class EntryFMGInfoPair
{
    public EntryFMGInfoPair(FMGInfo fmgInfo, FMG.Entry entry)
    {
        FmgInfo = fmgInfo;
        Entry = entry;
    }

    public FMGInfo FmgInfo { get; set; }
    public FMG.Entry Entry { get; set; }
}