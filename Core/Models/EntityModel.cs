using Helion.Geometry.Vectors;

namespace Helion.Models;

public class EntityModel
{
    public string Name { get; set; } = string.Empty;
    public int Id { get; set; }
    public int ThingId { get; set; }
    public double AngleRadians { get; set; }
    public EntityBoxModel Box { get; set; } = null!;
    public double SpawnPointX { get; set; }
    public double SpawnPointY { get; set; }
    public double SpawnPointZ { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double VelocityZ { get; set; }
    public int Health { get; set; }
    public int Armor { get; set; }
    public string? ArmorDefinition { get; set; }
    public int FrozenTics { get; set; }
    public int MoveCount { get; set; }
    public int Sector { get; set; }
    public int? Owner { get; set; }
    public int? Target { get; set; }
    public int? Tracer { get; set; }

    public bool Refire { get; set; }
    public bool MoveLinked { get; set; }
    public bool Respawn { get; set; }

    public int MoveDir { get; set; }
    public bool BlockFloat { get; set; }

    public FrameStateModel Frame { get; set; } = null!;
    public EntityFlagsModel Flags { get; set; } = null!;
    public int Threshold { get; set; }
    public int ReactionTime { get; set; }

    public int? HighSec { get; set; }
    public int? LowSec { get; set; }
    public int? HighEntity { get; set; }
    public int? LowEntity { get; set; }

    public Vec3D GetVelocity() => (VelocityX, VelocityY, VelocityZ);

    public Vec3D GetSpawnPoint() => (SpawnPointX, SpawnPointY, SpawnPointZ);
}
