using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Utils;

public class TimeActSearchResult
{
    public string TimeActName;
    public TAE ResultTAE;
    public TAE.Animation ResultAnim;
    public TAE.Event ResultEvent;

    public string FileKey = "";
    public string TimeActKey = "";

    public int AnimationIndex = -1;
    public int EventIndex = -1;
    public string EventPropertyValue = "";

    public TimeActSearchResult(string taeName, TAE tae, TAE.Animation anim, TAE.Event evt = null)
    {
        TimeActName = taeName;
        ResultTAE = tae;
        ResultAnim = anim;
        ResultEvent = evt;
    }
}