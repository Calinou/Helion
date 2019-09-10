﻿using System;
using System.Collections.Generic;
using System.Linq;
using Helion.Bsp.Builder;
using Helion.Bsp.Builder.Stepwise;
using Helion.Bsp.Node;
using Helion.Maps;
using Helion.Maps.Components;
using Helion.Util;
using Helion.Util.Geometry;
using Helion.Util.Geometry.Boxes;
using Helion.Util.Geometry.Segments;
using Helion.Util.Geometry.Vectors;
using Helion.World.Geometry.Builder;
using Helion.World.Geometry.Lines;
using Helion.World.Geometry.Sectors;
using Helion.World.Geometry.Sides;
using Helion.World.Geometry.Subsectors;
using NLog;
using static Helion.Util.Assertion.Assert;

namespace Helion.World.Bsp
{
    /// <summary>
    /// The compiled BSP tree that condenses the builder data into a cache
    /// efficient data structure.
    /// </summary>
    public class BspTree
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// All the segments, which are the edges of the subsector.
        /// </summary>
        public List<SubsectorSegment> Segments = new List<SubsectorSegment>();

        /// <summary>
        /// All the subsectors, the convex leaves at the bottom of the BSP 
        /// tree.
        /// </summary>
        public Subsector[] Subsectors = new Subsector[0];

        /// <summary>
        /// A compact struct for all the nodes, specifically to speed up all
        /// recursive BSP traversal.
        /// </summary>
        public BspNodeCompact[] Nodes = new BspNodeCompact[0];

        /// <summary>
        /// The next available subsector index. This is used only for building 
        /// the <see cref="Subsectors"/> list.
        /// </summary>
        private uint m_nextSubsectorIndex;

        /// <summary>
        /// The next available node index. This is used only for building the 
        /// <see cref="Nodes"/> list.
        /// </summary>
        private uint m_nextNodeIndex;

        /// <summary>
        /// The root node of the tree.
        /// </summary>
        /// <remarks>
        /// This is the end index of the nodes array because the recursive
        /// traversal fills in the array from post-order traversal.
        /// </remarks>
        public BspNodeCompact Root => Nodes[^1];

        /// <summary>
        /// Gets the subsector that maps onto the point provided.
        /// </summary>
        /// <param name="point">The point to get the subsector for.</param>
        /// <returns>The subsector for the provided point.</returns>
        public Subsector ToSubsector(Vec2D point)
        {
            BspNodeCompact node = Root;

            while (true)
            {
                if (node.Splitter.OnRight(point))
                {
                    if (node.IsRightSubsector)
                        return Subsectors[node.RightChildAsSubsector];
                    node = Nodes[node.RightChild];
                }
                else
                {
                    if (node.IsLeftSubsector)
                        return Subsectors[node.LeftChildAsSubsector];
                    node = Nodes[node.LeftChild];
                }
            }
        }

        /// <summary>
        /// Gets the sector that maps onto the point provided.
        /// </summary>
        /// <param name="point">The point to get the sector for.</param>
        /// <returns>The sector for the provided point.</returns>
        public Sector ToSector(Vec2D point) => ToSubsector(point).Sector;
        
        /// <summary>
        /// Gets the sector that maps onto the point provided.
        /// </summary>
        /// <param name="point">The point to get the sector for.</param>
        /// <returns>The sector for the provided point.</returns>
        public Sector ToSector(Vec3D point) => ToSubsector(point.To2D()).Sector;
        
        private BspTree(BspNode root, GeometryBuilder builder)
        {
            Precondition(!root.IsDegenerate, "Cannot make a BSP tree from a degenerate build");
            
            CreateComponents(root, builder);
        }
        
        /// <summary>
        /// Creates a BSP from the map provided. This can fail if the geometry
        /// for the map is corrupt and we cannot make a BSP tree.
        /// </summary>
        /// <param name="map">The map to build the tree from.</param>
        /// <param name="builder">The geometry builder for the map.</param>
        /// <returns>A built BSP tree, or a null value if the geometry for the
        /// map is corrupt beyond repair.</returns>
        public static BspTree? Create(IMap map, GeometryBuilder builder)
        {
            IBspBuilder builderBase = new StepwiseBspBuilder(map);
            BspNode? root = builderBase.Build();
            if (root == null)
            {
                Log.Error("Cannot create BSP tree for map {0}, it is corrupt", map.Name);
                return null;
            }
            
            return new BspTree(root, builder);
        }
        
        private static Side? GetSideFromEdge(SubsectorEdge edge, GeometryBuilder builder)
        {
            if (edge.Line == null)
                return null;

            // This should never be wrong because the edge line ID's should be
            // shared with the instantiated lines.
            Invariant(edge.Line.Id < builder.Lines.Count, "Expected a continuous and well formed line ID range");
            Line line = builder.Lines[edge.Line.Id];

            Precondition(!(line.OneSided && !edge.IsFront), "Trying to get a back side for a one sided line");
            return edge.IsFront ? line.Front : line.Back;
        }

        private void CreateComponents(BspNode root, GeometryBuilder builder)
        {
            // Since it's a full binary tree, N nodes implies N + 1 leaves.
            int parentNodeCount = root.CalculateParentNodeCount();
            int subsectorNodeCount = parentNodeCount + 1;
            int segmentCountGuess = subsectorNodeCount * 4;

            Segments = new List<SubsectorSegment>(segmentCountGuess);
            Subsectors = new Subsector[subsectorNodeCount];
            Nodes = new BspNodeCompact[parentNodeCount];

            RecursivelyCreateComponents(root, builder);

            Postcondition(m_nextSubsectorIndex <= ushort.MaxValue, "Subsector index overflow (need a 4-byte BSP tree for this map)");
            Postcondition(m_nextNodeIndex <= ushort.MaxValue, "Node index overflow (need a 4-byte BSP tree for this map)");
        }

        private BspCreateResult RecursivelyCreateComponents(BspNode? node, GeometryBuilder builder)
        {
            if (node == null || node.IsDegenerate)
                throw new HelionException("Should never recurse onto a null/degenerate node when composing a world BSP tree");

            return node.IsSubsector ? CreateSubsector(node, builder) : CreateNode(node, builder);
        }

        private BspCreateResult CreateSubsector(BspNode node, GeometryBuilder builder)
        {
            List<SubsectorSegment> clockwiseSegments = CreateClockwiseSegments(node, builder);

            List<Seg2D> clockwiseDoubleSegments = clockwiseSegments.Cast<Seg2D>().ToList();
            Box2D bbox = Box2D.BoundSegments(clockwiseDoubleSegments);
            
            Sector sector = GetSectorFrom(node, builder);
            Subsectors[m_nextSubsectorIndex] = new Subsector((int)m_nextSubsectorIndex, sector, bbox, clockwiseSegments);

            return BspCreateResult.Subsector(m_nextSubsectorIndex++);
        }

        private List<SubsectorSegment> CreateClockwiseSegments(BspNode node, GeometryBuilder builder)
        {
            List<SubsectorSegment> returnSegments = new List<SubsectorSegment>();
         
            foreach (SubsectorEdge edge in node.ClockwiseEdges)
            {
                Side? side = GetSideFromEdge(edge, builder);
                SubsectorSegment subsectorEdge = new SubsectorSegment(Segments.Count, side, edge.Start, edge.End);
                
                returnSegments.Add(subsectorEdge);
                Segments.Add(subsectorEdge);
            }

            return returnSegments;
        }
        
        private Sector GetSectorFrom(BspNode node, GeometryBuilder builder)
        {
            foreach (SubsectorEdge edge in node.ClockwiseEdges)
            {
                ISector? edgeSector = edge.Sector;
                if (edgeSector == null)
                    continue;
                
                // If this ever is wrong, something has gone terribly wrong
                // with building the geometry.
                return builder.Sectors[edgeSector.Id];
            }

            throw new NullReferenceException("BSP building malformed, subsector made up of only minisegs (or is a not a leaf)");
        }

        private BspCreateResult CreateNode(BspNode node, GeometryBuilder builder)
        {
            if (node.Splitter == null)
                throw new NullReferenceException("Malformed BSP node, splitter should never be null");
            
            BspCreateResult left = RecursivelyCreateComponents(node.Left, builder);
            BspCreateResult right = RecursivelyCreateComponents(node.Right, builder);
            Box2D bbox = MakeBoundingBoxFrom(left, right);

            BspNodeCompact compactNode = new BspNodeCompact(left.IndexWithBit, right.IndexWithBit, node.Splitter, bbox);
            Nodes[m_nextNodeIndex] = compactNode;

            return BspCreateResult.Node(m_nextNodeIndex++);
        }

        private Box2D MakeBoundingBoxFrom(BspCreateResult left, BspCreateResult right)
        {
            Box2D leftBox = (left.IsSubsector ? Subsectors[left.Index].BoundingBox : Nodes[left.Index].BoundingBox);
            Box2D rightBox = (right.IsSubsector ? Subsectors[right.Index].BoundingBox : Nodes[right.Index].BoundingBox);
            return Box2D.Combine(leftBox, rightBox);
        }
    }
}