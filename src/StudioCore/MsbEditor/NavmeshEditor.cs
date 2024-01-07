using HKX2;
using HKX2.Builders;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Resource;
using StudioCore.Scene;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace StudioCore.MsbEditor;

/// <summary>
///     Editor used to bake new navmeshes for cols
/// </summary>
public class NavmeshEditor
{
    private readonly AssetLocator _locator;
    private readonly MeshRenderableProxy _previewMesh = null;
    private readonly Selection _selection;
    private readonly int icount = 0;
    private readonly int vcount = 0;

    private ResourceHandle<HavokNavmeshResource> _previewResource;
    private RenderScene _scene;
    private float AgentClimb = 0.3f;
    private float AgentHeight = 0.5f;
    private float AgentRadius = 0.3f;
    private bool BuildSuccess;
    private float Cellheight = 0.3f;

    private float Cellsize = 0.3f;
    private bool DidBuild;
    private int MinRegionArea = 3;
    private float SlopeAngle = 30.0f;

    public NavmeshEditor(AssetLocator locator, RenderScene scene, Selection sel)
    {
        _locator = locator;
        _scene = scene;
        _selection = sel;
    }

    public void OnGui(GameType game)
    {
        if (ImGui.Begin("Navmesh Build"))
        {
            if (game != GameType.DarkSoulsIII)
            {
                ImGui.Text("Navmesh building only supported for DS3");
                ImGui.End();
                return;
            }

            var sel = _selection.GetSingleFilteredSelection<Entity>();
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

                if (ImGui.Button("Build Navmesh"))
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
                        var path = $@"{_locator.GameModDirectory}\navout\test.hkx";
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

        ImGui.End();
    }
}
