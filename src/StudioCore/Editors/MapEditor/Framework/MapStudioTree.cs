using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor.Framework;

/// <summary>
/// A tree hierarchy of axis-aligned bounding boxes used in various calculations such as drawing, culling, and collision detection.
/// </summary>
public class MapStudioTree : IMsbTree
{
    /// <summary>
    /// The bounds of this node.
    /// </summary>
    public BoundingBox Bounds { get; set; }
    MsbBoundingBox IMsbTree.Bounds { get => LocalBoxToMsbBox(Bounds); set => Bounds = MsbBoxToLocalBox(value); }

    /// <summary>
    /// The values in this node.
    /// </summary>
    public List<short> PartIndices { get; set; }

    /// <summary>
    /// The left child of this node.
    /// </summary>
    public MapStudioTree Left { get; set; }
    IMsbTree IMsbTree.Left { get => Left; set => Left = (MapStudioTree)value; }

    /// <summary>
    /// The right child of this node.
    /// </summary>
    public MapStudioTree Right { get; set; }
    IMsbTree IMsbTree.Right { get => Right; set => Right = (MapStudioTree)value; }

    /// <summary>
    /// Create a new <see cref="MapStudioTree"/>.
    /// </summary>
    public MapStudioTree()
    {
        PartIndices = new List<short>();
        Bounds = new BoundingBox();
        Left = null;
        Right = null;
    }

    /// <summary>
    /// Create a new <see cref="MapStudioTree"/>.
    /// </summary>
    /// <param name="bounds">The bounds of the node.</param>
    /// <param name="partIndices">The values in the node.</param>
    /// <param name="left">The left child of the node.</param>
    /// <param name="right">The right child of the node.</param>
    public MapStudioTree(BoundingBox bounds, List<short> partIndices, MapStudioTree left = null, MapStudioTree right = null)
    {
        Bounds = bounds;
        PartIndices = partIndices;
        Left = left;
        Right = right;
    }

    /// <summary>
    /// Create a new <see cref="MapStudioTree"/>.
    /// </summary>
    /// <param name="min">The minimum extent of the bounds of the node.</param>
    /// <param name="max">The maximum extent of the bounds of the node.</param>
    /// <param name="partIndices">The values in the node.</param>
    /// <param name="left">The left child of the node.</param>
    /// <param name="right">The right child of the node.</param>
    public MapStudioTree(Vector3 min, Vector3 max, List<short> partIndices, MapStudioTree left = null, MapStudioTree right = null)
    {
        Bounds = new BoundingBox(min, max);
        PartIndices = partIndices;
        Left = left;
        Right = right;
    }

    /// <summary>
    /// Add a node to the tree, either replacing this node, the left node, or the right node, depending on volume cost.
    /// </summary>
    /// <param name="bounds">The bounds of the node to add.</param>
    /// <param name="partIndices">The values to add to the newly made node.</param>
    // Does not work well with MSB.
    // Credit for this algorithm goes to:
    // https://github.com/kip-hart/AABBTree
    public void AddByVolumeCost(BoundingBox bounds, List<short> partIndices)
    {
        var emptyBox = new BoundingBox();
        var left = Left == null ? emptyBox : Left.Bounds;
        var right = Right == null ? emptyBox : Right.Bounds;

        var rootMerge = BoundingBox.Combine(Bounds, bounds);
        var leftMerge = BoundingBox.Combine(left, bounds);
        var rightMerge = BoundingBox.Combine(right, bounds);

        var rootCost = rootMerge.GetVolume();
        var leftCost = rootCost + (leftMerge.GetVolume() - left.GetVolume());
        var rightCost = rootCost + (rightMerge.GetVolume() - right.GetVolume());

        var rootOverlapCost = BoundingBox.GetOverlapingVolume(Bounds, bounds);
        var leftOverlapCost = BoundingBox.GetOverlapingVolume(left, bounds);
        var rightOverlapCost = BoundingBox.GetOverlapingVolume(right, bounds);

        rootCost += rootOverlapCost;
        leftCost += leftOverlapCost;
        rightCost += rightOverlapCost;

        if (rootCost < leftCost && rootCost < rightCost)
        {
            // Move this left, add the new one right, then set part indices empty
            Left = new MapStudioTree(Bounds, PartIndices, Left, Right);
            Right = new MapStudioTree(bounds, partIndices);
            PartIndices = [];
        }
        else if (leftCost < rightCost)
        {
            // Add the new one to the left
            Left.AddByVolumeCost(bounds, partIndices);
        }
        else
        {
            // Add the new one to the right
            Right.AddByVolumeCost(bounds, partIndices);
        }

        left = Left == null ? emptyBox : Left.Bounds;
        right = Right == null ? emptyBox : Right.Bounds;
        Bounds = BoundingBox.Combine(left, right);
    }

    /// <summary>
    /// Adds a new part to the root node while expanding the bounds if need be.
    /// </summary>
    /// <param name="bounds">The bounds of the part to add.</param>
    /// <param name="partIndex">The part to add.</param>
    // TODO MapStudioTree make a better algorithm
    public void AddSimple(BoundingBox bounds, short partIndex)
    {
        PartIndices.Add(partIndex);

        var nMin = bounds.Min;
        var nMax = bounds.Max;

        var minX = Bounds.Min.X;
        var minY = Bounds.Min.Y;
        var minZ = Bounds.Min.Z;
        var maxX = Bounds.Max.X;
        var maxY = Bounds.Max.Y;
        var maxZ = Bounds.Max.Z;

        if (nMin.X < minX)
            minX = nMin.X;

        if (nMin.Y < minY)
            minY = nMin.Y;

        if (nMin.Z < minZ)
            minZ = nMin.Z;

        if (nMax.X > maxX)
            maxX = nMax.X;

        if (nMax.Y > maxY)
            maxY = nMax.Y;

        if (nMax.Z > maxZ)
            maxZ = nMax.Z;

        Bounds = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }

    /// <summary>
    /// Enlarges the bounding box of this node by the given amount.
    /// </summary>
    /// <param name="amount">The amount to enlarge by.</param>
    // TODO MapStudioTree make a better algorithm
    public void EnlargeBounds(float amount)
    {
        var minX = Bounds.Min.X - amount;
        var minY = Bounds.Min.Y - amount;
        var minZ = Bounds.Min.Z - amount;
        var maxX = Bounds.Max.X + amount;
        var maxY = Bounds.Max.Y + amount;
        var maxZ = Bounds.Max.Z + amount;

        Bounds = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }

    /// <summary>
    /// Convert this tree to another MapStudioTree type using the <see cref="IMsbTree"/> interface.
    /// </summary>
    /// <typeparam name="TMsbTree">A MapStudioTree type.</typeparam>
    /// <returns>A MapStudioTree.</returns>
    public TMsbTree ToMsbTree<TMsbTree>() where TMsbTree : IMsbTree, new()
    {
        var tree = new TMsbTree
        {
            Bounds = LocalBoxToMsbBox(Bounds),
            PartIndices = PartIndices
        };

        if (Left != null)
            tree.Left = Left.ToMsbTree<TMsbTree>();

        if (Right != null)
            tree.Right = Right.ToMsbTree<TMsbTree>();

        return tree;
    }

    /// <summary>
    /// Convert a <see cref="BoundingBox"/> to an <see cref="MsbBoundingBox"/>.
    /// </summary>
    /// <param name="box">A bounding box.</param>
    /// <returns>A bounding box.</returns>
    public static MsbBoundingBox LocalBoxToMsbBox(BoundingBox box)
        => new(box.Min, box.Max);

    /// <summary>
    /// Convert an <see cref="MsbBoundingBox"/> to a <see cref="BoundingBox"/>.
    /// </summary>
    /// <param name="box">A bounding box.</param>
    /// <returns>A bounding box.</returns>
    public static BoundingBox MsbBoxToLocalBox(MsbBoundingBox box)
        => new(box.Min, box.Max);
}

public struct MsbTreePartInfo(short index, Vector3 position, BoundingBox bounds)
{
    public short Index = index;
    public Vector3 Position = position;
    public BoundingBox Bounds = bounds;
    public float Radius = MsbBoundingBox.CalculateRadius(bounds.Min, bounds.Max);
}
