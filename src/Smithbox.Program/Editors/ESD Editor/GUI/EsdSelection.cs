using SoulsFormats;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// What the ESD Editor has selected: a container, one of its ESDs, a machine, and a row of the state view (a state,
/// condition or command). Keeps a history for Back.
/// </summary>
public class EsdSelection
{
    private readonly EsdEditorView View;

    public EsdContainer SelectedContainer { get; private set; }
    public EsdFileWrapper SelectedFile { get; private set; }
    public long? SelectedMachine { get; private set; }

    /// <summary>The selected row's subject: an ESD.State, ESD.Condition or ESD.CommandCall (reference identity)</summary>
    public object SelectedItem { get; private set; }

    /// <summary>The state of the selected row</summary>
    public long SelectedState { get; private set; } = -1;

    public bool ScrollToItem;
    public bool ScrollToMachine;

    private readonly Stack<(EsdFileWrapper File, long Machine, long State)> History = new();

    public EsdSelection(EsdEditorView view)
    {
        View = view;
    }

    public Dictionary<long, ESD.State> Machine =>
        SelectedFile?.Esd != null && SelectedMachine is long id && SelectedFile.Esd.StateGroups.TryGetValue(id, out var m) ? m : null;

    public bool CanGoBack => History.Count > 0;

    public void SelectContainer(EsdContainer container)
    {
        if (SelectedContainer == container)
            return;

        SelectedContainer = container;
        container?.EnsureLoaded();

        // A loose file holds one ESD: select it straight away
        SelectFile(container != null && container.IsLoaded && container.Files.Count == 1 ? container.Files[0] : null);
    }

    public void SelectFile(EsdFileWrapper file)
    {
        if (file != null && file.Container != SelectedContainer)
        {
            SelectedContainer = file.Container;
            SelectedContainer.EnsureLoaded();
        }

        if (SelectedFile == file)
            return;

        SelectedFile = file;
        History.Clear();

        // The first entry machine (the lowest id that isn't a sub-machine), else the first machine
        long? first = null;
        if (file?.Esd != null && file.Esd.StateGroups.Count > 0)
        {
            var ids = file.Esd.StateGroups.Keys.OrderBy(k => k).ToList();
            first = ids.FirstOrDefault(k => !EsdLayout.IsSubMachine(k), ids[0]);
        }

        SetMachine(first);
    }

    public void SelectMachine(long id)
    {
        if (SelectedMachine == id)
            return;

        SetMachine(id);
    }

    private void SetMachine(long? id)
    {
        SelectedMachine = id;
        SelectedItem = null;
        SelectedState = -1;
        ScrollToMachine = true;
    }

    public void SelectRow(EsdRow row)
    {
        SelectedItem = row.Kind switch
        {
            EsdRowKind.State => row.State,
            EsdRowKind.Command => row.Command,
            _ => row.Condition
        };
        SelectedState = row.StateId;
    }

    public bool IsSelected(EsdRow row)
    {
        if (SelectedItem == null || row.StateId != SelectedState)
            return false;

        return row.Kind switch
        {
            EsdRowKind.State => ReferenceEquals(SelectedItem, row.State),
            EsdRowKind.Command => ReferenceEquals(SelectedItem, row.Command),
            EsdRowKind.Condition => ReferenceEquals(SelectedItem, row.Condition),
            _ => false
        };
    }

    /// <summary>Goes to a state of the selected machine (a transition's target), remembering where we were</summary>
    public void GoToState(long state)
    {
        if (Machine == null || !Machine.TryGetValue(state, out var s))
            return;

        Remember();
        SelectedItem = s;
        SelectedState = state;
        ScrollToItem = true;
    }

    /// <summary>Goes to another machine of the file (a bank 6 call's target), remembering where we were</summary>
    public void GoToMachine(long machine)
    {
        if (SelectedFile?.Esd == null || !SelectedFile.Esd.StateGroups.ContainsKey(machine))
            return;

        Remember();
        SetMachine(machine);
    }

    private void Remember()
    {
        if (SelectedFile != null && SelectedMachine is long m)
            History.Push((SelectedFile, m, SelectedState));
    }

    public void GoBack()
    {
        if (History.Count == 0)
            return;

        var (file, machine, state) = History.Pop();

        if (file != SelectedFile)
        {
            SelectedFile = file;
            SelectedContainer = file.Container;
        }

        SetMachine(machine);

        if (Machine != null && Machine.TryGetValue(state, out var s))
        {
            SelectedItem = s;
            SelectedState = state;
            ScrollToItem = true;
        }
    }
}
