using SoulsFormats;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// A .tae file within a source container. Only the bytes are held until it is first selected,
/// as a character can have well over a hundred of these.
/// </summary>
public class TimeActEntry
{
    public string Name { get; }
    public int BinderID { get; }

    /// <summary>
    /// The position of this file among the time acts of the binder, which is the category of its animations
    /// </summary>
    public int BindIndex { get; }

    public TAE TimeAct { get; private set; }

    public bool LoadFailed { get; private set; }

    /// <summary>
    /// Whether the time act has been modified since it was last saved
    /// </summary>
    public bool IsDirty { get; set; } = false;

    private Memory<byte> Data;

    // A character has hundreds of thousands of events, so they are only prepared once they are looked at
    private HashSet<TAE.Animation> TemplatedAnimations = new();

    public TimeActEntry(string name, int binderID, int bindIndex, Memory<byte> data)
    {
        Name = name;
        BinderID = binderID;
        BindIndex = bindIndex;
        Data = data;
    }

    public void Load()
    {
        if (TimeAct != null || LoadFailed)
            return;

        try
        {
            TimeAct = TAE.Read(Data);
        }
        catch (Exception e)
        {
            LoadFailed = true;

            Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_TimeAct", Name), e);
        }
    }

    /// <summary>
    /// Gets the ID that the animation's HKX is stored under. The animations of a player
    /// character only hold the lower half of it, as the upper half comes from the file.
    /// </summary>
    public long GetFullAnimationID(TAE.Animation animation, int categorySize)
    {
        return animation.ID >= categorySize ? animation.ID : BindIndex * categorySize + animation.ID;
    }

    /// <summary>
    /// Applies the template to the events of an animation. An event only takes on its template if the
    /// template can write the event back exactly: templates aren't complete, and the parameters
    /// that one has wrong or doesn't know of would otherwise be lost on save.
    /// </summary>
    public void ApplyTemplate(TAE.Animation animation, TAE.Template.BankTemplate template)
    {
        if (template == null || !TemplatedAnimations.Add(animation))
            return;

        var bigEndian = TimeAct.BigEndian;

        foreach (var ev in animation.Events)
        {
            if (ev.Template != null || !template.TryGetValue(ev.Type, out var eventTemplate))
                continue;

            if (IsExactFit(ev, eventTemplate))
            {
                ev.ApplyTemplate(bigEndian, eventTemplate, true);
            }
        }
    }

    private bool IsExactFit(TAE.Event ev, TAE.Template.EventTemplate eventTemplate)
    {
        var bigEndian = TimeAct.BigEndian;

        var original = ev.GetParameterBytes(bigEndian);

        try
        {
            var clone = ev.GetClone(bigEndian);
            clone.ApplyTemplate(bigEndian, eventTemplate, true);

            var written = clone.GetParameterBytes(bigEndian);

            // The lengths can differ by the padding that goes unused, which is always zero
            for (int i = 0; i < Math.Max(original.Length, written.Length); i++)
            {
                var originalByte = i < original.Length ? original[i] : 0;
                var writtenByte = i < written.Length ? written[i] : 0;

                if (originalByte != writtenByte)
                    return false;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
