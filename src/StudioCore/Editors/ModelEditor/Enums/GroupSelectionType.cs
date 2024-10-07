using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.ModelEditor.Enums;

public enum GroupSelectionType
{
    [Display(Name = "None")] None,
    [Display(Name = "Header")] Header,
    [Display(Name = "Dummy")] Dummy,
    [Display(Name = "Material")] Material,
    [Display(Name = "GX List")] GXList,
    [Display(Name = "Node")] Node,
    [Display(Name = "Mesh")] Mesh,
    [Display(Name = "Buffer Layout")] BufferLayout,
    [Display(Name = "Base Skeleton")] BaseSkeleton,
    [Display(Name = "All Skeleton")] AllSkeleton,
    [Display(Name = "Low Collision")] CollisionLow,
    [Display(Name = "High Collision")] CollisionHigh,
    [Display(Name = "Internal File")] InternalFile
}
