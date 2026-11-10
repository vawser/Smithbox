using Hexa.NET.DirectXTex;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using SoulsFormats;
using StudioCore.Keybinds;
using System;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TexProperties
{
    public TexEditorView Parent;
    public ProjectEntry Project;

    public TexProperties(TexEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display()
    {
        ImGui.BeginChild("TextureProperties", new Vector2(0, 0), ImGuiChildFlags.Borders);

        GUI.TitleHeader(
            LOC.Get("TEXVIEW_Properties_Header_Properties", Parent.Selection.SelectedTextureKey),
            LOC.Get("TEXVIEW_Properties_Header_Properties_TT"));

        GUI.WrappedText(
            LOC.Get("TEXVIEW_Properties_Controls_Hint",
            InputManager.GetHint(KeybindID.TextureViewer_Reset_Zoom_Level)));

        GUI.Spacer();

        float sizeX = -1;
        float sizeY = -1;
        Vector2 relativePos = new Vector2();
        string format = "";
        string iconName = "";

        if (Parent.Selection.SelectedTexture != null)
        {
            if (Parent.Selection.ViewerTextureResource != null)
            {
                Vector2 size = Parent.DisplayViewport.GetImageSize(
                    Parent.Selection.ViewerTextureResource, 
                    false);

                sizeX = size.X;
                sizeY = size.Y;

                relativePos = Parent.DisplayViewport.GetRelativePosition(
                    size,
                    Parent.Selection.TextureViewWindowPosition,
                    Parent.Selection.TextureViewScrollPosition);

                if (Parent.Selection.ViewerTextureResource.GPUTexture != null)
                {
                    format = $"{Parent.Selection.ViewerTextureResource.GPUTexture.Format}".ToUpper();
                }

                if (Project.Handler.TextureData.PrimaryBank.ShoeboxEntries != null)
                {
                    var shoeboxEntry = Project.Handler.TextureData.PrimaryBank.ShoeboxEntries.FirstOrDefault();

                    if (shoeboxEntry.Value != null)
                    {
                        if (shoeboxEntry.Value.Textures.ContainsKey(Parent.Selection.SelectedTextureKey))
                        {
                            var subTexs = shoeboxEntry.Value.Textures[Parent.Selection.SelectedTextureKey];
                            foreach (var entry in subTexs)
                            {
                                string IconName;
                                bool IsMatch;
                                (IconName, IsMatch) = Parent.DisplayViewport.MatchMousePosToIcon(entry, relativePos);

                                if (IsMatch)
                                {
                                    iconName = IconName;
                                }
                            }
                        }
                    }
                }
            }
        }

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders;

        // Display
        if (ImGui.BeginTable($"propertiesTable", 2, tblFlags))
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);

            var colFlags = ImGuiTableColumnFlags.WidthStretch;

            ImGui.TableSetupColumn("Property", colFlags, 0.4f);
            ImGui.TableSetupColumn("Value", colFlags);
            ImGui.TableSetupScrollFreeze(2, 1);

            // ID
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(LOC.Get("TEXVIEW_Properties_Column_Header_ID"));

            // Name
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(LOC.Get("TEXVIEW_Properties_Column_Header_Name"));

            // Size Y
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(LOC.Get("TEXVIEW_Properties_Column_Header_SizeY"));

            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{sizeY}");

            // Size X
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(LOC.Get("TEXVIEW_Properties_Column_Header_SizeX"));

            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{sizeX}");

            // Relative Position: X
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(LOC.Get("TEXVIEW_Properties_Column_Header_RelPosX"));

            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{relativePos.X}");

            // Relative Position: Y
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(LOC.Get("TEXVIEW_Properties_Column_Header_RelPosY"));

            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{relativePos.Y}");

            // Format
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(LOC.Get("TEXVIEW_Properties_Column_Header_Format"));

            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{format}");

            // Icon
            if (iconName != "")
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text(LOC.Get("TEXVIEW_Properties_Column_Header_Icon"));

                ImGui.TableSetColumnIndex(1);
                ImGui.Text($"{iconName}");
            }

            ImGui.EndTable();
        }

        DisplayTextureData();

        ImGui.EndChild();
    }

    // Edit buffers for the texture fields, reset when another texture is selected
    private TPF.Texture BufferTexture;
    private string NameBuffer = "";
    private int FormatBuffer;
    private int FlagsBuffer;
    private int FloatUnkBuffer;
    private float[] FloatValueBuffer = [];

    /// <summary>
    /// The selected texture's TPF fields (editable on PC TPFs) and its DDS header (read only).
    /// Each field is committed as one undoable change when the input loses focus.
    /// </summary>
    private void DisplayTextureData()
    {
        var sel = Parent.Selection;
        var tex = sel.SelectedTexture;

        if (tex == null || sel.SelectedTpf == null || !sel.SelectedTpf.Textures.Contains(tex))
            return;

        if (BufferTexture != tex)
        {
            ResetBuffers(tex);
        }

        var target = Parent.GetEditTarget();
        var info = TextureFormats.GetInfo(sel.SelectedTpf, tex);

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_Properties_Texture_Data_Header"), LOC.Get("TEXVIEW_Properties_Texture_Data_Header_TT"));

        if (target == null)
        {
            GUI.WrappedText(LOC.Get("TEXVIEW_TexImport_Read_Only_Platform", sel.SelectedTpf.Platform));
        }

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable("textureDataTable", 2, tblFlags))
        {
            ImGui.TableSetupColumn("Property", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

            // Name
            Row(LOC.Get("TEXVIEW_Properties_Texture_Name"), LOC.Get("TEXVIEW_Properties_Texture_Name_TT"));
            if (target != null)
            {
                ImGui.SetNextItemWidth(-1);
                ImGui.InputText("##textureName", ref NameBuffer, 255);
                GUI.Tooltip(LOC.Get("TEXVIEW_Properties_Texture_Name_TT"));

                if (ImGui.IsItemDeactivatedAfterEdit() && NameBuffer != tex.Name && NameBuffer.Trim() != "")
                {
                    Commit(target, tex, s => s.Name = NameBuffer.Trim());
                }
            }
            else
            {
                ImGui.Text(tex.Name);
            }

            // Format byte
            Row(LOC.Get("TEXVIEW_Properties_Texture_Format_Byte"), LOC.Get("TEXVIEW_Properties_Texture_Format_Byte_TT"));
            if (target != null)
            {
                ImGui.SetNextItemWidth(-1);
                ImGui.InputInt("##textureFormatByte", ref FormatBuffer);
                GUI.Tooltip(LOC.Get("TEXVIEW_Properties_Texture_Format_Byte_TT"));

                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    FormatBuffer = Math.Clamp(FormatBuffer, 0, 255);
                    if (FormatBuffer != tex.Format)
                        Commit(target, tex, s => s.Format = (byte)FormatBuffer);
                }
            }
            else
            {
                ImGui.Text($"{tex.Format}");
            }

            // Flags1
            Row(LOC.Get("TEXVIEW_Properties_Texture_Flags1"), LOC.Get("TEXVIEW_Properties_Texture_Flags1_TT"));
            if (target != null)
            {
                ImGui.SetNextItemWidth(-1);
                ImGui.InputInt("##textureFlags1", ref FlagsBuffer);
                GUI.Tooltip(LOC.Get("TEXVIEW_Properties_Texture_Flags1_TT"));

                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    // The values the TPF reader accepts
                    if (FlagsBuffer is 0 or 1 or 2 or 3 or 0x80)
                    {
                        if (FlagsBuffer != tex.Flags1)
                            Commit(target, tex, s => s.Flags1 = (byte)FlagsBuffer);
                    }
                    else
                    {
                        FlagsBuffer = tex.Flags1;
                    }
                }
            }
            else
            {
                ImGui.Text($"0x{tex.Flags1:X2}");
            }

            // Type, mips, DDS header: read only, they come from the image
            Row(LOC.Get("TEXVIEW_Properties_Texture_Type"), LOC.Get("TEXVIEW_Properties_Texture_Type_TT"));
            ImGui.Text($"{tex.Type}");

            Row(LOC.Get("TEXVIEW_Properties_Texture_Mipmaps"), LOC.Get("TEXVIEW_Properties_Texture_Mipmaps_TT"));
            ImGui.Text($"{tex.Mipmaps}");

            if (info.Readable)
            {
                Row(LOC.Get("TEXVIEW_Properties_Texture_DDS_Size"), LOC.Get("TEXVIEW_Properties_Texture_DDS_Size_TT"));
                ImGui.Text($"{info.Width} x {info.Height}, {info.MipLevels} mips");

                Row(LOC.Get("TEXVIEW_Properties_Texture_DDS_Format"), LOC.Get("TEXVIEW_Properties_Texture_DDS_Format_TT"));
                ImGui.Text($"{TextureFormats.GetLabel(info.Dxgi)} ({(info.Dx10Header ? "DX10" : "legacy")} header)");
            }

            Row(LOC.Get("TEXVIEW_Properties_Texture_Data_Size"), LOC.Get("TEXVIEW_Properties_Texture_Data_Size_TT"));
            ImGui.Text($"{tex.Bytes.Length:N0} bytes");

            // FloatStruct (DS3/BB GI textures, DS2 sky)
            if (tex.FloatStruct != null)
            {
                Row(LOC.Get("TEXVIEW_Properties_Texture_FloatStruct_Unk00"), LOC.Get("TEXVIEW_Properties_Texture_FloatStruct_TT"));
                if (target != null)
                {
                    ImGui.SetNextItemWidth(-1);
                    ImGui.InputInt("##floatStructUnk00", ref FloatUnkBuffer);
                    GUI.Tooltip(LOC.Get("TEXVIEW_Properties_Texture_FloatStruct_TT"));

                    if (ImGui.IsItemDeactivatedAfterEdit() && FloatUnkBuffer != tex.FloatStruct.Unk00)
                        Commit(target, tex, s => s.FloatStructUnk00 = FloatUnkBuffer);
                }
                else
                {
                    ImGui.Text($"{tex.FloatStruct.Unk00}");
                }

                for (int i = 0; i < FloatValueBuffer.Length && i < tex.FloatStruct.Values.Count; i++)
                {
                    Row(LOC.Get("TEXVIEW_Properties_Texture_FloatStruct_Value", i), LOC.Get("TEXVIEW_Properties_Texture_FloatStruct_TT"));
                    if (target != null)
                    {
                        ImGui.SetNextItemWidth(-1);
                        ImGui.InputFloat($"##floatStructValue{i}", ref FloatValueBuffer[i]);
                        GUI.Tooltip(LOC.Get("TEXVIEW_Properties_Texture_FloatStruct_TT"));

                        var index = i;
                        if (ImGui.IsItemDeactivatedAfterEdit() && FloatValueBuffer[i] != tex.FloatStruct.Values[i])
                            Commit(target, tex, s => s.FloatStructValues[index] = FloatValueBuffer[index]);
                    }
                    else
                    {
                        ImGui.Text($"{tex.FloatStruct.Values[i]}");
                    }
                }
            }

            ImGui.EndTable();
        }
    }

    private static void Row(string label, string tooltip)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text(label);
        GUI.Tooltip(tooltip);
        ImGui.TableSetColumnIndex(1);
    }

    private void ResetBuffers(TPF.Texture tex)
    {
        BufferTexture = tex;
        NameBuffer = tex.Name;
        FormatBuffer = tex.Format;
        FlagsBuffer = tex.Flags1;
        FloatUnkBuffer = tex.FloatStruct?.Unk00 ?? 0;
        FloatValueBuffer = tex.FloatStruct?.Values.ToArray() ?? [];
    }

    /// <summary>One undoable field change; the buffers are re-read from the texture afterwards.</summary>
    private void Commit(TextureTarget target, TPF.Texture tex, Action<TextureState> change)
    {
        var before = TextureState.Capture(tex);
        var after = before.Clone();
        change(after);

        Parent.ActionManager.ExecuteAction(new TextureChangeAction(Parent, target, tex, before, after));
        ResetBuffers(tex);
    }
}
