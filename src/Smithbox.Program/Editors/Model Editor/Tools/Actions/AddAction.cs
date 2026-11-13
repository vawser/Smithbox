using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Creates new dummies, bones and materials in the loaded model. New objects go to the end of their list, so no
/// existing index shifts, and take the values the games give such objects (see Tools/BoneAxisSurvey defaults).
/// </summary>
public class AddAction
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public AddAction(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private ModelContainer Container => View.Selection.SelectedModelWrapper?.Container;

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        var enabled = Container != null;

        if (ImGui.BeginMenu($"{LOC.Get("MODEL_Tools_Action_Add_Title")}##addMenu", enabled))
        {
            DisplayItems(isMenu: true);

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.BeginMenu($"{LOC.Get("MODEL_Tools_Action_Add_Title")}##addContextMenu", Container != null))
        {
            DisplayItems(isMenu: false);

            ImGui.EndMenu();
        }
    }

    private void DisplayItems(bool isMenu)
    {
        var flags = Container != null ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled;

        bool Item(string label, string id) => isMenu
            ? ImGui.MenuItem($"{label}##{id}", "", false, Container != null)
            : ImGui.Selectable($"{label}##{id}", false, flags);

        // Dummy
        if (Item(LOC.Get("MODEL_Tools_Action_Add_Dummy"), "addDummy"))
        {
            ApplyAddDummy();
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Add_Dummy_TT"));

        // Bone
        if (Item(LOC.Get("MODEL_Tools_Action_Add_Bone"), "addBone"))
        {
            ApplyAddBone();
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Add_Bone_TT"));

        // Material
        if (Item(LOC.Get("MODEL_Tools_Action_Add_Material"), "addMaterial"))
        {
            ApplyAddMaterial();
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Add_Material_TT"));
    }

    private ModelEntity GetSingleSelected<T>()
    {
        var selected = View.ViewportSelection.GetFilteredSelection<ModelEntity>()
            .Where(e => e.WrappedObject is T)
            .ToList();

        return selected.Count == 1 ? selected[0] : null;
    }

    #region Dummy
    /// <summary>
    /// Adds a dummy.
    /// <para>
    /// With a bone selected it sits on that bone and follows it (its attach bone). With a dummy selected it joins that
    /// dummy's group, at its place, with its attach bone. Otherwise it is placed in front of the camera, unattached.
    /// </para>
    /// <para>
    /// The games hang every dummy off a group node (Dmypoly_...) as its parent bone, which sets the space its position
    /// is in; the attach bone is the one it moves with. The new dummy uses the group most of the model's dummies on
    /// the same attach bone use, or the model's most used group.
    /// </para>
    /// </summary>
    public void ApplyAddDummy()
    {
        var container = Container;

        if (container?.Flver == null)
            return;

        var flver = container.Flver;
        var nodes = ModelIndexRemapper.GetNodes(container);
        var dummies = container.Objects.Select(e => e.WrappedObject).OfType<FLVER.Dummy>().ToList();

        if (nodes.Count == 0)
        {
            Smithbox.LogError<AddAction>(LOC.Get("MODEL_Tools_Action_Add_Dummy_No_Nodes"));
            return;
        }

        var selectedBone = GetSingleSelected<FLVER.Node>();
        var selectedDummy = GetSingleSelected<FLVER.Dummy>();

        short attach = -1;
        short parent;
        Vector3 modelPosition;

        if (selectedDummy?.WrappedObject is FLVER.Dummy source)
        {
            attach = source.AttachBoneIndex;
            parent = source.ParentBoneIndex;
            modelPosition = Vector3.Transform(source.Position, GetNodeWorld(nodes, parent));
        }
        else if (selectedBone?.WrappedObject is FLVER.Node bone)
        {
            attach = (short)nodes.IndexOf(bone);
            parent = ChooseGroup(nodes, dummies, attach);
            modelPosition = GetNodeWorld(nodes, attach).Translation;
        }
        else
        {
            parent = ChooseGroup(nodes, dummies, -1);
            modelPosition = GetCameraPointInModel(container);
        }

        // The position is in the space of the parent bone
        var toParent = Matrix4x4.Invert(GetNodeWorld(nodes, parent), out var inverse) ? inverse : Matrix4x4.Identity;

        // Fields the games keep per group: Unk34 is the same for every dummy of a group, and Unk30 mostly
        // counts up in threes within it, so the new dummy takes the next step
        var groupDummies = dummies.Where(d => d.ParentBoneIndex == parent).ToList();

        var dummy = new FLVER.Dummy
        {
            Position = Vector3.Transform(modelPosition, toParent),
            Forward = Vector3.UnitZ,
            Upward = Vector3.UnitY,
            ReferenceID = GetNextReferenceID(dummies),
            ParentBoneIndex = parent,
            AttachBoneIndex = attach,
            FollowAttachBone = attach != -1,
            UseUpwardVector = true,
            Color = groupDummies.Count > 0 ? groupDummies[^1].Color : Color.White,
            Unk30 = groupDummies.Count > 0 ? groupDummies.Max(d => d.Unk30) + 3 : 0,
            Unk34 = groupDummies.Count > 0 ? groupDummies[0].Unk34 : 0
        };

        var entity = new ModelEntity(View.Universe, container, dummy, ModelEntityType.Dummy);
        var parentEntity = parent >= 0 && parent < container.Nodes.Count ? container.Nodes[parent] : container.RootObject;

        var action = new AddModelObjectAction(View, Project, container, entity, parentEntity, (c, record) =>
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
        });

        View.ViewportActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// The group node a new dummy hangs off: the one the model's dummies on the same attach bone use most, then the
    /// one used most overall. A model without dummies uses its first root node.
    /// </summary>
    private static short ChooseGroup(List<FLVER.Node> nodes, List<FLVER.Dummy> dummies, short attach)
    {
        var candidates = dummies
            .Where(d => d.ParentBoneIndex >= 0 && d.ParentBoneIndex < nodes.Count)
            .ToList();

        var onAttach = candidates.Where(d => d.AttachBoneIndex == attach).ToList();
        var pool = onAttach.Count > 0 ? onAttach : candidates;

        if (pool.Count > 0)
        {
            return pool.GroupBy(d => d.ParentBoneIndex).OrderByDescending(g => g.Count()).First().Key;
        }

        var root = nodes.FindIndex(n => n.ParentIndex == -1);

        return (short)Math.Max(root, 0);
    }

    private static short GetNextReferenceID(List<FLVER.Dummy> dummies)
    {
        var used = new HashSet<short>(dummies.Select(d => d.ReferenceID));

        var id = dummies.Count > 0 ? dummies.Max(d => d.ReferenceID) : (short)0;

        while (id < short.MaxValue && used.Contains(id))
        {
            id++;
        }

        return id;
    }

    /// <summary>
    /// A node's matrix in model space, the way the bones place dummies and vertices
    /// </summary>
    private static Matrix4x4 GetNodeWorld(List<FLVER.Node> nodes, int index)
    {
        return index >= 0 && index < nodes.Count ? Utils.GetBoneWorldMatrix(nodes[index], nodes) : Matrix4x4.Identity;
    }

    /// <summary>
    /// The point in front of the camera that Pull to Camera uses, in the model's space (without the display rotation)
    /// </summary>
    private Vector3 GetCameraPointInModel(ModelContainer container)
    {
        var viewport = View.ViewportWindow.Viewport;

        if (viewport == null)
            return Vector3.Zero;

        var camera = viewport.ViewportCamera.CameraTransform;
        var direction = Vector3.Transform(Vector3.UnitZ, camera.RotationMatrix);
        var point = camera.Position + direction * CFG.Current.Toolbar_Move_to_Camera_Offset;

        return Matrix4x4.Invert(container.DisplayMatrix, out var fromDisplay)
            ? Vector3.Transform(point, fromDisplay)
            : point;
    }
    #endregion

    #region Bone
    /// <summary>
    /// Adds a bone under the selected bone (or at the top of the skeleton), at its parent's origin. It is marked as
    /// unused, with an empty bounding box, as the games mark bones no mesh or dummy uses.
    /// </summary>
    public void ApplyAddBone()
    {
        var container = Container;

        if (container == null)
            return;

        var nodes = ModelIndexRemapper.GetNodes(container);
        var selectedBone = GetSingleSelected<FLVER.Node>();

        var parentIndex = selectedBone?.WrappedObject is FLVER.Node parentNode ? (short)nodes.IndexOf(parentNode) : (short)-1;

        var node = new FLVER.Node
        {
            Name = GetUniqueName(nodes.Select(n => n.Name), "NewBone"),
            ParentIndex = parentIndex,
            Translation = Vector3.Zero,
            Rotation = Vector3.Zero,
            Scale = Vector3.One,
            BoundingBoxMin = new Vector3(float.MaxValue),
            BoundingBoxMax = new Vector3(float.MinValue),
            Flags = FLVER.Node.NodeFlags.Disabled
        };

        var entity = new ModelEntity(View.Universe, container, node, ModelEntityType.Node);
        var parentEntity = selectedBone ?? (Entity)container.RootObject;

        var action = new AddModelObjectAction(View, Project, container, entity, parentEntity, (c, record) =>
        {
            // Into the sibling chain under its parent, now that it has its place at the end of the node list
            var currentNodes = ModelIndexRemapper.GetNodes(c);

            ModelIndexRemapper.LinkNewNode(currentNodes, currentNodes.IndexOf(node), record);
        });

        View.ViewportActionManager.ExecuteAction(action);
    }
    #endregion

    #region Material
    /// <summary>
    /// Adds a copy of the selected material (or of the model's first one) under a name of its own, for meshes to be
    /// moved onto. A model without materials gets an empty one.
    /// </summary>
    public void ApplyAddMaterial()
    {
        var container = Container;

        if (container == null)
            return;

        var materials = ModelIndexRemapper.GetMaterials(container);

        var source = GetSingleSelected<FLVER2.Material>()
            ?? container.Objects.OfType<ModelEntity>().FirstOrDefault(e => e.WrappedObject is FLVER2.Material);

        ModelEntity entity;

        if (source != null)
        {
            entity = (ModelEntity)source.Clone();
            entity.Type = ModelEntityType.Material;
        }
        else
        {
            entity = new ModelEntity(View.Universe, container, new FLVER2.Material { Name = "", MTD = "" },
                ModelEntityType.Material);
        }

        var material = (FLVER2.Material)entity.WrappedObject;
        material.Name = GetUniqueName(materials.Select(m => m.Name), source != null ? material.Name : "NewMaterial");

        var action = new AddModelObjectAction(View, Project, container, entity, container.RootObject);

        View.ViewportActionManager.ExecuteAction(action);
    }
    #endregion

    /// <summary>
    /// The name, or the name with the lowest number after it that isn't taken
    /// </summary>
    private static string GetUniqueName(IEnumerable<string> taken, string name)
    {
        var used = new HashSet<string>(taken.Where(n => n != null), StringComparer.OrdinalIgnoreCase);

        if (!used.Contains(name))
            return name;

        for (var i = 1; ; i++)
        {
            var candidate = $"{name}_{i}";

            if (!used.Contains(candidate))
                return candidate;
        }
    }
}
