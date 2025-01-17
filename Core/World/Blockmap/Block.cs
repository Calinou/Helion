using System.Collections.Generic;
using Helion.Geometry;
using Helion.Geometry.Boxes;
using Helion.Geometry.Segments;
using Helion.Geometry.Vectors;
using Helion.Util.Container;
using Helion.World.Entities;
using Helion.World.Geometry.Lines;
using Helion.World.Geometry.Sectors;
using Helion.World.Geometry.Sides;
using static Helion.Util.Assertion.Assert;

namespace Helion.World.Blockmap;

/// <summary>
/// Represents a cell in the blockmap.
/// </summary>
/// 
public struct BlockLine
{
    public int LineId;
    public Seg2D Segment;
    public bool OneSided;
    public LineFlags Flags;
    public Line Line;
    public Sector FrontSector;
    public Sector? BackSector;

    public BlockLine(Seg2D segment, Line line, bool oneSided, Sector frontSector, Sector? backSector)
    {
        LineId = line.Id;
        Segment = segment;
        Flags = line.Flags;
        Line = line;
        OneSided = oneSided;
        FrontSector = frontSector;
        BackSector = backSector;
    }
}

public class Block
{
    /// <summary>
    /// All the lines for this block.
    /// </summary>  
    public readonly DynamicArray<BlockLine> BlockLines = new(arrayPool: false);

    /// <summary>
    /// All the entities in this block.
    /// </summary>
    public readonly LinkableList<Entity> Entities = new();
    public readonly LinkableList<Sector> DynamicSectors = new();
    public readonly LinkableList<Side> DynamicSides = new();

    public Entity? HeadEntity;

    public Box2D Box;

    /// <summary>
    /// Gets the block X coordinate, assuming the coordinate was set.
    /// </summary>
    public int X => m_coordinate.X;

    /// <summary>
    /// Gets the block Y coordinate, assuming the coordinate was set.
    /// </summary>
    public int Y => m_coordinate.Y;

    private Vec2I m_coordinate = Vec2I.Zero;

    /// <summary>
    /// Sets the internal coordinates for this block.
    /// </summary>
    /// <remarks>
    /// We are stuck with this because we can't do this in the constructor
    /// as this is passed in as a generic value to a UniformGrid. The only
    /// other way is to make it have some kind of interface and constrain
    /// it to that, but performance needs to be investigated first before
    /// doing that.
    /// </remarks>
    /// <param name="x">The X coordinate, which should not be negative.
    /// </param>
    /// <param name="y">The Y coordinate, which should not be negative.
    /// </param>
    internal void SetCoordinate(int x, int y, int dimension, Vec2D origin)
    {
        Precondition(x >= 0, "Cannot have a negative blockmap X index");
        Precondition(y >= 0, "Cannot have a negative blockmap Y index");

        m_coordinate = new Vec2I(x, y);

        Vec2D point = new Vec2D(x * dimension, y * dimension) + origin;
        Box = new(point, point + (dimension, dimension));
    }

    public void AddLink(Entity entity)
    {
        if (HeadEntity == null)
        {
            HeadEntity = entity;
            return;
        }

        entity.RenderBlockNext = HeadEntity;
        HeadEntity.RenderBlockPrevious = entity;
        HeadEntity = entity;
    }

    public void RemoveLink(Entity entity)
    {
        if (entity == HeadEntity)
        {
            HeadEntity = entity.RenderBlockNext;
            if (HeadEntity != null)
                HeadEntity.RenderBlockPrevious = null;
            entity.RenderBlockNext = null;
            entity.RenderBlockPrevious = null;
            return;
        }

        if (entity.RenderBlockNext != null)
            entity.RenderBlockNext.RenderBlockPrevious = entity.RenderBlockPrevious;
        if (entity.RenderBlockPrevious != null)
            entity.RenderBlockPrevious.RenderBlockNext = entity.RenderBlockNext;

        entity.RenderBlockNext = null;
        entity.RenderBlockPrevious = null;
    }
}
