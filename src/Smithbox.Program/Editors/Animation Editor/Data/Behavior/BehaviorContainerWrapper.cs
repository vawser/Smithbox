using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorContainerWrapper
{
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;

    public string Name { get; set; }
    public string Path { get; set; }

    public List<BehaviorWrapper> Entries { get; set; }

    public BehaviorContainerWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        Project = project;
        TargetFS = targetFS;
        Name = dictEntry.Filename;
        Path = dictEntry.Path;

        Entries = new();
    }

    // Loads the binder purely to fill the entry list up
    // Actual behavior data is loaded upon selection.
    public void PopulateEntryList()
    {
        Entries.Clear();

        // BND
        if (Project.Descriptor.ProjectType is ProjectType.DS3)
        {
            try
            {
                var fileData = TargetFS.ReadFile(Path);
                if (fileData != null)
                {
                    var binder = new BND4Reader(fileData.Value);
                    foreach (var file in binder.Files)
                    {
                        var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        var filepath = file.Name.ToLower();

                        // Need to explicitly check the internal folder,
                        // since <id>.hkx is repeated within the binder.
                        if (filepath.Contains("behaviors") && filepath.Contains(".hkx"))
                        {
                            var behWrapper = new BehaviorWrapper(this, filename);
                            Entries.Add(behWrapper);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Anim Editor] Failed to read {Path} during behavior load.", e);
            }
        }
    }
}
