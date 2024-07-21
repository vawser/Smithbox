using SoulsFormats;
using StudioCore.MsbEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    public class SkeletonHandler
    {
        private ModelEditorScreen Screen;
        private Universe Universe;

        private List<NodeConnection> Connections;

        public SkeletonHandler(ModelEditorScreen screen, Universe universe)
        {
            Screen = screen;
            Universe = universe;
        }

        // Generate the 'skeleton' via the node connections
        public void GenerateSkeleton()
        {
            // TODO
            /*
            if (Universe.LoadedModelContainers.ContainsKey(Screen.ViewportHandler.ContainerID))
            {
                var container = Universe.LoadedModelContainers[Screen.ViewportHandler.ContainerID];
                foreach (var entry in container.Bone_RootNode.Children)
                {
                    var node = (FLVER.Node)entry.WrappedObject;
                    var endNode = (FLVER.Node)container.Bone_RootNode.Children[node.FirstChildIndex].WrappedObject;
                    var nodeConnection = new NodeConnection(node, node.Position, endNode.Position);
                    Connections.Add(nodeConnection);
                }
            }
            */
        }

        // Display the 'skeleton' renderables if view skeleton is enabled
        public void OnGui()
        {
            if (CFG.Current.ModelEditor_ViewSkeleton)
            {

            }
        }
    }

    public class NodeConnection
    {
        public FLVER.Node StartNode;
        public Vector3 StartPosition;
        public Vector3 EndPosition;

        public NodeConnection(FLVER.Node node, Vector3 startPos, Vector3 endPos)
        {
            StartNode = node;
            StartPosition = startPos;
            EndPosition = endPos;
        }
    }
}
