using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public enum TransferMissingBone
{
    /// <summary>A dummy whose bone the target lacks isn't added</summary>
    Skip,

    /// <summary>It is added at the top level (no parent or attach bone)</summary>
    TopLevel
}

public enum TransferMaterialMode
{
    /// <summary>Every material is added, renamed when the name is taken</summary>
    AddRenamed,

    /// <summary>A material whose name the target has replaces that one's MTD, textures and GX list</summary>
    ReplaceSameName
}

public sealed class TransferOptions
{
    public TransferMissingBone MissingBone = TransferMissingBone.Skip;
    public int RefIdOffset;
    public TransferMaterialMode MaterialMode = TransferMaterialMode.AddRenamed;
    public bool CopyGx = true;
}

/// <summary>What an import did, or would do</summary>
public sealed class TransferReport
{
    public int DummiesAdded;
    public int MaterialsAdded;
    public int MaterialsReplaced;
    public int GxListsAdded;

    public List<string> SkippedDummies = new();
    public HashSet<string> MissingBones = new();
}

/// <summary>
/// Data Transfer: dummies and materials out of one model (<see cref="Extract" />) into the format of
/// <see cref="ModelTransferFile" />, and from it into the loaded model as one undo step (<see cref="BuildImport" />).
/// The same format is written to and read from JSON. Plan: Documentation/Model Editor Second Review.md, R9.
/// </summary>
public static class ModelDataTransfer
{
    #region Extract
    public static ModelTransferFile Extract(FLVER2 flver, IEnumerable<FLVER.Dummy> dummies,
        IEnumerable<FLVER2.Material> materials, string sourceModel, string game)
    {
        string BoneName(int index) => index >= 0 && index < flver.Nodes.Count ? flver.Nodes[index].Name : null;

        var file = new ModelTransferFile { Game = game, SourceModel = sourceModel };

        foreach (var d in dummies)
        {
            file.Dummies.Add(new ModelTransferDummy
            {
                ReferenceID = d.ReferenceID,
                Position = ToArray(d.Position),
                Forward = ToArray(d.Forward),
                Upward = ToArray(d.Upward),
                ParentBone = BoneName(d.ParentBoneIndex),
                AttachBone = BoneName(d.AttachBoneIndex),
                Color = new int[] { d.Color.A, d.Color.R, d.Color.G, d.Color.B },
                FollowAttachBone = d.FollowAttachBone,
                UseUpwardVector = d.UseUpwardVector,
                Unk30 = d.Unk30,
                Unk34 = d.Unk34
            });
        }

        foreach (var m in materials)
        {
            var material = new ModelTransferMaterial
            {
                Name = m.Name,
                MTD = m.MTD,
                Textures = m.Textures.Select(t => new ModelTransferTexture
                {
                    ParamName = t.ParamName,
                    Path = t.Path,
                    TilingScale = new[] { t.TilingScale.X, t.TilingScale.Y },
                    TilingTypeU = (int)t.TilingTypeU,
                    TilingTypeV = (int)t.TilingTypeV,
                    Unk14 = t.Unk14,
                    Unk18 = t.Unk18,
                    Unk1C = t.Unk1C
                }).ToList()
            };

            if (m.GXIndex >= 0 && m.GXIndex < flver.GXLists.Count)
            {
                var gx = flver.GXLists[m.GXIndex];

                material.GX = new ModelTransferGxList
                {
                    TerminatorID = gx.TerminatorID,
                    TerminatorLength = gx.TerminatorLength,
                    Items = gx.Select(i => new ModelTransferGxItem
                    {
                        ID = i.ID,
                        Unk04 = i.Unk04,
                        Data = Convert.ToBase64String(i.Data ?? Array.Empty<byte>())
                    }).ToList()
                };
            }

            file.Materials.Add(material);
        }

        return file;
    }

    public static string ToJson(ModelTransferFile file)
    {
        return System.Text.Json.JsonSerializer.Serialize(file, ModelEditorJsonSerializerContext.Default.ModelTransferFile);
    }

    /// <summary>Null, with the reason, when the text isn't a Data Transfer file</summary>
    public static ModelTransferFile FromJson(string text, out string error)
    {
        error = null;

        try
        {
            var file = System.Text.Json.JsonSerializer.Deserialize(text, ModelEditorJsonSerializerContext.Default.ModelTransferFile);

            if (file == null || file.Format != "SmithboxModelData")
            {
                error = LOC.Get("MODEL_Transfer_Not_Transfer_File");
                return null;
            }

            file.Dummies ??= new();
            file.Materials ??= new();

            foreach (var m in file.Materials)
            {
                m.Textures ??= new();
            }

            return file;
        }
        catch (System.Text.Json.JsonException e)
        {
            error = e.Message;
            return null;
        }
    }

    private static float[] ToArray(Vector3 v) => new[] { v.X, v.Y, v.Z };

    private static Vector3 ToVector3(float[] a) => a is { Length: >= 3 } ? new Vector3(a[0], a[1], a[2]) : Vector3.Zero;
    #endregion

    #region Import
    /// <summary>
    /// The action that adds the data's dummies and materials to the loaded model, as one undo step, or null when there
    /// is nothing to add. The report says what it does, including the dummies skipped for missing bones.
    /// </summary>
    public static ViewportAction BuildImport(ModelEditorView view, ProjectEntry project, ModelContainer container,
        ModelTransferFile data, TransferOptions options, out TransferReport report)
    {
        report = new TransferReport();

        var flver = container.Flver;

        if (flver == null || data == null)
            return null;

        var actions = new List<ViewportAction>();

        // GX lists first: materials point at them by index
        var gxIndices = BuildGxLists(flver, data, options, actions, report);

        BuildMaterials(view, project, container, data, options, gxIndices, actions, report);
        BuildDummies(view, project, container, data, options, actions, report);

        if (actions.Count == 0)
            return null;

        return new ViewportCompoundAction(actions)
        {
            Label = LOC.Get("MODEL_Transfer_History", report.DummiesAdded, report.MaterialsAdded + report.MaterialsReplaced,
                data.SourceModel ?? "?")
        };
    }

    /// <summary>
    /// Each imported material's GX list: an equal one the model has, or one appended. Null entries for materials with
    /// none, or when GX lists aren't copied.
    /// </summary>
    private static int?[] BuildGxLists(FLVER2 flver, ModelTransferFile data, TransferOptions options,
        List<ViewportAction> actions, TransferReport report)
    {
        var result = new int?[data.Materials.Count];

        if (!options.CopyGx)
            return result;

        var pending = new List<FLVER2.GXList>();

        for (var i = 0; i < data.Materials.Count; i++)
        {
            var source = data.Materials[i].GX;

            if (source == null)
                continue;

            FLVER2.GXList gx;

            try
            {
                gx = ToGxList(source);
            }
            catch (FormatException)
            {
                continue;
            }

            var existing = flver.GXLists.FindIndex(g => GxEqual(g, gx));

            if (existing >= 0)
            {
                result[i] = existing;
                continue;
            }

            var added = pending.FindIndex(g => GxEqual(g, gx));

            if (added < 0)
            {
                pending.Add(gx);
                added = pending.Count - 1;
            }

            result[i] = flver.GXLists.Count + added;
        }

        foreach (var gx in pending)
        {
            actions.Add(new AddGxListAction(flver, gx));
        }

        report.GxListsAdded = pending.Count;

        return result;
    }

    private static FLVER2.GXList ToGxList(ModelTransferGxList source)
    {
        var gx = new FLVER2.GXList
        {
            TerminatorID = source.TerminatorID,
            TerminatorLength = source.TerminatorLength
        };

        foreach (var item in source.Items)
        {
            gx.Add(new FLVER2.GXItem(item.ID, item.Unk04, Convert.FromBase64String(item.Data ?? "")));
        }

        return gx;
    }

    private static bool GxEqual(FLVER2.GXList a, FLVER2.GXList b)
    {
        if (a.Count != b.Count || a.TerminatorID != b.TerminatorID || a.TerminatorLength != b.TerminatorLength)
            return false;

        for (var i = 0; i < a.Count; i++)
        {
            if (a[i].ID != b[i].ID || a[i].Unk04 != b[i].Unk04 ||
                !(a[i].Data ?? Array.Empty<byte>()).AsSpan().SequenceEqual(b[i].Data ?? Array.Empty<byte>()))
                return false;
        }

        return true;
    }

    private static List<FLVER2.Texture> ToTextures(ModelTransferMaterial source)
    {
        return source.Textures.Select(t => new FLVER2.Texture(t.ParamName ?? "", t.Path ?? "",
            t.TilingScale is { Length: >= 2 } ? new Vector2(t.TilingScale[0], t.TilingScale[1]) : Vector2.One,
            (FLVER2.Texture.TilingType)t.TilingTypeU, (FLVER2.Texture.TilingType)t.TilingTypeV,
            t.Unk14, t.Unk18, t.Unk1C)).ToList();
    }

    private static void BuildMaterials(ModelEditorView view, ProjectEntry project, ModelContainer container,
        ModelTransferFile data, TransferOptions options, int?[] gxIndices, List<ViewportAction> actions, TransferReport report)
    {
        var materials = ModelIndexRemapper.GetMaterials(container);
        var names = new HashSet<string>(materials.Select(m => m.Name ?? ""), StringComparer.Ordinal);
        var count = materials.Count;

        for (var i = 0; i < data.Materials.Count; i++)
        {
            var source = data.Materials[i];
            var gxIndex = gxIndices[i] ?? -1;

            // Replace: the target's material of that name takes the MTD, textures and GX list
            if (options.MaterialMode is TransferMaterialMode.ReplaceSameName)
            {
                var targetEntity = container.Materials.FirstOrDefault(e => (e.WrappedObject as FLVER2.Material)?.Name == source.Name);

                if (targetEntity?.WrappedObject is FLVER2.Material target)
                {
                    var changes = new List<ViewportAction>
                    {
                        Change(view, targetEntity, nameof(FLVER2.Material.MTD), source.MTD ?? ""),
                        Change(view, targetEntity, nameof(FLVER2.Material.Textures), ToTextures(source))
                    };

                    if (options.CopyGx)
                    {
                        changes.Add(Change(view, targetEntity, nameof(FLVER2.Material.GXIndex), gxIndex));
                    }

                    actions.AddRange(changes);
                    report.MaterialsReplaced++;
                    continue;
                }
            }

            // Add, under a name of its own
            var name = UniqueName(names, source.Name ?? "Material");
            names.Add(name);

            var material = new FLVER2.Material
            {
                Name = name,
                MTD = source.MTD ?? "",
                Textures = ToTextures(source),
                GXIndex = gxIndex,
                Index = count
            };

            count++;

            var entity = new ModelEntity(view.Universe, container, material, ModelEntityType.Material);
            actions.Add(new AddModelObjectAction(view, project, container, entity, container.RootObject));

            report.MaterialsAdded++;
        }
    }

    private static string UniqueName(HashSet<string> taken, string name)
    {
        if (!taken.Contains(name))
            return name;

        for (var i = 1; ; i++)
        {
            var candidate = $"{name}_{i}";

            if (!taken.Contains(candidate))
                return candidate;
        }
    }

    private static void BuildDummies(ModelEditorView view, ProjectEntry project, ModelContainer container,
        ModelTransferFile data, TransferOptions options, List<ViewportAction> actions, TransferReport report)
    {
        var nodes = ModelIndexRemapper.GetNodes(container);
        var nodeIndex = new Dictionary<string, int>(StringComparer.Ordinal);

        for (var i = 0; i < nodes.Count; i++)
        {
            nodeIndex.TryAdd(nodes[i].Name ?? "", i);
        }

        // A bone name to an index here: -1 for none, null when the model lacks it
        int? Resolve(string name)
        {
            if (name == null)
                return -1;

            if (nodeIndex.TryGetValue(name, out var index))
                return index;

            report.MissingBones.Add(name);
            return null;
        }

        foreach (var source in data.Dummies)
        {
            var id = source.ReferenceID + options.RefIdOffset;

            if (id < short.MinValue || id > short.MaxValue)
            {
                report.SkippedDummies.Add(LOC.Get("MODEL_Transfer_Skip_Id_Range", source.ReferenceID, id));
                continue;
            }

            var parent = Resolve(source.ParentBone);
            var attach = Resolve(source.AttachBone);

            if (parent == null || attach == null)
            {
                if (options.MissingBone is TransferMissingBone.Skip)
                {
                    report.SkippedDummies.Add(LOC.Get("MODEL_Transfer_Skip_Bone", source.ReferenceID,
                        parent == null ? source.ParentBone : source.AttachBone));
                    continue;
                }

                parent ??= -1;
                attach ??= -1;
            }

            var color = source.Color is { Length: >= 4 }
                ? Color.FromArgb(source.Color[0], source.Color[1], source.Color[2], source.Color[3])
                : Color.White;

            var dummy = new FLVER.Dummy
            {
                ReferenceID = (short)id,
                Position = ToVector3(source.Position),
                Forward = ToVector3(source.Forward),
                Upward = ToVector3(source.Upward),
                ParentBoneIndex = (short)parent.Value,
                AttachBoneIndex = (short)attach.Value,
                Color = color,

                // A dummy follows its attach bone exactly when it has one, in the games (see R2)
                FollowAttachBone = source.FollowAttachBone && attach.Value != -1,
                UseUpwardVector = source.UseUpwardVector,
                Unk30 = source.Unk30,
                Unk34 = source.Unk34
            };

            var entity = new ModelEntity(view.Universe, container, dummy, ModelEntityType.Dummy);
            var parentEntity = parent.Value >= 0 && parent.Value < container.Nodes.Count
                ? container.Nodes[parent.Value]
                : container.RootObject;

            actions.Add(new AddModelObjectAction(view, project, container, entity, parentEntity, (c, record) =>
            {
                // Every node a dummy names is marked as a dummy owner in the games
                var currentNodes = ModelIndexRemapper.GetNodes(c);

                foreach (var index in new int[] { dummy.ParentBoneIndex, dummy.AttachBoneIndex })
                {
                    if (index >= 0 && index < currentNodes.Count)
                    {
                        ModelIndexRemapper.MarkDummyOwner(currentNodes[index], record);
                    }
                }
            }));

            report.DummiesAdded++;
        }
    }

    private static ViewportAction Change(ModelEditorView view, Entity entity, string property, object value)
    {
        var prop = entity.WrappedObject.GetType().GetProperty(property);

        return new ModelPropertyChangeAction(view, prop, new HashSet<Entity> { entity }, value, ownerObject: entity.WrappedObject);
    }
    #endregion
}

/// <summary>
/// Appends a GX list to the model, for materials added or replaced by Data Transfer. Undo removes that list.
/// </summary>
public class AddGxListAction : ViewportAction
{
    private readonly FLVER2 Flver;
    private readonly FLVER2.GXList List;

    public AddGxListAction(FLVER2 flver, FLVER2.GXList list)
    {
        Flver = flver;
        List = list;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Flver.GXLists.Add(List);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Flver.GXLists.Remove(List);
        return ActionEvent.NoEvent;
    }
}
