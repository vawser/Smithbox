using StudioCore.Application;

namespace StudioCore.Editors.ModelEditor;

public class ModelSelection
{
    public ModelContainerWrapper SelectedModelContainerWrapper;
    public ModelWrapper SelectedModelWrapper;

    public bool IsAnyModelLoaded()
    {
        if(SelectedModelWrapper != null)
        {
            return true;
        }

        return false;
    }
}
