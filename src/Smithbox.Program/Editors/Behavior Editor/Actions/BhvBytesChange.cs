using StudioCore.Editors.Common;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// Overwrites bytes inside a raw blob (state body, transition params, condition data, animation record, preamble,
/// struct C/DA) at a fixed length: a known field edit or a hex edit. The blob is fetched through a getter that
/// closes over its owner, so a later resize (P3) that swaps the array doesn't leave this action writing to the old one.
/// </summary>
public class BhvBytesChange : EditorAction
{
    private readonly BhvFileWrapper File;
    private readonly Func<byte[]> Blob;
    private readonly int Offset;
    private readonly byte[] OldBytes;
    private readonly byte[] NewBytes;

    /// <summary>Bytes past the end of the blob are dropped; the blob never changes length.</summary>
    public BhvBytesChange(BhvFileWrapper file, Func<byte[]> blob, int offset, byte[] newBytes)
    {
        File = file;
        Blob = blob;
        Offset = offset;

        var current = blob();
        var count = Math.Max(0, Math.Min(newBytes.Length, current.Length - offset));

        NewBytes = newBytes[..count];
        OldBytes = current[offset..(offset + count)];
    }

    /// <summary>False when nothing would change (same bytes, or offset outside the blob)</summary>
    public bool HasEffect => NewBytes.Length > 0 && !NewBytes.AsSpan().SequenceEqual(OldBytes);

    public override ActionEvent Execute()
    {
        NewBytes.CopyTo(Blob(), Offset);
        File.MarkChanged();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        OldBytes.CopyTo(Blob(), Offset);
        File.MarkUndone();
        return ActionEvent.NoEvent;
    }
}
