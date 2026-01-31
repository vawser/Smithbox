using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamImportPreviewModal
{
    private ParamDeltaPatcher Patcher;

    public string ModalName = "Import Preview";

    public bool DisplayModal = false;
    public bool InitialLayout = false;

    public ParamImportPreviewModal(ParamDeltaPatcher patcher)
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
