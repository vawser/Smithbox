using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public enum ModelVanillaStatus
{
    /// <summary>Not a material, node, dummy or mesh</summary>
    None,

    /// <summary>The game has no such model (the mod added it), or it couldn't be read</summary>
    NoVanilla,

    Unchanged,
    Modified,

    /// <summary>Vanilla has no counterpart</summary>
    Added
}

/// <summary>
/// Compares the loaded model of a view with the model as the game ships it, for the Vanilla Compare tool: what is
/// modified, added and removed, field by field, and the actions that revert them. The diff itself is
/// <see cref="FlverDiff" />. Plan: Documentation/Model Editor Second Review.md, R6.
/// </summary>
public class ModelVanillaCompare
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    private ModelWrapper CachedModel;
    private FLVER2 CachedVanilla;
    private bool CachedHasVanilla;

    private FlverDiff CachedDiff;
    private int CachedVersion = int.MinValue;

    public ModelVanillaCompare(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Drops the vanilla copy, to be read again on next use. Called after a revert or restore, which hand vanilla's own
    /// objects (lists, nodes, dummies) to the model, so they mustn't be compared against any more.
    /// </summary>
    public void Invalidate()
    {
        CachedModel = null;
        CachedVanilla = null;
        CachedDiff = null;
    }

    /// <summary>
    /// The diff of the loaded model with vanilla, rebuilt when the history changes. Null when there is no loaded
    /// model or no vanilla model (<paramref name="hasVanilla" /> says which).
    /// </summary>
    public FlverDiff GetDiff(out bool hasVanilla)
    {
        hasVanilla = false;

        var wrapper = View.Selection.SelectedModelWrapper;

        if (wrapper?.Container?.View != View || wrapper.FLVER == null)
            return null;

        if (CachedModel != wrapper)
        {
            CachedModel = wrapper;
            CachedVanilla = wrapper.ReadVanillaFlver();
            CachedHasVanilla = CachedVanilla != null;
            CachedDiff = null;
        }

        hasVanilla = CachedHasVanilla;

        if (!CachedHasVanilla)
            return null;

        var version = View.ViewportActionManager.Version;

        if (CachedDiff == null || CachedVersion != version)
        {
            // The FLVER's lists follow the objects, in the order a save writes them
            wrapper.UpdateFLVER();

            CachedDiff = FlverDiff.Build(wrapper.FLVER, CachedVanilla);
            CachedVersion = version;
        }

        return CachedDiff;
    }

    public ModelVanillaStatus GetStatus(Entity entity)
    {
        if (entity?.WrappedObject is not (FLVER2.Material or FLVER.Node or FLVER.Dummy or FLVER2.Mesh))
            return ModelVanillaStatus.None;

        var diff = GetDiff(out var hasVanilla);

        if (!hasVanilla)
            return ModelVanillaStatus.NoVanilla;

        if (diff == null || !diff.Pairs.TryGetValue(entity.WrappedObject, out var vanilla))
            return ModelVanillaStatus.None;

        if (vanilla == null)
            return ModelVanillaStatus.Added;

        // Contents asks for every row every frame, and a mesh compares its geometry, so the answer is kept until
        // the next diff (one is made after every edit)
        if (!ReferenceEquals(StatusFor, diff))
        {
            StatusFor = diff;
            StatusCache.Clear();
        }

        if (!StatusCache.TryGetValue(entity.WrappedObject, out var status))
        {
            status = diff.Differs(entity.WrappedObject) ? ModelVanillaStatus.Modified : ModelVanillaStatus.Unchanged;
            StatusCache[entity.WrappedObject] = status;
        }

        return status;
    }

    private FlverDiff StatusFor;
    private readonly Dictionary<object, ModelVanillaStatus> StatusCache = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Pushes the Contents text colour for an object that differs from vanilla (green) or isn't in it (blue), when
    /// Options › Contents › Highlight Changes from Vanilla is on. True if a colour was pushed, to pop after the item.
    /// </summary>
    public bool PushHighlightColour(Entity entity)
    {
        if (!CFG.Current.ModelEditor_Vanilla_Highlight)
            return false;

        switch (GetStatus(entity))
        {
            case ModelVanillaStatus.Modified:
                Hexa.NET.ImGui.ImGui.PushStyleColor(Hexa.NET.ImGui.ImGuiCol.Text, UI.Current.ImGui_MapEditor_VanillaModified_Text);
                return true;

            case ModelVanillaStatus.Added:
                Hexa.NET.ImGui.ImGui.PushStyleColor(Hexa.NET.ImGui.ImGuiCol.Text, UI.Current.ImGui_MapEditor_VanillaAdded_Text);
                return true;

            default:
                return false;
        }
    }

    #region Reverts
    /// <summary>The entity wrapping a FLVER object of the loaded model, or the model root for the model itself</summary>
    private Entity EntityOf(object owner)
    {
        var container = View.Selection.SelectedModelWrapper?.Container;

        if (container == null)
            return null;

        if (owner is FLVER2 || owner is FLVER2.FLVERHeader)
            return container.RootObject;

        return container.Objects.FirstOrDefault(e => ReferenceEquals(e.WrappedObject, owner));
    }

    /// <summary>
    /// The action that sets one field back to vanilla, or null with the reason. <paramref name="target" /> is the
    /// entity the field belongs to (for a nested field, the object holding it).
    /// </summary>
    public ViewportAction BuildFieldRevert(FlverDiff diff, Entity target, FlverFieldDiff field, out string reason)
    {
        reason = null;

        switch (field.Kind)
        {
            case FlverDiffKind.Info:
                reason = LOC.Get("MODEL_Vanilla_Revert_Not_Here");
                return null;

            case FlverDiffKind.Geometry:
                if (target is not ModelEntity meshEntity || field.Owner is not FLVER2.Mesh mesh)
                    return null;

                var geometry = diff.CopyVanillaGeometry(mesh, out var missing);

                if (geometry == null)
                {
                    reason = LOC.Get("MODEL_Vanilla_Revert_Missing", missing);
                    return null;
                }

                return new MeshGeometryRevertAction(meshEntity, geometry);

            case FlverDiffKind.NodeReference:
            case FlverDiffKind.MaterialReference:
                var index = ResolveReference(diff, field.Kind, field.Vanilla as string, out reason);

                if (index == null)
                    return null;

                return Change(target, field, Convert.ChangeType(index.Value, field.Property.PropertyType));

            case FlverDiffKind.BoneTable:
                var names = (List<string>)field.Vanilla;
                var table = new List<int>();

                foreach (var name in names)
                {
                    var bone = ResolveReference(diff, FlverDiffKind.NodeReference, name, out reason);

                    if (bone == null)
                        return null;

                    table.Add(bone.Value);
                }

                return Change(target, field, table);

            default:
                if (field.Property == null || (field.Index < 0 && field.Property.GetSetMethod() == null))
                {
                    reason = LOC.Get("MODEL_Vanilla_Revert_Read_Only");
                    return null;
                }

                return Change(target, field, field.Vanilla);
        }
    }

    private ViewportAction Change(Entity target, FlverFieldDiff field, object value)
    {
        var entity = EntityOf(field.Owner) ?? target;

        return new ModelPropertyChangeAction(View, field.Property, new HashSet<Entity> { entity }, value,
            field.Index, ownerObject: field.Owner);
    }

    /// <summary>The current index of a node or material named as vanilla names it ("#N" keeps the number)</summary>
    private static int? ResolveReference(FlverDiff diff, FlverDiffKind kind, string name, out string reason)
    {
        reason = null;

        if (name == null)
            return -1;

        if (name.StartsWith('#') && int.TryParse(name[1..], out var raw))
            return raw;

        var index = kind is FlverDiffKind.MaterialReference
            ? diff.Current.Materials.FindIndex(m => m.Name == name)
            : diff.Current.Nodes.FindIndex(n => n.Name == name);

        if (index < 0)
        {
            reason = LOC.Get(kind is FlverDiffKind.MaterialReference ? "MODEL_Vanilla_Revert_No_Material" : "MODEL_Vanilla_Revert_No_Node", name);
            return null;
        }

        return index;
    }

    /// <summary>
    /// Every field of the object that can be reverted, as one undo step. <paramref name="skipped" /> counts the
    /// fields that couldn't be, with the first reason.
    /// </summary>
    /// <param name="isModel">The entity is the model root, so the model's own fields (the header) are reverted</param>
    public ViewportAction BuildObjectRevert(FlverDiff diff, Entity entity, bool isModel, out int skipped, out string reason)
    {
        skipped = 0;
        reason = null;

        var actions = new List<ViewportAction>();

        var fields = isModel ? diff.CompareModel() : diff.Compare(entity.WrappedObject);

        foreach (var field in fields)
        {
            var action = BuildFieldRevert(diff, entity, field, out var why);

            if (action == null)
            {
                skipped++;
                reason ??= why;
                continue;
            }

            actions.Add(action);
        }

        if (actions.Count == 0)
            return null;

        return new ViewportCompoundAction(actions)
        {
            Label = LOC.Get("MODEL_Vanilla_History_Revert",
                isModel ? LOC.Get("MODEL_SaveCheck_Object_Model") : ModelHistory.NameObject(entity))
        };
    }

    /// <summary>
    /// The action that adds a removed vanilla material, node or dummy back, at the end of its list. Its node indices
    /// are mapped by name. Null, with the reason, for a mesh or a node the model lacks.
    /// </summary>
    public ViewportAction BuildRestore(FlverDiff diff, object removed, out string reason)
    {
        reason = null;

        var container = View.Selection.SelectedModelWrapper?.Container;

        if (container == null)
            return null;

        switch (removed)
        {
            case FLVER2.Material material:
            {
                var entity = new ModelEntity(View.Universe, container, material, ModelEntityType.Material);
                return Label(new AddModelObjectAction(View, Project, container, entity, container.RootObject), material.Name);
            }

            case FLVER.Node node:
            {
                var parent = ResolveReference(diff, FlverDiffKind.NodeReference, diff.VanillaNodeName(node.ParentIndex), out reason);

                if (parent == null)
                    return null;

                node.ParentIndex = (short)parent.Value;
                node.FirstChildIndex = -1;
                node.NextSiblingIndex = -1;
                node.PreviousSiblingIndex = -1;

                var entity = new ModelEntity(View.Universe, container, node, ModelEntityType.Node);
                var parentEntity = parent.Value >= 0 && parent.Value < container.Nodes.Count ? container.Nodes[parent.Value] : container.RootObject;

                var action = new AddModelObjectAction(View, Project, container, entity, parentEntity, (c, record) =>
                {
                    var nodes = ModelIndexRemapper.GetNodes(c);
                    ModelIndexRemapper.LinkNewNode(nodes, nodes.IndexOf(node), record);
                });

                return Label(action, node.Name);
            }

            case FLVER.Dummy dummy:
            {
                var parent = ResolveReference(diff, FlverDiffKind.NodeReference, diff.VanillaNodeName(dummy.ParentBoneIndex), out reason);
                var attach = parent == null ? null : ResolveReference(diff, FlverDiffKind.NodeReference, diff.VanillaNodeName(dummy.AttachBoneIndex), out reason);

                if (parent == null || attach == null)
                    return null;

                dummy.ParentBoneIndex = (short)parent.Value;
                dummy.AttachBoneIndex = (short)attach.Value;

                var entity = new ModelEntity(View.Universe, container, dummy, ModelEntityType.Dummy);
                var parentEntity = parent.Value >= 0 && parent.Value < container.Nodes.Count ? container.Nodes[parent.Value] : container.RootObject;

                return Label(new AddModelObjectAction(View, Project, container, entity, parentEntity), LOC.Get("MODEL_History_Dummy", dummy.ReferenceID));
            }

            default:
                reason = LOC.Get("MODEL_Vanilla_Restore_Mesh");
                return null;
        }
    }

    private static ViewportAction Label(ViewportAction action, string name)
    {
        return new ViewportCompoundAction(new List<ViewportAction> { action })
        {
            Label = LOC.Get("MODEL_Vanilla_History_Restore", name)
        };
    }
    #endregion

    #region Display
    /// <summary>A short text for a value in the compare table</summary>
    public static string Format(object value)
    {
        switch (value)
        {
            case null:
                return LOC.Get("MODEL_Vanilla_None_Value");
            case string s:
                return $"\"{s}\"";
            case float f:
                return f.ToString("0.####", CultureInfo.InvariantCulture);
            case Vector3 v:
                return $"{Format(v.X)}, {Format(v.Y)}, {Format(v.Z)}";
            case Vector2 v2:
                return $"{Format(v2.X)}, {Format(v2.Y)}";
            case IList list:
                var shown = list.Cast<object>().Take(8).Select(Format);
                return $"[{string.Join(", ", shown)}{(list.Count > 8 ? $", … ({list.Count})" : "")}]";
            default:
                var type = value.GetType();
                return type.IsClass ? type.Name : Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
    #endregion
}
