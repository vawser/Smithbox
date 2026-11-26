using Andre.IO.VFS;
using StudioCore.Application;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Another project as a Material Merge source (Claude/Documentation/Materials and Textures.md §2, F11). Only the binders the project has in its
/// own folder are read: every other binder is the game's, so it has nothing to merge. Those binders are read whole,
/// so a vanilla material missing from one was deleted by that project.
/// </summary>
public static class MaterialMergeProject
{
    /// <summary>
    /// Reads the other project's own material binders. Run off the UI thread; loads the project as an auxiliary one
    /// (VFS and file list only) if it isn't loaded.
    /// </summary>
    public static async Task<MaterialMergeSource> Load(ProjectEntry target)
    {
        var name = target.Descriptor.ProjectName;

        await Smithbox.Orchestrator.LoadAuxiliaryProject(target, ProjectInitType.MaterialEditorOnly, false);

        if (target.VFS == null || target.Locator?.MTD_Files == null || target.Locator.MATBIN_Files == null)
            return MaterialMergeSource.Failed(name, "Project_Not_Loaded");

        var projectFs = target.VFS.ProjectFS;
        if (projectFs is EmptyVirtualFileSystem)
            return MaterialMergeSource.Failed(name, "No_Project_Folder", target.Descriptor.ProjectPath);

        var bank = new MaterialBank("Merge", target, target.VFS.FS);

        try
        {
            await bank.Setup(entry => projectFs.FileExists(entry.Path));

            var source = new MaterialMergeSource { Name = name };

            foreach (var binder in bank.BindersInLoadOrder())
            {
                source.CompleteBinders.Add(binder.FilePath);

                var unreadable = binder switch
                {
                    MTDWrapper w => w.UnreadableFiles,
                    MATBINWrapper w => w.UnreadableFiles,
                    _ => new List<string>()
                };

                foreach (var key in unreadable)
                    source.AddUnreadable(binder.FilePath, key);

                foreach (var key in binder.FileOrder.Distinct())
                {
                    var file = binder.GetFile(key);
                    if (file != null)
                        source.Candidates.Add(new MaterialMergeCandidate(binder.FilePath, key,
                            MaterialBank.ShortNameOf(key), MaterialVanilla.MaterialOf(file)));
                }
            }

            return source;
        }
        finally
        {
            // The candidates keep their materials; the merge copies them before anything is applied
            bank.Dispose();
        }
    }
}
