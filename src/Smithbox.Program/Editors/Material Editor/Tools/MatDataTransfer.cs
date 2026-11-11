using SoulsFormats;
using StudioCore.Utilities;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Export, import and the clipboard for materials (Material Editor plan, F9), as <see cref="MaterialJson"/>.
/// Every vanilla material of the surveyed games comes back from JSON with every field the same (survey json mode).
/// </summary>
public static class MatDataTransfer
{
    private static readonly string[] Filters = ["json"];

    private static string OutputFolder(ProjectEntry project) => Path.Join(project.Descriptor.ProjectPath, "output");

    public static void CopyToClipboard(MaterialFileWrapper file)
    {
        var json = MaterialJson.From(MaterialBank.ShortNameOf(file.Key), MaterialVanilla.MaterialOf(file));
        PlatformUtils.Instance.SetClipboardText(json.ToText());
        Smithbox.Log(typeof(MatDataTransfer), LOC.Get("MAT_Transfer_Copied", MaterialBank.ShortNameOf(file.Key)));
    }

    public static void Export(ProjectEntry project, MaterialFileWrapper file)
    {
        if (!PlatformUtils.Instance.SaveFileDialog(LOC.Get("MAT_Transfer_Export"), Filters, OutputFolder(project), out var path) ||
            string.IsNullOrEmpty(path))
            return;

        if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            path += ".json";

        try
        {
            var json = MaterialJson.From(MaterialBank.ShortNameOf(file.Key), MaterialVanilla.MaterialOf(file));
            File.WriteAllText(path, json.ToText());
            Smithbox.Log(typeof(MatDataTransfer), LOC.Get("MAT_Transfer_Exported", path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(MatDataTransfer), LOC.Get("MAT_Transfer_Export_Failed", path), e);
        }
    }

    /// <summary>
    /// Replaces the material's contents with the material on the clipboard, as one undo step
    /// </summary>
    public static void PasteOver(MaterialEditorView view, IMaterialBinder binder, MaterialFileWrapper file)
    {
        var material = Read(PlatformUtils.Instance.GetClipboardText(), binder.SourceType, out _);
        if (material == null)
            return;

        view.ActionManager.ExecuteAction(new MatRevertMaterial(file, MaterialVanilla.MaterialOf(file), material));
        Smithbox.Log(typeof(MatDataTransfer), LOC.Get("MAT_Transfer_Pasted", MaterialBank.ShortNameOf(file.Key)));
    }

    /// <summary>
    /// Adds the material of a JSON file to the binder, after <paramref name="near"/> and in its folder, under the
    /// name the file gives (numbered if taken)
    /// </summary>
    public static void ImportNew(MaterialEditorView view, IMaterialBinder binder, MaterialFileWrapper near)
    {
        if (!PlatformUtils.Instance.OpenFileDialog(LOC.Get("MAT_Transfer_Import"), Filters, OutputFolder(view.Project), out var path) ||
            string.IsNullOrEmpty(path))
            return;

        string text;
        try
        {
            text = File.ReadAllText(path);
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(MatDataTransfer), LOC.Get("MAT_Transfer_Import_Failed", path), e);
            return;
        }

        var material = Read(text, binder.SourceType, out var json);
        if (material == null)
            return;

        var bank = view.Project.Handler.MaterialData.PrimaryBank;

        // The file's own name, or numbered until free
        var wanted = string.IsNullOrWhiteSpace(json.Name) ? "Imported" : json.Name.Trim();
        var name = wanted;
        for (var n = 2; bank.IsShortNameTaken(binder.SourceType, name); n++)
            name = $"{wanted}_{n}";

        // A new file of the binder's kind, its contents then set from the import
        var file = binder.CreateCopy(near, MaterialBank.WithShortName(near.Key, name));
        CopyInto(MaterialVanilla.MaterialOf(file), material);

        // A MATBIN's source path names it, so it follows a name that had to change
        if (name != wanted && MaterialVanilla.MaterialOf(file) is MATBIN matbin &&
            string.Equals(MaterialBank.ShortNameOf(matbin.SourcePath ?? ""), wanted, StringComparison.OrdinalIgnoreCase))
        {
            matbin.SourcePath = MaterialBank.WithShortName(matbin.SourcePath, name);
            MaterialKeys.Apply(matbin);
        }

        var index = binder.FileOrder.ToList().IndexOf(near.Key) + 1;
        view.ActionManager.ExecuteAction(new MatInsertFile(view.Editor, bank, binder, file, index));
        view.Selection.SelectFile(file);

        Smithbox.Log(typeof(MatDataTransfer), LOC.Get("MAT_Transfer_Imported", name, binder.Name));
    }

    /// <summary>
    /// A material from JSON text, checked to be of the binder's kind. Logs why not, and returns null.
    /// </summary>
    private static object Read(string text, MaterialSourceType kind, out MaterialJson json)
    {
        json = MaterialJson.FromText(text);

        if (json == null)
        {
            Smithbox.LogError(typeof(MatDataTransfer), LOC.Get("MAT_Transfer_Not_Material"));
            return null;
        }

        if (json.Kind != kind.ToString())
        {
            Smithbox.LogError(typeof(MatDataTransfer), LOC.Get("MAT_Transfer_Wrong_Kind", json.Kind, kind));
            return null;
        }

        var material = json.ToMaterial(out var error);
        if (material == null)
            Smithbox.LogError(typeof(MatDataTransfer), LOC.Get("MAT_Transfer_Bad_Param", error));

        return material;
    }

    /// <summary>
    /// Sets a material's fields and lists to another's, outside the undo history (for a file not yet in a binder)
    /// </summary>
    private static void CopyInto(object target, object source)
    {
        switch (target, source)
        {
            case (MTD t, MTD s):
                t.ShaderPath = s.ShaderPath;
                t.Description = s.Description;
                t.Params = s.Params;
                t.Textures = s.Textures;
                break;

            case (MATBIN t, MATBIN s):
                t.ShaderPath = s.ShaderPath;
                t.SourcePath = s.SourcePath;
                t.Key = s.Key;
                t.Params = s.Params;
                t.Samplers = s.Samplers;
                break;
        }
    }
}
