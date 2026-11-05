using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// An action that changes a binder's file list or stored files (paste, delete, rename, insert, compression)
/// </summary>
public interface IHavokBinderAction
{
    FileDictionaryEntry BinderEntry { get; }

    /// <summary>
    /// The wrappers the action can put back into the binder, which must stay as they are
    /// </summary>
    IEnumerable<HavokFileWrapper> Wrappers { get; }
}

/// <summary>
/// The recorded state of a binder's file list: each file object with the values a file action can change,
/// and the wrapper that holds its editing state
/// </summary>
public class HavokBinderSnapshot
{
    public record FileState(
        BinderFile File,
        HavokFileWrapper Wrapper,
        string Name,
        Memory<byte> Bytes,
        int ID,
        Binder.FileFlags Flags,
        DCX.Type CompressionType);

    public List<FileState> Files = new();
}

/// <summary>
/// A file action staged on the binder's in-memory copy. The first Execute runs the change and records the
/// binder before and after; Undo and Redo put the recorded states back. Nothing is written until Save.
/// </summary>
public class HavokBinderAction : EditorAction, IHavokBinderAction
{
    private readonly HavokBank Bank;
    private readonly Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> BankDict;
    private readonly IBinder Binder;
    private readonly Func<IBinder, bool> Modify;

    private HavokBinderSnapshot Before;
    private HavokBinderSnapshot After;

    /// <summary>
    /// The files whose stored bytes the change replaced (compression), so undo/redo restores their bytes too
    /// </summary>
    private readonly HashSet<BinderFile> ChangedBytes = new(ReferenceEqualityComparer.Instance);

    public FileDictionaryEntry BinderEntry { get; }

    public IEnumerable<HavokFileWrapper> Wrappers =>
        (Before?.Files ?? new()).Concat(After?.Files ?? new()).Select(e => e.Wrapper);

    public HavokBinderAction(HavokBank bank,
        Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict,
        FileDictionaryEntry binderEntry, IBinder binder, Func<IBinder, bool> modify)
    {
        Bank = bank;
        BankDict = bankDict;
        BinderEntry = binderEntry;
        Binder = binder;
        Modify = modify;
    }

    public override ActionEvent Execute()
    {
        Bank.RunBinderChange(BinderEntry, () =>
        {
            if (After == null)
            {
                // First run: make the change and record the states either side of it
                Before = Bank.CaptureBinder(BankDict, BinderEntry, Binder);

                Modify(Binder);

                Bank.SyncWrappers(BankDict, BinderEntry, Binder, Before);
                After = Bank.CaptureBinder(BankDict, BinderEntry, Binder);

                var beforeBytes = Before.Files.ToDictionary(e => e.File, e => e.Bytes, ReferenceEqualityComparer.Instance);

                foreach (var state in After.Files)
                {
                    if (beforeBytes.TryGetValue(state.File, out var bytes) && !bytes.Equals(state.Bytes))
                    {
                        ChangedBytes.Add(state.File);
                    }
                }
            }
            else
            {
                Bank.RestoreBinder(BankDict, BinderEntry, Binder, After, ChangedBytes);
            }
        });

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Bank.RunBinderChange(BinderEntry, () =>
        {
            Bank.RestoreBinder(BankDict, BinderEntry, Binder, Before, ChangedBytes);
        });

        return ActionEvent.NoEvent;
    }
}
