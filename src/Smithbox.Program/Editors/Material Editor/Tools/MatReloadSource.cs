using StudioCore.Application;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Turns Material Editor files into what the Material Reloader sends to the game: the game's name for the material
/// and its MATBIN bytes. Shared by Reload in Game, Reload Modified, the history's re-send and Send Vanilla, and Save.
/// </summary>
public static class MatReloadSource
{
    /// <summary>
    /// The editor's copy of a material, including unsaved changes. Null (with an error logged) when the material has
    /// check errors, the same rule as saving, or can't be written.
    /// </summary>
    public static MatGameLink.SendItem FromEditor(MaterialBank bank, IMaterialBinder binder, MATBINFileWrapper file, MaterialEditorScreen editor)
    {
        var name = MatGameLink.GameNameOf(file.Key);

        var findings = MaterialEditorChecks.CheckFile(bank, binder, file, editor);
        if (findings.Any(f => f.Severity is MaterialCheckSeverity.Error))
        {
            Smithbox.LogError(typeof(MatReloadSource), LOC.Get("MAT_InGameTool_Reload_Has_Errors", name));
            return null;
        }

        return Write(name, file.MATBIN);
    }

    /// <summary>
    /// The vanilla copy of a material, for undoing edits in the game. Null when the vanilla materials aren't loaded
    /// yet or the material isn't vanilla (both logged).
    /// </summary>
    public static MatGameLink.SendItem FromVanilla(IMaterialBinder binder, MATBINFileWrapper file, MaterialEditorScreen editor)
    {
        var name = MatGameLink.GameNameOf(file.Key);

        editor.Vanilla.EnsureLoading();
        if (!editor.Vanilla.IsReady)
        {
            Smithbox.LogError(typeof(MatReloadSource), LOC.Get("MAT_InGameTool_Vanilla_Not_Ready"));
            return null;
        }

        if (editor.Vanilla.FindVanilla(binder, file.Key) is not MATBINFileWrapper vanilla)
        {
            Smithbox.LogError(typeof(MatReloadSource), LOC.Get("MAT_InGameTool_Vanilla_None", name));
            return null;
        }

        return Write(name, vanilla.MATBIN);
    }

    /// <summary>
    /// The editor's copy of a material found by the game's name, with its binder
    /// </summary>
    public static (IMaterialBinder Binder, MATBINFileWrapper File) Find(MaterialBank bank, string gameName)
    {
        var reference = bank?.FindReference(MaterialSourceType.MATBIN, gameName);
        if (reference?.File is not MATBINFileWrapper file || !bank.MATBINs.TryGetValue(reference.BinderEntry, out var binder))
            return (null, null);

        return (binder, file);
    }

    /// <summary>
    /// Every MATBIN with unsaved changes, ready to send; files with check errors are left out (and logged)
    /// </summary>
    public static List<MatGameLink.SendItem> Dirty(MaterialBank bank, MaterialEditorScreen editor)
    {
        var items = new List<MatGameLink.SendItem>();

        foreach (var binder in bank?.MATBINs.Values ?? Enumerable.Empty<MATBINWrapper>())
        {
            foreach (var file in binder.GetFiles().OfType<MATBINFileWrapper>().Where(f => f.IsDirty))
            {
                var item = FromEditor(bank, binder, file, editor);
                if (item != null)
                    items.Add(item);
            }
        }

        return items;
    }

    /// <summary>
    /// How many MATBINs have unsaved changes
    /// </summary>
    public static int CountDirty(MaterialBank bank) =>
        bank?.MATBINs.Values.Sum(b => b.GetFiles().Count(f => f.IsDirty)) ?? 0;

    private static MatGameLink.SendItem Write(string name, SoulsFormats.MATBIN matbin)
    {
        try
        {
            return new MatGameLink.SendItem(name, matbin.Write());
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(MatReloadSource), LOC.Get("MAT_InGameTool_Reload_Write_Failed", name), e);
            return null;
        }
    }
}
