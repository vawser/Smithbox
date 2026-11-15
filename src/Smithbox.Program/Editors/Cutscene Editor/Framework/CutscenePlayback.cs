using System;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The clock of the selected cut, in frames of the MQB (30 per second). The timeline, the actors and the camera all
/// read the frame from here.
/// </summary>
public class CutscenePlayback
{
    public float Frame;
    public bool Playing;

    /// <summary>Set when the clock ran past the end of the cut and should move on to the next one</summary>
    public bool ReachedEnd { get; private set; }

    public void Restart()
    {
        Frame = 0;
        ReachedEnd = false;
    }

    public void Update(float dt, float framerate, int frameCount)
    {
        ReachedEnd = false;
        var last = Math.Max(0, frameCount - 1);

        if (Playing)
        {
            Frame += dt * framerate * CFG.Current.CutsceneEditor_Playback_Speed;

            if (Frame > last)
            {
                if (CFG.Current.CutsceneEditor_Continue_To_Next_Cut)
                {
                    ReachedEnd = true;
                    Frame = last;
                }
                else if (CFG.Current.CutsceneEditor_Loop)
                {
                    Frame = last > 0 ? Frame % last : 0;
                }
                else
                {
                    Frame = last;
                    Playing = false;
                }
            }
        }

        Frame = Math.Clamp(Frame, 0, last);
    }
}
