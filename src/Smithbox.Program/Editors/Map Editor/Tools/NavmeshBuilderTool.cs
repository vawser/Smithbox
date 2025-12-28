using Hexa.NET.ImGui;
using HKX2;
using HKX2.Builders;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class NavmeshBuilderTool
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    private readonly MeshRenderableProxy _previewMesh = null;
    private readonly int icount = 0;
    private readonly int vcount = 0;

    private ResourceHandle<HavokNavmeshResource> _previewResource;

    private float AgentClimb = 0.3f;
    private float AgentHeight = 0.5f;
    private float AgentRadius = 0.3f;
    private bool BuildSuccess;
    private float Cellheight = 0.3f;

    private float Cellsize = 0.3f;
    private bool DidBuild;
    private int MinRegionArea = 3;
    private float SlopeAngle = 30.0f;

    public NavmeshBuilderTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Update Loop
    /// </summary>
    public void OnGui()
    {

    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {

    }
    
    /// <summary>
     /// Context Menu
     /// </summary>
    public void OnContext(Entity ent)
    {
        // Not shown here
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Not shown here
    }

    /// <summary>
    /// Tool Menu
    /// </summary>
    public void OnToolMenu()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Navmesh Builder"))
        {
            if (Editor.Project.ProjectType != ProjectType.DS3)
            {
                ImGui.Text("Navmesh building only supported for DS3");
                ImGui.End();
                return;
            }

            var sel = Editor.ViewportSelection.GetSingleFilteredSelection<Entity>();
            if (sel != null && sel.RenderSceneMesh != null && sel.RenderSceneMesh is MeshRenderableProxy mrp &&
                mrp.ResourceHandle != null && mrp.ResourceHandle is ResourceHandle<HavokCollisionResource> col)
            {
                ImGui.LabelText("value", "lable");
                ImGui.DragFloat("Cell size", ref Cellsize, 0.005f, 0.0f);
                ImGui.DragFloat("Cell height", ref Cellheight, 0.005f, 0.0f);
                ImGui.DragFloat("Slope Angle", ref SlopeAngle, 0.5f, 0.0f, 85.0f);
                ImGui.DragFloat("Agent Height", ref AgentHeight, 0.005f, 0.0f);
                ImGui.DragFloat("Agent Climb", ref AgentClimb, 0.005f, 0.0f);
                ImGui.DragFloat("Agent Radius", ref AgentRadius, 0.005f, 0.0f);
                ImGui.DragInt("Min Region Area", ref MinRegionArea, 1, 0);

                if (ImGui.Button("Build Navmesh", DPI.StandardButtonSize))
                {
                    var buildverts = new List<Vector3>();
                    var buildindices = new List<int>();
                    var vbase = 0;
                    foreach (HavokCollisionResource.CollisionSubmesh sm in col.Get().GPUMeshes)
                    {
                        buildverts.AddRange(sm.PickingVertices);
                        foreach (var i in sm.PickingIndices)
                        {
                            buildindices.Add(i + vbase);
                        }

                        vbase += sm.PickingVertices.Length;
                    }

                    //var sm = col.Resource.Get().GPUMeshes[0];
                    Vector3[] bv = buildverts.ToArray();
                    //buildindices.Reverse();
                    var bi = buildindices.ToArray();

                    foreach (var i in bi)
                    {
                        Vector3 x = bv[i];
                    }


                    NavGen.SetNavmeshBuildParams(Cellsize, Cellheight, SlopeAngle, AgentHeight, AgentClimb,
                        AgentRadius, MinRegionArea);

                    var p = new hkaiNavMeshBuilder.BuildParams();
                    p.Cellsize = Cellsize;
                    p.Cellheight = Cellheight;
                    p.SlopeAngle = SlopeAngle;
                    p.AgentHeight = AgentHeight;
                    p.AgentClimb = AgentClimb;
                    p.AgentRadius = AgentRadius;
                    p.MinRegionArea = MinRegionArea;

                    var builder = new hkaiNavMeshBuilder();
                    hkRootLevelContainer built = builder.BuildNavmesh(p, buildverts, buildindices);
                    BuildSuccess = built != null;
                    DidBuild = true;
                    if (BuildSuccess)
                    {
                        if (_previewMesh != null)
                        {
                            _previewMesh.Dispose();
                        }

                        HavokNavmeshResource res = HavokNavmeshResource.ResourceFromNavmeshRoot(built);
                        _previewResource = ResourceHandle<HavokNavmeshResource>.TempHandleFromResource(res);
                        //_previewMesh = MeshRenderableProxy.MeshRenderableFromHavokNavmeshResource(_scene, _previewResource, true);
                        _previewMesh.World = mrp.World;

                        // Do a test save
                        var path = Path.Join(Editor.Project.ProjectPath, "navout", "test.hkx");
                        using (FileStream s2 = File.Create(path))
                        {
                            BinaryWriterEx bw = new(false, s2);
                            var s = new PackFileSerializer();
                            s.Serialize(built, bw);
                        }

                        /*vcount = NavGen.GetMeshVertCount();
                        icount = NavGen.GetMeshTriCount();

                        if (icount > 0)
                        {
                            // Make preview mesh
                            ushort[] verts = new ushort[vcount * 3];
                            ushort[] indices = new ushort[icount * 3 * 2];
                            NavGen.GetMeshVerts(verts);
                            NavGen.GetMeshTris(indices);

                            Vector3[] bounds = new Vector3[2];
                            NavGen.GetBoundingBox(bounds);

                            if (_previewMesh != null)
                            {
                                _previewMesh.UnregisterWithScene();
                                _previewMesh = null;
                            }

                            var nvm = new Scene.NvmRenderer(bounds[0], bounds[1], Cellsize, Cellheight, verts, indices);
                            _previewMesh = new Scene.Mesh(Scene, nvm.Bounds, nvm);
                            _previewMesh.Highlighted = true;
                        }*/
                    }
                }

                if (DidBuild)
                {
                    if (BuildSuccess)
                    {
                        ImGui.Text("Successfully built navmesh");
                        ImGui.Text($@"Vertex count: {vcount}");
                        ImGui.Text($@"Triangle count: {icount}");
                    }
                    else
                    {
                        ImGui.Text("Navmesh build failed");
                    }
                }
            }
            else
            {
                ImGui.Text("Select a single collision mesh to generate a navmesh");
            }
        }
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        // Not shown here
    }

    /// <summary>
    /// Effect
    /// </summary>
}
