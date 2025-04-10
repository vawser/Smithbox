using SoulsFormats;

namespace StudioCore.Editors.TimeActEditor.Utils;

public class TimeActSearchResult
{
    public string TimeActName;
    public TAE ResultTAE;
    public TAE.Animation ResultAnim;
    public TAE.Event ResultEvent;

    public int ContainerIndex = -1;
    public int TimeActIndex = -1;
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