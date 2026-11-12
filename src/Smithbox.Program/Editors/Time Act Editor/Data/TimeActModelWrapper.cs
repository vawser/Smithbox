using Havok.Shared;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActModelWrapper
{
    public TimeActContainerWrapper Parent { get; set; }

    public string Name { get; set; }

    public TimeActContainer Container { get; set; }

    public TimeActModelWrapper(TimeActContainerWrapper parent, string name)
    {
        Parent = parent;
        Name = name;
    }

    // FLVER
    public FLVER2 FLVER { get; set; }

    /// <summary>
    /// Whether the model has been modified since it was last loaded or saved
    /// </summary>
    public bool IsDirty { get; set; } = false;

    /// <summary>
    /// Loads the model into the view that opened its source, which isn't always the active one
    /// </summary>
    public void Load(TimeActEditorView view)
    {
        // Keep the unsaved changes instead of replacing them with the stored version
        if (!IsDirty || FLVER == null)
        {
            LoadFromStorage();
        }

        if (FLVER != null)
        {
            view?.Universe.LoadModel(this);
        }
    }

    private void LoadFromStorage()
    {
        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Parent.Project, Parent.Path);

        if (binderType is ResourceContainerType.None)
        {
            LoadDirect();
            return;
        }

        try
        {
            using var binder = Parent.ReadContainer();

            if (binder == null)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Parent.Path));
                return;
            }

            foreach (var file in binder.Files)
            {
                var filename = FilePathUtils.GetPureFilename(file.Name);
                var filepath = file.Name.ToLower();

                ReadFile(filepath, filename, file.Bytes);
            }

            // DES binders can name their model differently (am_f_0000_l holds AM_F_0000.flver, hd_f_9000 HD_F_0000.flver)
            FLVER ??= Flver0Converter.ReadOnlyFlver(Parent.Project, binder);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Parent.Path), e);
        }
    }

    public void LoadDirect()
    {
        var fileData = Parent.TargetFS.ReadFile(Parent.Path);
        if (fileData != null)
        {
            FLVER = Flver0Converter.ReadEditorFlver(Parent.Project, fileData.Value);
        }
    }

    public void ReadFile(string filepath, string filename, Memory<byte> data)
    {
        // FLVER
        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
        {
            if (string.Equals(filename, Name, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    FLVER = Flver0Converter.ReadEditorFlver(Parent.Project, data);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_FLVER", filepath), e);
                }
            }
        }
    }

    /// <summary>
    /// Unloads the model from the view it was loaded into. The universe drops that view's actions on the model,
    /// and keeps its time act edits.
    /// </summary>
    public void Unload(TimeActEditorView view)
    {
        // Store the unsaved changes in the FLVER, as the container holding them is discarded on unload
        if (IsDirty && Container != null)
        {
            UpdateFLVER();
        }

        view?.Universe.UnloadModel(this);
    }

    public void UpdateFLVER()
    {
        // Dummies
        FLVER.Dummies.Clear();
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER.Dummy entDummy)
            {
                FLVER.Dummies.Add(entDummy);
            }
        }

        // Nodes
        FLVER.Nodes.Clear();
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER.Node entNode)
            {
                FLVER.Nodes.Add(entNode);
            }
        }

        // Meshes
        FLVER.Meshes.Clear();
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER2.Mesh entMesh)
            {
                FLVER.Meshes.Add(entMesh);
            }
        }
    }
}