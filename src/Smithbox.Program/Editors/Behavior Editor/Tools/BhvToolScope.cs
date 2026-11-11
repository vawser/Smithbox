using Hexa.NET.ImGui;

namespace StudioCore.Editors.BehaviorEditor;

public enum BhvScope { File, Binder, Modified, All }

/// <summary>
/// Which files a tool works on, and the radio buttons that pick it. Shared by Problems, Used By and Search.
/// </summary>
public class BhvToolScope
{
    private readonly BhvEditorView View;
    private readonly string Id;

    public BhvScope Scope;

    public BhvToolScope(BhvEditorView view, string id, BhvScope initial)
    {
        View = view;
        Id = id;
        Scope = initial;
    }

    /// <summary>The Scope group. Returns false (with the reason shown) when the scope needs a selection there isn't.</summary>
    public bool Draw(bool allowModified = true)
    {
        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("BHV_Tool_Scope_Header"), LOC.Get("BHV_Tool_Scope_Header_TT"));

        Radio(BhvScope.File, "BHV_Tool_Scope_File");
        Radio(BhvScope.Binder, "BHV_Tool_Scope_Binder");
        if (allowModified)
            Radio(BhvScope.Modified, "BHV_Tool_Scope_Modified");
        Radio(BhvScope.All, "BHV_Tool_Scope_All");

        if (!allowModified && Scope is BhvScope.Modified)
            Scope = BhvScope.All;

        if (Scope is BhvScope.File or BhvScope.Binder && View.Selection.SelectedFile == null)
        {
            GUI.WrappedText(LOC.Get("BHV_Tool_Scope_Needs_File"));
            return false;
        }

        return true;
    }

    private void Radio(BhvScope scope, string key)
    {
        if (ImGui.RadioButton($"{LOC.Get(key)}##{Id}_{scope}", Scope == scope))
            Scope = scope;
        GUI.Tooltip(LOC.Get($"{key}_TT"));
    }

    /// <summary>The files in scope, in bank order</summary>
    public List<BhvFileWrapper> Files()
    {
        var bank = View.Project.Handler.BehaviorData?.PrimaryBank;
        if (bank == null)
            return new();

        var selected = View.Selection.SelectedFile;

        return Scope switch
        {
            BhvScope.File => selected != null ? [selected] : [],
            BhvScope.Binder => selected?.Binder.Files.ToList() ?? [],
            BhvScope.Modified => bank.Binders.SelectMany(b => b.Files).Where(f => f.IsDirty).ToList(),
            _ => bank.Binders.SelectMany(b => b.Files).ToList(),
        };
    }
}
