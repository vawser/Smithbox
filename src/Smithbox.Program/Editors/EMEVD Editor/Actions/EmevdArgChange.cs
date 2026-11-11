using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Overwrites bytes of one instruction's args at a fixed length: an arg value edit, or a raw bytes edit of an
/// instruction the layouts can't decode. The instruction's length never changes, so the layout form it decoded
/// with still fits afterwards.
/// </summary>
public class EmevdArgChange : EditorAction, IEmevdEdit
{
    private readonly EmevdFileWrapper File;
    private readonly EMEVD.Instruction Instr;
    private readonly int Offset;
    private readonly byte[] OldBytes;
    private readonly byte[] NewBytes;

    /// <summary>Bytes past the end of the args are dropped</summary>
    public EmevdArgChange(EmevdFileWrapper file, EMEVD.Instruction instr, int offset, byte[] newBytes)
    {
        File = file;
        Instr = instr;
        Offset = offset;

        var current = instr.ArgData ?? Array.Empty<byte>();
        var count = Math.Max(0, Math.Min(newBytes.Length, current.Length - offset));

        NewBytes = newBytes[..count];
        OldBytes = count > 0 ? current[offset..(offset + count)] : Array.Empty<byte>();
    }

    /// <summary>False when nothing would change (same bytes, or the offset is outside the args)</summary>
    public bool HasEffect => NewBytes.Length > 0 && !NewBytes.AsSpan().SequenceEqual(OldBytes);

    public override ActionEvent Execute()
    {
        NewBytes.CopyTo(Instr.ArgData, Offset);
        File.MarkChanged();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        OldBytes.CopyTo(Instr.ArgData, Offset);
        File.MarkUndone();
        return ActionEvent.NoEvent;
    }
}
