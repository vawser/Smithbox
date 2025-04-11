using ImGuiNET;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Tools.LightmapAtlasEditor;
using StudioCore.Interface;
using StudioCore.Scene.Framework;
using StudioCore.Scene.Meshes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.MSB.Shape.Composite;

namespace StudioCore.Editors.MapEditor.Tools.EntityInformation;

public class EntityInformationView
{
    private MapEditorScreen Screen;

    public EntityInformationView(MapEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnGui()
    {
        if (!UI.Current.Interface_MapEditor_EntityInformation)
            return;

        var scale = DPI.GetUIScale();

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Entity Information##MapEditor_EntityInformation"))
        {
            /*
            ImGui.Separator();
            ImGui.Text("LoadedObjectContainers");
            ImGui.Separator();
            var maps = Screen.Universe.LoadedObjectContainers;

            if(maps != null)
            {
                foreach(var entry in maps)
                {
                    ImGui.Text($"{entry.Key}: {entry.Value}");
                }
            }
            */

            // Entity
            var firstEnt = (Entity)Screen.Selection.GetSelection().FirstOrDefault();

            if(firstEnt != null)
            {
                // Entity
                ImGui.Separator();
                ImGui.Text("Entity");
                ImGui.Separator();

                ImGui.Text($"Name: {firstEnt.Name}");

                if (firstEnt.Parent != null)
                {
                    ImGui.Text($"Parent Name: {firstEnt.Parent.Name}");
                }

                ImGui.Text($"Children:");
                for (int i = 0; i < firstEnt.Children.Count; i++)
                {
                    var child = (Entity)firstEnt.Children[i];

                    ImGui.Text($" [{i}]: {child.Name}");
                }

                ImGui.Separator();
                ImGui.Text($"EditorVisible: {firstEnt.EditorVisible}");
                ImGui.Text($"CachedName: {firstEnt.CachedName}");
                ImGui.Text($"CachedAliasName: {firstEnt.CachedAliasName}");
                ImGui.Text($"CurrentModelName: {firstEnt.CurrentModelName}");
                ImGui.Text($"disposedValue: {firstEnt.disposedValue}");
                ImGui.Text($"TempTransform: {firstEnt.TempTransform}");
                ImGui.Text($"UseTempTransform: {firstEnt.UseTempTransform}");

                ImGui.Text($"ReferencingObjects: {firstEnt.ReferencingObjects}");

                // ObjectContainer
                ImGui.Separator();
                ImGui.Text("ObjectContainer");
                ImGui.Separator();
                ImGui.Text($"Name: {firstEnt.Container.Name}");

                // WrappedObject
                ImGui.Separator();
                ImGui.Text("WrappedObject");
                ImGui.Separator();
                ImGui.Text($"WrappedObject: {firstEnt.WrappedObject}");

                // RenderableProxy
                ImGui.Separator();
                ImGui.Text("RenderableProxy");
                ImGui.Separator();

                if (firstEnt.RenderSceneMesh != null)
                {
                    ImGui.Text($"AutoRegister: {firstEnt.RenderSceneMesh.AutoRegister}");
                    ImGui.Text($"_registered: {firstEnt.RenderSceneMesh._registered}");
                    ImGui.Text($"disposedValue: {firstEnt.RenderSceneMesh.disposedValue}");
                    ImGui.Text($"RenderSelectionOutline: {firstEnt.RenderSceneMesh.RenderSelectionOutline}");
                    ImGui.Text($"World: {firstEnt.RenderSceneMesh.World}");
                    ImGui.Text($"DrawFilter: {firstEnt.RenderSceneMesh.DrawFilter}");
                    ImGui.Text($"DrawGroups: {firstEnt.RenderSceneMesh.DrawGroups}");
                    ImGui.Text($"Visible: {firstEnt.RenderSceneMesh.Visible}");

                    // -> MeshRenderableProxy
                    if (firstEnt.RenderSceneMesh is MeshRenderableProxy)
                    {
                        ImGui.Text("");
                        ImGui.Text("MeshRenderableProxy ->");

                        var meshRenderableProxy = (MeshRenderableProxy)firstEnt.RenderSceneMesh;

                        if (meshRenderableProxy != null)
                        {
                            ImGui.Text($"VirtPath: {meshRenderableProxy.VirtPath}");
                            ImGui.Text($"_placeholderType: {meshRenderableProxy._placeholderType}");

                            // Mesh Provider
                            // _meshProvider
                            var meshProvider = meshRenderableProxy._meshProvider;

                            if (meshProvider != null)
                            {
                                ImGui.Text("");
                                ImGui.Text("MeshProvider ->");

                                ImGui.Text($"_listeners:");

                                for (int i = 0; i < meshProvider._listeners.Count; i++)
                                {
                                    var curListener = meshProvider._listeners[i];

                                    ImGui.Text($" [{i}]: {curListener}");
                                }

                                ImGui.Text($"ChildCount: {meshProvider.ChildCount}");
                                //ImGui.Text($"LayoutType: {meshProvider.LayoutType}");
                                //ImGui.Text($"LayoutDescription: {meshProvider.LayoutDescription}");
                                ImGui.Text($"Bounds: {meshProvider.Bounds}");
                                ImGui.Text($"ObjectTransform: {meshProvider.ObjectTransform}");
                                //ImGui.Text($"GeometryBuffer: {meshProvider.GeometryBuffer}");
                                //ImGui.Text($"MaterialBuffer: {meshProvider.MaterialBuffer}");
                                ImGui.Text($"MaterialIndex: {meshProvider.MaterialIndex}");
                                //ImGui.Text($"BoneBuffer: {meshProvider.BoneBuffer}");
                                //ImGui.Text($"ShaderName: {meshProvider.ShaderName}");

                                /*
                                ImGui.Text($"SpecializationConstants:");

                                for (int i = 0; i < meshProvider.SpecializationConstants.Length; i++)
                                {
                                    var curConstant = meshProvider.SpecializationConstants[i];

                                    ImGui.Text($" [{i}]: {curConstant}");
                                }
                                */

                                ImGui.Text($"CullMode: {meshProvider.CullMode}");
                                ImGui.Text($"FillMode: {meshProvider.FillMode}");
                                ImGui.Text($"FrontFace: {meshProvider.FrontFace}");
                                ImGui.Text($"Topology: {meshProvider.Topology}");
                                ImGui.Text($"Is32Bit: {meshProvider.Is32Bit}");
                                ImGui.Text($"IndexOffset: {meshProvider.IndexOffset}");
                                ImGui.Text($"IndexCount: {meshProvider.IndexCount}");
                                ImGui.Text($"VertexSize: {meshProvider.VertexSize}");


                                ImGui.Text($"UseSelectedShader: {meshProvider.UseSelectedShader}");
                                ImGui.Text($"SelectedUseBackface: {meshProvider.SelectedUseBackface}");
                                ImGui.Text($"SelectedRenderBaseMesh: {meshProvider.SelectedRenderBaseMesh}");

                                if (meshProvider.ResourceHandle != null)
                                {
                                    ImGui.Text($"ResourceHandle.AssetVirtualPath: {meshProvider.ResourceHandle.AssetVirtualPath}");
                                    ImGui.Text($"ResourceHandle.IsLoaded: {meshProvider.ResourceHandle.IsLoaded()}");
                                    ImGui.Text($"ResourceHandle.AccessLevel: {meshProvider.ResourceHandle.AccessLevel}");
                                    ImGui.Text($"ResourceHandle.EventListenerCount: {meshProvider.ResourceHandle.EventListenerCount}");
                                }

                                // CollisionMeshProvider

                                if (meshProvider is CollisionMeshProvider)
                                {
                                    var _curMeshProvider = (CollisionMeshProvider)meshProvider;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ImGui.Text("No entity selected.");
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
