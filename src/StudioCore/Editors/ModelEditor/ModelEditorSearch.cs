using StudioCore.Formats.PureFLVER;
using StudioCore.Formats.PureFLVER.FLVER2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    public static class ModelEditorSearch
    {
        public static bool IsModelEditorSearchMatch_Dummy(string rawInput, FLVER.Dummy dummy, FLVER2 currentFlver, int index)
        {
            bool match = false;

            string input = rawInput.Trim().ToLower();

            if (input.Equals(""))
            {
                match = true; // If input is empty, show all
                return match;
            }

            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];


                if (CFG.Current.ModelEditor_ExactSearch)
                {
                    if (index.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (dummy.ReferenceID.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (dummy.AttachBoneIndex.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }
                    if (dummy.ParentBoneIndex.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (dummy.AttachBoneIndex < currentFlver.Nodes.Count)
                    {
                        if (dummy.AttachBoneIndex > -1)
                        {
                            var node = currentFlver.Nodes[dummy.AttachBoneIndex];

                            if (node.Name.ToLower() == entry)
                            {
                                partTruth[i] = true;
                            }
                        }
                    }

                    if (dummy.ParentBoneIndex < currentFlver.Nodes.Count)
                    {
                        if (dummy.ParentBoneIndex > -1)
                        {
                            var node = currentFlver.Nodes[dummy.ParentBoneIndex];

                            if (node.Name.ToLower() == entry)
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                }
                else
                {
                    if (index.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (dummy.ReferenceID.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (dummy.AttachBoneIndex.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }
                    if (dummy.ParentBoneIndex.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (dummy.AttachBoneIndex < currentFlver.Nodes.Count)
                    {
                        if (dummy.AttachBoneIndex > -1)
                        {
                            var node = currentFlver.Nodes[dummy.AttachBoneIndex];

                            if (node.Name.ToLower().Contains(entry))
                            {
                                partTruth[i] = true;
                            }
                        }
                    }

                    if (dummy.ParentBoneIndex < currentFlver.Nodes.Count)
                    {
                        if (dummy.ParentBoneIndex > -1)
                        {
                            var node = currentFlver.Nodes[dummy.ParentBoneIndex];

                            if (node.Name.ToLower().Contains(entry))
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                }
            }

            match = true;

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    match = false;
            }

            return match;
        }

        public static bool IsModelEditorSearchMatch_Material(string rawInput, FLVER2.Material mat, FLVER2 currentFlver, int index)
        {
            bool match = false;

            string input = rawInput.Trim().ToLower();

            if (input.Equals(""))
            {
                match = true; // If input is empty, show all
                return match;
            }

            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (CFG.Current.ModelEditor_ExactSearch)
                {
                    if (index.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (mat.Name.ToLower() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (mat.MTD.ToLower() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (mat.Index.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    foreach (var tex in mat.Textures)
                    {
                        if (tex.Path == entry)
                        {
                            partTruth[i] = true;
                        }
                    }
                }
                else
                {
                    if (index.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (mat.Name.ToLower().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (mat.MTD.ToLower().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (mat.Index.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    foreach (var tex in mat.Textures)
                    {
                        if (tex.Path.Contains(entry))
                        {
                            partTruth[i] = true;
                        }
                    }
                }
            }

            match = true;

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    match = false;
            }

            return match;
        }

        public static bool IsModelEditorSearchMatch_GXList(string rawInput, FLVER2.GXList list, FLVER2 currentFlver, int index)
        {
            bool match = false;

            string input = rawInput.Trim().ToLower();

            if (input.Equals(""))
            {
                match = true; // If input is empty, show all
                return match;
            }

            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (CFG.Current.ModelEditor_ExactSearch)
                {
                    if (index.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    foreach (var item in list)
                    {
                        if (item.ID == entry)
                        {
                            partTruth[i] = true;
                        }
                    }
                }
                else
                {
                    if (index.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    foreach (var item in list)
                    {
                        if (item.ID.Contains(entry))
                        {
                            partTruth[i] = true;
                        }
                    }
                }
            }

            match = true;

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    match = false;
            }

            return match;
        }
        public static bool IsModelEditorSearchMatch_Node(string rawInput, FLVER.Node node, FLVER2 currentFlver, int index)
        {
            bool match = false;

            string input = rawInput.Trim().ToLower();

            if (input.Equals(""))
            {
                match = true; // If input is empty, show all
                return match;
            }

            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (CFG.Current.ModelEditor_ExactSearch)
                {
                    if (index.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (node.Name.ToLower() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (node.ParentIndex.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }
                    if (node.FirstChildIndex.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }
                    if (node.NextSiblingIndex.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }
                    if (node.PreviousSiblingIndex.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (node.ParentIndex < currentFlver.Nodes.Count)
                    {
                        if (node.ParentIndex > -1)
                        {
                            var tNode = currentFlver.Nodes[node.ParentIndex];

                            if (tNode.Name.ToLower() == entry)
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                    if (node.FirstChildIndex < currentFlver.Nodes.Count)
                    {
                        if (node.FirstChildIndex > -1)
                        {
                            var tNode = currentFlver.Nodes[node.FirstChildIndex];

                            if (tNode.Name.ToLower() == entry)
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                    if (node.NextSiblingIndex < currentFlver.Nodes.Count)
                    {
                        if (node.NextSiblingIndex > -1)
                        {
                            var tNode = currentFlver.Nodes[node.NextSiblingIndex];

                            if (tNode.Name.ToLower() == entry)
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                    if (node.PreviousSiblingIndex < currentFlver.Nodes.Count)
                    {
                        if (node.PreviousSiblingIndex > -1)
                        {
                            var tNode = currentFlver.Nodes[node.PreviousSiblingIndex];

                            if (tNode.Name.ToLower() == entry)
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                }
                else
                {
                    if (index.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (node.Name.ToLower().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (node.ParentIndex.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }
                    if (node.FirstChildIndex.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }
                    if (node.NextSiblingIndex.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }
                    if (node.PreviousSiblingIndex.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (node.ParentIndex < currentFlver.Nodes.Count)
                    {
                        if (node.ParentIndex > -1)
                        {
                            var tNode = currentFlver.Nodes[node.ParentIndex];

                            if (tNode.Name.ToLower().Contains(entry))
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                    if (node.FirstChildIndex < currentFlver.Nodes.Count)
                    {
                        if (node.FirstChildIndex > -1)
                        {
                            var tNode = currentFlver.Nodes[node.FirstChildIndex];

                            if (tNode.Name.ToLower().Contains(entry))
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                    if (node.NextSiblingIndex < currentFlver.Nodes.Count)
                    {
                        if (node.NextSiblingIndex > -1)
                        {
                            var tNode = currentFlver.Nodes[node.NextSiblingIndex];

                            if (tNode.Name.ToLower().Contains(entry))
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                    if (node.PreviousSiblingIndex < currentFlver.Nodes.Count)
                    {
                        if (node.PreviousSiblingIndex > -1)
                        {
                            var tNode = currentFlver.Nodes[node.PreviousSiblingIndex];

                            if (tNode.Name.ToLower().Contains(entry))
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                }
            }

            match = true;

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    match = false;
            }

            return match;
        }

        public static bool IsModelEditorSearchMatch_Mesh(string rawInput, FLVER2.Mesh mesh, FLVER2 currentFlver, int index)
        {
            bool match = false;

            string input = rawInput.Trim().ToLower();

            if (input.Equals(""))
            {
                match = true; // If input is empty, show all
                return match;
            }

            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (CFG.Current.ModelEditor_ExactSearch)
                {
                    if (index.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (mesh.MaterialIndex.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (mesh.MaterialIndex < currentFlver.Materials.Count)
                    {
                        if (mesh.MaterialIndex > -1)
                        {
                            var tMat = currentFlver.Materials[mesh.MaterialIndex];

                            if (tMat.Name.ToLower() == entry)
                            {
                                partTruth[i] = true;
                            }
                        }
                    }

                    if (mesh.MaterialIndex.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (mesh.NodeIndex < currentFlver.Nodes.Count)
                    {
                        if (mesh.NodeIndex > -1)
                        {
                            var tMat = currentFlver.Nodes[mesh.NodeIndex];

                            if (tMat.Name.ToLower() == entry)
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                }
                else
                {
                    if (index.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (mesh.MaterialIndex.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (mesh.MaterialIndex < currentFlver.Materials.Count)
                    {
                        if (mesh.MaterialIndex > -1)
                        {
                            var tMat = currentFlver.Materials[mesh.MaterialIndex];

                            if (tMat.Name.ToLower().Contains(entry))
                            {
                                partTruth[i] = true;
                            }
                        }
                    }

                    if (mesh.MaterialIndex.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (mesh.NodeIndex < currentFlver.Nodes.Count)
                    {
                        if (mesh.NodeIndex > -1)
                        {
                            var tMat = currentFlver.Nodes[mesh.NodeIndex];

                            if (tMat.Name.ToLower().Contains(entry))
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                }
            }

            match = true;

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    match = false;
            }

            return match;
        }

        public static bool IsModelEditorSearchMatch_BufferLayout(string rawInput, FLVER2.BufferLayout layout, FLVER2 currentFlver, int index)
        {
            bool match = false;

            string input = rawInput.Trim().ToLower();

            if (input.Equals(""))
            {
                match = true; // If input is empty, show all
                return match;
            }

            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (CFG.Current.ModelEditor_ExactSearch)
                {
                    if (index.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    foreach (var item in layout)
                    {
                        if (item.Index.ToString() == entry)
                        {
                            partTruth[i] = true;
                        }
                    }
                }
                else
                {
                    if (index.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    foreach (var item in layout)
                    {
                        if (item.Index.ToString().Contains(entry))
                        {
                            partTruth[i] = true;
                        }
                    }
                }
            }

            match = true;

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    match = false;
            }

            return match;
        }

        public static bool IsModelEditorSearchMatch_SkeletonBone(string rawInput, FLVER2.SkeletonSet.Bone bone, FLVER2 currentFlver, int index)
        {
            bool match = false;

            string input = rawInput.Trim().ToLower();

            if (input.Equals(""))
            {
                match = true; // If input is empty, show all
                return match;
            }

            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (CFG.Current.ModelEditor_ExactSearch)
                {
                    if (index.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (bone.NodeIndex.ToString() == entry)
                    {
                        partTruth[i] = true;
                    }

                    if (bone.NodeIndex < currentFlver.Nodes.Count)
                    {
                        if (bone.NodeIndex > -1)
                        {
                            var tNode = currentFlver.Nodes[bone.NodeIndex];

                            if (tNode.Name.ToLower() == entry)
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                }
                else
                {
                    if (index.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (bone.NodeIndex.ToString().Contains(entry))
                    {
                        partTruth[i] = true;
                    }

                    if (bone.NodeIndex < currentFlver.Nodes.Count)
                    {
                        if (bone.NodeIndex > -1)
                        {
                            var tNode = currentFlver.Nodes[bone.NodeIndex];

                            if (tNode.Name.ToLower().Contains(entry))
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                }
            }

            match = true;

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    match = false;
            }

            return match;
        }
    }
}
