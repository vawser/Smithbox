using StudioCore.MsbEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    public class SkeletonHandler
    {
        private ModelEditorScreen Screen;
        private Universe Universe;

        public SkeletonHandler(ModelEditorScreen screen, Universe universe) 
        {
            Screen = screen;
            Universe = universe;
        }

        // Generate the 'skeleton' via the node connections
        public void GenerateSkeleton()
        {

        }

        // Display the 'skeleton' renderables if view skeleton is enabled
        public void OnGui()
        {
            if(CFG.Current.ModelEditor_ViewSkeleton)
            {
                if (Universe.LoadedModelContainers.ContainsKey(Screen.ViewportHandler.ContainerID))
                {
                    var container = Universe.LoadedModelContainers[Screen.ViewportHandler.ContainerID];
                    foreach (var entry in container.Bone_RootNode.Children)
                    {

                    }
                }
            }
        }
    }
}
