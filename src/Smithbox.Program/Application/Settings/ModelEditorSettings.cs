using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ProjectileEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using static StudioCore.Editors.MapEditor.DuplicateAction;

namespace StudioCore.Application;

/// <summary>
///     The Model Editor's settings (<c>CFG.Current.ModelEditor</c>), including its viewport grids. Saved in
///     Configuration.json under the same ModelEditor_ keys as before they moved here.
/// </summary>
public sealed class ModelEditorSettings : ICfgSection
{
    // Model Editor Collision tool: the layer edited (High or Low; models have no fall protection), and the layers its
    // Create tab builds. Its brush, material and simplify options are the Map Editor's Collision Editor's.
    public HavokCollisionType ModelEditor_CollisionTool_Layer = HavokCollisionType.Low;
    public bool ModelEditor_CollisionTool_Create_High = true;
    public bool ModelEditor_CollisionTool_Create_Low = true;

    ///------------------------------------------------------------
    /// Model Editor
    ///------------------------------------------------------------
    // General
    public bool ModelEditor_Containers_IncludeAliasInSearch = true;

    public bool ModelEditor_Files_AutoLoadFirstEntry = true;

    // Where the Render Snapshots tool writes its images: <folder>/<GAME>/Current
    public string ModelEditor_RenderSnapshot_Folder = "";

    public bool ModelEditor_Contents_Auto_Open_Tree = false;
    public bool ModelEditor_Contents_NodeNameInMeshEntry = true;

    // Redraw the model from its edited data shortly after each edit, rather than from the stored file
    public bool ModelEditor_Viewport_Live_Preview = true;
    public bool ModelEditor_Contents_Node_Hierarchy = false;

    // Save Check: File › Save and Save All check the models they will write first, and stop on errors (or warnings too)
    public bool ModelEditor_SaveCheck_Enabled = true;
    public bool ModelEditor_SaveCheck_Stop_On_Warnings = false;

    // Contents colours objects that differ from, or aren't in, the vanilla model
    public bool ModelEditor_Vanilla_Highlight = true;

    // History: how many models switched away from keep their objects and undo history in each view (0 = none)
    public int ModelEditor_History_Parked_Limit = 4;

    public bool ModelEditor_ViewMeshes = true;
    public bool ModelEditor_ViewDummyPolys = true;
    public bool ModelEditor_ViewBones = true;
    public bool ModelEditor_ViewSkeleton = true;
    public bool ModelEditor_ViewHighCollision = false;
    public bool ModelEditor_ViewLowCollision = true;

    public bool ModelEditor_ExactSearch = false;

    public bool ModelEditor_Properties_Enable_Commmunity_Names = true;
    public bool ModelEditor_Properties_Enable_Type_Column = false;
    public bool ModelEditor_Properties_Auto_Open_Tree = true;
    public bool ModelEditor_Properties_Enable_Mesh_Fields = true;

    public bool ModelEditor_DisplayDmyPolyReferenceID = true;
    public bool ModelEditor_DisplayMatNameOnMesh = true;

    public bool ModelEditor_ModelLoad_MapPieces = true;
    public bool ModelEditor_ModelLoad_Objects = true;
    public bool ModelEditor_ModelLoad_Parts = true;
    public bool ModelEditor_ModelLoad_Characters = true;
    public bool ModelEditor_ModelLoad_Collisions = true;
    public bool ModelEditor_ModelLoad_Navmeshes = true;

    public bool ModelEditor_TextureLoad_MapPieces = true;
    public bool ModelEditor_TextureLoad_Objects = true;
    public bool ModelEditor_TextureLoad_Parts = true;
    public bool ModelEditor_TextureLoad_Characters = true;
    public bool ModelEditor_TextureLoad_Misc = true;

    public bool ModelEditor_DisplayDummyLabels = false;
    public bool ModelEditor_DisplayNodeLabels = false;
    public bool ModelEditor_DisplayDummies = true;
    public bool ModelEditor_DisplayNodes = true;
    public float ModelEditor_LabelScale = 1.0f;
    public bool ModelEditor_DisplayEdgePoints = true;

    public bool ModelEditor_ModelSourceList_RequireDoubleClick = false;

    // Viewport labels only on the selected dummies and bones, as a character has hundreds
    public bool ModelEditor_Labels_Selected_Only = false;

    // Saving
    public bool ModelEditor_Allow_Unmodified_Save = false;
    public bool ModelEditor_Allow_Automatic_Save = true;

    public ViewportGridSet ModelEditor_Grids = new();

    // ModelExport_
    public string ModelExport_Output_Directory = "";
    public bool ModelExport_UseCollisionMaterial = false;
    public bool ModelExport_UseSingleCollisionMaterial = false;
    public bool ModelExport_FlipUVs = true;
    public bool ModelExport_MirrorX = false;
    public bool ModelExport_MirrorY = false;
    public bool ModelExport_MirrorZ = false;

    // Variant tool: which sections are open
    public bool ModelEditor_VariantTool_Show_Tint = true;
    public bool ModelEditor_VariantTool_Show_Colour_Shift = true;
    public bool ModelEditor_VariantTool_Show_Collision = true;
    public ModelExportAxis ModelExport_ForwardAxis = ModelExportAxis.PosZ;
    public ModelExportAxis ModelExport_UpAxis = ModelExportAxis.PosY;
}
