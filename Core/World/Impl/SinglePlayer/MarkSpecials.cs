﻿using Helion.Geometry.Vectors;
using Helion.Maps.Specials;
using Helion.Util;
using Helion.Util.Configs.Impl;
using Helion.Util.Container;
using Helion.World.Entities;
using Helion.World.Entities.Players;
using Helion.World.Geometry.Lines;
using Helion.World.Geometry.Sectors;
using Helion.World.Geometry.Walls;
using Helion.World.Special;
using Helion.World.Special.Switches;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helion.World.Impl.SinglePlayer;

public class MarkSpecials
{
    public readonly DynamicArray<Sector> MarkedSectors = new();
    public readonly DynamicArray<Line> MarkedLines = new();
    private readonly DynamicArray<int> m_playerTracers = new();
    private readonly Vec3F[] TracerColors = new Vec3F[] { new(0.2f, 0.2f, 1f), new(0.2f, 1f, 0.2f), new(1f, 0.2f, 0.2f), new(0.8f, 0.8f, 0.8f) };
    private int m_developerMarkedLineId = -1;
    private int m_tracerColor;

    public void Mark(IWorld world, Entity entity, Line line)
    {
        if (!world.Config.Game.MarkSpecials || entity.PlayerObj == null || entity.PlayerObj.IsVooDooDoll)
            return;

        if (line.Id == m_developerMarkedLineId)
            return;

        var player = entity.PlayerObj;

        ClearMarkedSectors();
        ClearMarkedLines();
        ClearPlayerTracers(player);
        MarkSpecialLines(world, line);

        if (line.HasSpecial)
        {
            var sectors = world.SpecialManager.GetSectorsFromSpecialLine(line);
            for (int i = 0; i < sectors.Count; i++)
            {
                var sector = sectors.GetSector(i);
                sector.MarkAutomap = true;
                MarkedSectors.Add(sector);
                if (!SectorHasLine(sector, line))
                {
                    sector.ActivatedByLineId = line.Id;
                    ConnectLineToSector(world, player, line, sector);
                }
                world.DisplayMessage($"Line {line.Id} activates sector: {sector.Id} - {GetLineSpecialDescritpion(line)}");
            }
        }

        if (MarkedLines.Length > 0)
        {
            Sector? markSector = null;
            if (line.Front.Sector.Tag != 0)
                markSector = line.Front.Sector;
            else if (line.Back != null && line.Back.Sector.Tag != 0)
                markSector = line.Back.Sector;

            if (markSector != null)
            {
                for (int i = 0; i < MarkedLines.Length; i++)
                {
                    var markLine = MarkedLines[i];
                    markSector.MarkAutomap = true;
                    MarkedSectors.Add(markSector);
                    if (!SectorHasLine(markSector, markLine))
                    {
                        markSector.ActivatedByLineId = markLine.Id;
                        ConnectLineToSector(world, player, markLine, markSector);
                    }
                    world.DisplayMessage($"Sector {markSector.Id} activated by line: {markLine.Id} - {GetLineSpecialDescritpion(markLine)}");
                }
            }
        }

        if (MarkedLines.Length > 0 || MarkedSectors.Length > 0)
        {
            m_developerMarkedLineId = line.Id;
            MarkedLines.Add(line);
            line.MarkAutomap = true;
            return;
        }

        m_developerMarkedLineId = -1;
    }

    private void ConnectLineToSector(IWorld world, Player player, Line line, Sector sector)
    {
        m_tracerColor = ++m_tracerColor % TracerColors.Length;
        Vec3D start = GetActivatedLinePoint(world, line);
        var box = sector.GetBoundingBox();
        Vec3D end = new((box.Min.X + box.Max.X) / 2, (box.Min.Y + box.Max.Y) / 2, Math.Min(sector.Floor.Z + 8, sector.Ceiling.Z));
        m_playerTracers.Add(player.Tracers.AddTracer((start, end), world.Gametick, TracerColors[m_tracerColor], int.MaxValue));
    }

    private static bool SectorHasLine(Sector sector, Line line)
    {
        for (int i = 0; i < sector.Lines.Count; i++)
        {
            var sectorLine = sector.Lines[i];
            if (sectorLine.Id == line.Id)
                return true;
        }

        return false;
    }

    private void ClearPlayerTracers(Player player)
    {
        for (int i = 0; i < m_playerTracers.Length; i++)
            player.Tracers.RemoveTracer(m_playerTracers[i]);
        m_playerTracers.Clear();
    }

    private static Vec3D GetActivatedLinePoint(IWorld world, Line line)
    {
        var lineCenter = line.Segment.FromTime(0.5);
        var lineAngle = line.Segment.Start.Angle(line.Segment.End);
        lineCenter = lineCenter + Vec2D.UnitCircle(lineAngle - MathHelper.HalfPi) * 8;

        if (line.Back == null)
            return lineCenter.To3D((line.Front.Sector.Floor.Z + line.Front.Sector.Ceiling.Z) / 2);

        if (SwitchManager.IsLineSwitch(world.ArchiveCollection, line))
        {
            var location = SwitchManager.GetLineLineSwitchTexture(world.ArchiveCollection, line, false);
            switch (location.Item2)
            {
                case WallLocation.Upper:
                    return lineCenter.To3D((line.Back.Sector.Ceiling.Z + line.Front.Sector.Ceiling.Z) / 2);
                case WallLocation.Middle:
                    return lineCenter.To3D((line.Back.Sector.Floor.Z + line.Back.Sector.Ceiling.Z) / 2);
                case WallLocation.Lower:
                    return lineCenter.To3D((line.Front.Sector.Floor.Z + line.Back.Sector.Floor.Z) / 2);
            }
        }

        return lineCenter.To3D(Math.Min(line.Front.Sector.Floor.Z + 8, line.Front.Sector.Ceiling.Z));
    }

    private static string GetLineSpecialDescritpion(Line line) =>
        $"[{(int)line.Special.LineSpecialType}]{line.Special.LineSpecialType} - {GetArgs(line)} - {GetActivations(line)} - Activated[{GetIntBool(line.Activated)}] Repeat[{GetIntBool(line.Flags.Repeat)}]";

    private static object GetArgs(Line line) =>
        $"{line.Args.Arg0},{line.Args.Arg1},{line.Args.Arg2},{line.Args.Arg3},{line.Args.Arg4}";

    private static int GetIntBool(bool b) => b ? 1 : 0;

    private static string GetActivations(Line line)
    {
        StringBuilder sb = new();
        for (int i = 0; i < 32; i++)
        {
            int flag = 1 << i;
            if (((int)line.Flags.Activations & flag) != 0)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append((LineActivations)flag);
            }
        }
        return sb.ToString();
    }

    private void MarkSpecialLines(IWorld world, Line sourceLine)
    {
        int frontTag = sourceLine.Front.Sector.Tag;
        int backTag = sourceLine.Back == null ? 0 : sourceLine.Back.Sector.Tag;

        for (int i = 0; i < world.Lines.Count; i++)
        {
            Line line = world.Lines[i];
            if (!line.HasSpecial)
                continue;

            if (line.SectorTag == 0)
                continue;

            if (line.SectorTag != frontTag && line.SectorTag != backTag)
                continue;

            line.MarkAutomap = true;
            MarkedLines.Add(line);
        }
    }

    private void ClearMarkedLines()
    {
        for (int i = 0; i < MarkedLines.Length; i++)
            MarkedLines[i].MarkAutomap = false;
        MarkedLines.Clear();
    }
    private void ClearMarkedSectors()
    {
        for (int i = 0; i < MarkedSectors.Length; i++)
        {
            MarkedSectors[i].ActivatedByLineId = -1;
            MarkedSectors[i].MarkAutomap = false;
        }
        MarkedSectors.Clear();
    }
}