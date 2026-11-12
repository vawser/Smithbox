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
    public string Name { get; private set; }
    public int BinderID { get; private set; }

    /// <summary>
    /// The position of this file among the time acts of the binder, which is the category of its animations
    /// </summary>
    public int BindIndex { get; private set; }

    public TAE TimeAct { get; private set; }

    public bool LoadFailed { get; private set; }

    /// <summary>
    /// Whether the time act has been modified since it was last saved
    /// </summary>
    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            _isDirty = value;
            EditVersion++;

            if (value)
            {
                HasBeenEdited = true;
            }
        }
    }

    private bool _isDirty = false;

    /// <summary>
    /// Raised by every edit to any time act, as each one marks its time act as modified. The comparisons with the
    /// game's version are kept until this changes.
    /// </summary>
    public static int EditVersion { get; private set; }

    /// <summary>
    /// Whether the time act has been changed at any point this session, saved or not. The undo history can
    /// still point at it, so it is kept rather than read again from the file.
    /// </summary>
    public bool HasBeenEdited { get; private set; } = false;

    /// <summary>
    /// The full name, flags and compression of the binder file, so a copy can be added alongside it
    /// </summary>
    public string BinderFileName { get; private set; }
    public Binder.FileFlags BinderFlags { get; }
    public DCX.Type CompressionType { get; }

    private Memory<byte> Data;

    // A character has hundreds of thousands of events, so they are only prepared once they are looked at
    private HashSet<TAE.Animation> TemplatedAnimations = new();

    public TimeActEntry(string name, int binderID, int bindIndex, Memory<byte> data,
        string binderFileName, Binder.FileFlags binderFlags, DCX.Type compressionType)
    {
        Name = name;
        BinderID = binderID;
        BindIndex = bindIndex;
        Data = data;
        BinderFileName = binderFileName;
        BinderFlags = binderFlags;
        CompressionType = compressionType;
    }

    /// <summary>
    /// Gets the bytes of the .tae file, including any changes that are yet to be saved.
    /// The stored bytes are used otherwise, as they are exact.
    /// </summary>
    public byte[] GetBytes()
    {
        if (IsDirty && TimeAct != null)
            return TimeAct.Write();

        return Data.ToArray();
    }

    /// <summary>
    /// Called once the bytes have been written to the binder, so they become the stored version
    /// </summary>
    public void MarkSaved(byte[] bytes)
    {
        Data = bytes;
        IsDirty = false;
    }

    /// <summary>
    /// Creates a copy of this time act under another category. The copy is marked as modified, so it is written on save.
    /// </summary>
    public TimeActEntry Duplicate(string name, int bindIndex)
    {
        return CreateSibling(name, bindIndex, GetBytes());
    }

    /// <summary>
    /// Creates a time act alongside this one in the binder, sharing its folder, flags and compression
    /// </summary>
    public TimeActEntry CreateSibling(string name, int bindIndex, Memory<byte> data)
    {
        var entry = new TimeActEntry(name, BinderID - BindIndex + bindIndex, bindIndex, data,
            GetBinderFileName(name), BinderFlags, CompressionType);

        entry.IsDirty = true;

        return entry;
    }

    /// <summary>
    /// Moves the time act to another category. The binder file is renamed when the source is saved.
    /// </summary>
    public void Rename(string name, int bindIndex)
    {
        BinderFileName = GetBinderFileName(name);
        BinderID = BinderID - BindIndex + bindIndex;
        BindIndex = bindIndex;
        Name = name;

        // Written again under its new ID, as the file of the old ID is removed
        IsDirty = true;
    }

    /// <summary>
    /// The binder path of a time act with another name, in the same folder as this one
    /// </summary>
    private string GetBinderFileName(string name)
    {
        if (BinderFileName == null)
            return $"{name}.tae";

        var filename = System.IO.Path.GetFileName(BinderFileName);

        return BinderFileName.Substring(0, BinderFileName.Length - filename.Length)
            + $"{name}{System.IO.Path.GetExtension(BinderFileName)}";
    }

    /// <summary>
    /// Raised whenever an animation changes its ID, so lists built from the animations know to rebuild
    /// </summary>
    public int AnimationsVersion { get; private set; } = 0;

    /// <summary>
    /// Puts an animation back in ID order after its ID has changed, as the animations are stored sorted
    /// </summary>
    public void ResortAnimation(TAE.Animation animation)
    {
        var animations = TimeAct.Animations;

        animations.Remove(animation);

        var index = animations.FindIndex(e => e.ID > animation.ID);

        animations.Insert(index < 0 ? animations.Count : index, animation);

        AnimationsVersion++;
    }

    /// <summary>
    /// Called whenever animations are added, removed or reordered, so lists built from them know to rebuild
    /// </summary>
    public void MarkAnimationsChanged()
    {
        AnimationsVersion++;
        EditVersion++;
    }

    /// <summary>
    /// Whether the stored bytes of the two time acts are the same. These are the bytes last read or saved, so this
    /// only says the time acts match while neither has unsaved changes.
    /// </summary>
    public bool StoredBytesEqual(TimeActEntry other)
    {
        return Data.Span.SequenceEqual(other.Data.Span);
    }

    /// <summary>
    /// Puts another time act in place of this one's, keeping the entry, such as when reverting it to the game's
    /// version. Its events take on their templates once their animation is selected.
    /// </summary>
    public void ReplaceTimeAct(TAE timeAct)
    {
        TimeAct = timeAct;

        IsDirty = true;
        MarkAnimationsChanged();
    }

    /// <summary>
    /// Lets the template be applied to the animation again, once its events have been swapped for ones without it
    /// </summary>
    public void ForgetTemplate(TAE.Animation animation)
    {
        TemplatedAnimations.Remove(animation);
    }

    /// <summary>
    /// Applies the template to the event if it writes the event back exactly, as when the time act is first read
    /// </summary>
    public bool TryApplyTemplateExactly(TAE.Event ev, TAE.Template.EventTemplate eventTemplate)
    {
        if (ev.Template != null || eventTemplate == null || !IsExactFit(ev, eventTemplate))
            return false;

        ev.ApplyTemplate(TimeAct.BigEndian, eventTemplate, true);

        return true;
    }

    /// <summary>
    /// Gets the first animation ID from the given one onwards that no animation of this time act uses
    /// </summary>
    public long GetFreeAnimationID(long start)
    {
        var id = Math.Max(start, 0);

        var used = TimeAct.Animations.Select(e => e.ID).ToHashSet();

        while (used.Contains(id))
        {
            id++;
        }

        return id;
    }

    /// <summary>
    /// Creates a copy of an animation under another ID. The events are copied along with the groups they belong to.
    /// </summary>
    public TAE.Animation CloneAnimation(TAE.Animation animation, long id, string animFileName)
    {
        var clone = CloneAnimation(animation, id, animFileName, TimeAct.BigEndian, true);

        // The events are copied with their templates, so the copy is ready to edit
        TemplatedAnimations.Add(clone);

        return clone;
    }

    /// <summary>
    /// Creates a copy of an animation that belongs to no time act. Without templates, the events keep only their
    /// bytes, so the copy can go into a time act whose template differs: that template is applied once it is selected.
    /// </summary>
    public static TAE.Animation CloneAnimation(TAE.Animation animation, long id, string animFileName, bool bigEndian,
        bool keepTemplates)
    {
        var clone = new TAE.Animation(id, animation.MiniHeader?.GetClone(), animFileName);

        var groups = new Dictionary<TAE.EventGroup, TAE.EventGroup>();

        foreach (var group in animation.EventGroups)
        {
            var groupClone = group.GetClone();

            groups[group] = groupClone;
            clone.EventGroups.Add(groupClone);
        }

        foreach (var ev in animation.Events)
        {
            var eventClone = keepTemplates
                ? ev.GetClone(bigEndian)
                : new TAE.Event(ev.StartTime, ev.EndTime, ev.Type, ev.Unk04, ev.GetParameterBytes(bigEndian), bigEndian);

            if (ev.Group != null && groups.TryGetValue(ev.Group, out var groupClone))
            {
                eventClone.Group = groupClone;
            }

            clone.Events.Add(eventClone);
        }

        return clone;
    }

    // The search reads time acts in the background, while the editor can be reading the same one
    private readonly object LoadLock = new();

    public void Load()
    {
        if (TimeAct != null || LoadFailed)
            return;

        lock (LoadLock)
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
    }

    /// <summary>
    /// Gets the ID that the animation's HKX is stored under. The animations of a player
    /// character only hold the lower half of it, as the upper half comes from the file.
    /// </summary>
    public long GetFullAnimationID(TAE.Animation animation, int categorySize)
    {
        return GetFullAnimationID(animation.ID, categorySize);
    }

    public long GetFullAnimationID(long animationID, int categorySize)
    {
        return animationID >= categorySize ? animationID : (long)BindIndex * categorySize + animationID;
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
        return TryCreateTemplatedClone(ev, eventTemplate, out _);
    }

    /// <summary>
    /// Gets an event with its parameters filled in, for reading them without changing the time act:
    /// the event itself if it already has them, or else a copy with the template applied if it fits exactly.
    /// Safe to call off the main thread, as the event is only read.
    /// </summary>
    public TAE.Event GetReadableEvent(TAE.Event ev, TAE.Template.BankTemplate template)
    {
        if (ev.Parameters != null || template == null || !template.TryGetValue(ev.Type, out var eventTemplate))
            return ev;

        return TryCreateTemplatedClone(ev, eventTemplate, out var clone) ? clone : ev;
    }

    /// <summary>
    /// Why an event without a template on it has none: no template for its type, or the first byte the template
    /// would write back differently, or the template failing to read it. Null if the template would fit after all.
    /// </summary>
    public string DescribeTemplateMismatch(TAE.Event ev, TAE.Template.BankTemplate template)
    {
        if (template == null || !template.TryGetValue(ev.Type, out var eventTemplate))
            return LOC.Get("TAE_TemplateFit_No_Template", ev.Type);

        var bigEndian = TimeAct.BigEndian;

        try
        {
            var original = ev.GetParameterBytes(bigEndian);

            var copy = ev.GetClone(bigEndian);
            copy.ApplyTemplate(bigEndian, eventTemplate, true);

            var written = copy.GetParameterBytes(bigEndian);

            for (int i = 0; i < Math.Max(original.Length, written.Length); i++)
            {
                var originalByte = i < original.Length ? original[i] : 0;
                var writtenByte = i < written.Length ? written[i] : 0;

                if (originalByte != writtenByte)
                {
                    return LOC.Get("TAE_TemplateFit_Mismatch", eventTemplate.Name, $"0x{i:X}", $"{originalByte:X2}",
                        $"{writtenByte:X2}", original.Length, eventTemplate.GetAllParametersByteCount());
                }
            }

            return null;
        }
        catch (Exception e)
        {
            return LOC.Get("TAE_TemplateFit_Failed", eventTemplate.Name, e.Message.Split('\n')[0]);
        }
    }

    /// <summary>
    /// A byte that putting a template on an event anyway would change. Old is null past the end of the event's
    /// data, and New is null past the end of the template, where the byte would be dropped.
    /// </summary>
    public record TemplateByteChange(int Offset, byte? Old, byte? New, string Field, bool IsFixedValue);

    /// <summary>
    /// Works out what putting the template on the event anyway would change, on a copy. The template reads past
    /// the values it would assert, and writes its own fixed value there. False if it can't read the event at all.
    /// </summary>
    public bool TryPreviewForcedTemplate(TAE.Event ev, TAE.Template.EventTemplate eventTemplate,
        out List<TemplateByteChange> changes, out string error)
    {
        changes = new List<TemplateByteChange>();
        error = null;

        var bigEndian = TimeAct.BigEndian;
        byte[] original;
        byte[] written;

        try
        {
            original = ev.GetParameterBytes(bigEndian).ToArray();

            var copy = ev.GetClone(bigEndian);
            copy.ApplyTemplate(bigEndian, eventTemplate, true);

            written = copy.GetParameterBytes(bigEndian);
        }
        catch (Exception e)
        {
            error = e.Message.Split('\n')[0];
            return false;
        }

        // Which field of the template each byte belongs to, in the order the template writes them
        var fields = new List<(int Start, int End, string Name, bool Fixed)>();
        var offset = 0;
        var index = 0;

        foreach (var parameter in eventTemplate.Values)
        {
            var size = parameter.GetByteCount();

            fields.Add((offset, offset + size, parameter.Name ?? $"#{index}", parameter.ValueToAssert != null));

            offset += size;
            index++;
        }

        for (var i = 0; i < Math.Max(original.Length, written.Length); i++)
        {
            byte? oldByte = i < original.Length ? original[i] : null;
            byte? newByte = i < written.Length ? written[i] : null;

            // Zero padding that comes or goes changes nothing the game reads
            if ((oldByte ?? 0) == (newByte ?? 0))
                continue;

            var field = fields.FirstOrDefault(e => i >= e.Start && i < e.End);

            changes.Add(new TemplateByteChange(i, oldByte, newByte, field.Name, field.Fixed));
        }

        return true;
    }

    /// <summary>
    /// Copies the event and applies the template to the copy, which is kept only if it writes back the same bytes
    /// </summary>
    private bool TryCreateTemplatedClone(TAE.Event ev, TAE.Template.EventTemplate eventTemplate, out TAE.Event clone)
    {
        clone = null;

        var bigEndian = TimeAct.BigEndian;

        try
        {
            var original = ev.GetParameterBytes(bigEndian);

            var copy = ev.GetClone(bigEndian);
            copy.ApplyTemplate(bigEndian, eventTemplate, true);

            var written = copy.GetParameterBytes(bigEndian);

            // The lengths can differ by the padding that goes unused, which is always zero
            for (int i = 0; i < Math.Max(original.Length, written.Length); i++)
            {
                var originalByte = i < original.Length ? original[i] : 0;
                var writtenByte = i < written.Length ? written[i] : 0;

                if (originalByte != writtenByte)
                    return false;
            }

            clone = copy;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
