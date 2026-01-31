using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ParamExportPreviewModal
{
    private ParamDeltaPatcher Patcher;

    public string ModalName = "Export Preview";

    public bool DisplayModal = false;
    public bool InitialLayout = false;

    public ParamExportPreviewModal(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
    }
    public void Draw()
    {
        if (!DisplayModal)
            return;

        var popupName = $"{ModalName}##{ModalName.GetHashCode()}";

        ImGui.OpenPopup(popupName);

        if (!InitialLayout)
        {
            UIHelper.SetupPopupWindow();
            InitialLayout = true;
        }

        if (ImGui.BeginPopupModal(popupName,
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            // TODO

            ImGui.EndPopup();
        }
    }
}
