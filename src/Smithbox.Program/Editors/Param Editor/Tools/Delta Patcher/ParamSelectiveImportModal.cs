using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.Editors.ParamEditor;

public class ParamSelectiveImportModal
{
    private ParamDeltaPatcher Patcher;

    public string ModalName = "Selective Import";

    public bool DisplayModal = false;
    public bool InitialLayout = false;

    public ParamSelectiveImportModal(ParamDeltaPatcher patcher)
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
