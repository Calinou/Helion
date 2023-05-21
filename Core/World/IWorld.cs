using System;
using System.Collections.Generic;
using Helion.Geometry.Vectors;
using Helion.Util.Configs;
using Helion.Util.RandomGenerators;
using Helion.World.Bsp;
using Helion.World.Entities;
using Helion.World.Entities.Players;
using Helion.World.Geometry.Lines;
using Helion.World.Geometry.Sectors;
using Helion.World.Geometry.Sides;
using Helion.World.Geometry.Walls;
using Helion.World.Physics;
using Helion.World.Physics.Blockmap;
using Helion.World.Sound;
using Helion.Resources.Definitions.MapInfo;
using Helion.Resources.Archives.Collection;
using Helion.Util.Container;
using Helion.Models;
using Helion.World.Entities.Definition.Flags;
using Helion.World.Cheats;
using Helion.World.Stats;
using Helion.World.Special;
using Helion.World.Entities.Definition;
using Helion.World.Entities.Definition.States;
using Helion.Util;
using Helion.World.Special.Specials;
using Helion.Resources;
using Helion.World.Static;
using Helion.World.Blockmap;
using Helion.Maps.Specials;

namespace Helion.World;

public delegate double GetTracerVelocityZ(Entity tracer, Entity target);

public interface IWorld : IDisposable
{
    event EventHandler<LevelChangeEvent>? LevelExit;
    event EventHandler? WorldResumed;
    event EventHandler? ClearConsole;
    event EventHandler? OnResetInterpolation;
    event EventHandler<SectorPlane>? SectorMoveStart;
    event EventHandler<SectorPlane>? SectorMoveComplete;
    event EventHandler<SideTextureEvent>? SideTextureChanged;
    event EventHandler<PlaneTextureEvent>? PlaneTextureChanged;
    event EventHandler<Sector>? SectorLightChanged;
    event EventHandler<SideScrollEvent>? SideScrollChanged;
    event EventHandler<SectorPlane> SectorPlaneScrollChanged;
    event EventHandler<PlayerMessageEvent>? PlayerMessage;
    event EventHandler? OnTick;
    event EventHandler? OnDestroying;

    string MapName { get; }
    // Increments every tick unless the game is paused.
    int Gametick { get; }
    // Increments every tick, even if the game is paused;
    int GameTicker { get; }
    int LevelTime { get; }
    double Gravity { get; }
    WorldState WorldState { get; }
    IList<Line> Lines { get; }
    IList<Side> Sides { get; }
    IList<Wall> Walls { get; }
    IList<Sector> Sectors { get; }
    CompactBspTree BspTree { get; }
    LinkableList<Entity> Entities { get; }
    IRandom Random { get; }
    // Used for randomization that should not affect demos
    IRandom SecondaryRandom { get; }
    EntityManager EntityManager { get; }
    WorldSoundManager SoundManager { get; }
    BlockmapTraverser BlockmapTraverser { get; }
    BlockMap RenderBlockmap { get; }
    SpecialManager SpecialManager { get; }
    TextureManager TextureManager { get; }
    IConfig Config { get; }
    SkillDef SkillDefinition { get; }
    ArchiveCollection ArchiveCollection { get; }
    MapInfoDef MapInfo { get; }
    GameInfoDef GameInfo { get; }
    LevelStats LevelStats { get; }
    bool Paused { get; }
    bool DrawPause { get; }
    bool PlayingDemo { get; }
    GlobalData GlobalData { get; }
    CheatManager CheatManager { get; }
    DataCache DataCache { get; }
    Player Player { get; }
    bool IsFastMonsters { get; }
    int CheckCounter { get; set; }
    bool IsChaseCamMode { get; }
    bool DrawHud { get; }
    bool AnyLayerObscuring { get; set; }


    void Link(Entity entity);
    void Tick();
    void Pause(PauseOptions options = PauseOptions.None);
    void Resume();
    IEnumerable<Sector> FindBySectorTag(int tag);
    IEnumerable<Entity> FindByTid(int tid);
    IEnumerable<Line> FindByLineId(int lineId);
    void SetLineId(Line line, int lineId);
    void ExitLevel(LevelChangeType type, LevelChangeFlags flags = LevelChangeFlags.None);
    Entity[] GetBossTargets();
    int CurrentBossTarget { get; set; }
    void TelefragBlockingEntities(Entity entity);
    bool EntityUse(Entity entity);
    void OnTryEntityUseLine(Entity entity, Line line);
    bool CanActivate(Entity entity, Line line, ActivationContext context);
    bool ActivateSpecialLine(Entity entity, Line line, ActivationContext context);
    bool GetAutoAimEntity(Entity startEntity, in Vec3D start, double angle, double distance, out double pitch, out Entity? entity);
    Entity? FireProjectile(Entity shooter, double angle, double pitch, double autoAimDistance, bool autoAim, string projectClassName, out Entity? autoAimEntity,
        double addAngle = 0, double addPitch = 0, double zOffset = 0, bool decreaseAmmo = true);
    void FireHitscanBullets(Entity shooter, int bulletCount, double spreadAngleRadians, double spreadPitchRadians, double pitch, double distance, bool autoAim,
        Func<int>? damageFunc = null);
    Entity? FireHitscan(Entity shooter, double angle, double pitch, double distance, int damage);
    bool DamageEntity(Entity target, Entity? source, int damage, DamageType damageType, Thrust thrust = Thrust.HorizontalAndVertical, Sector? sectorSource = null);
    bool GiveItem(Player player, Entity item, EntityFlags? flags, out EntityDefinition definition, bool pickupFlash = true);
    void PerformItemPickup(Entity entity, Entity item);
    void HandleEntityHit(Entity entity, in Vec3D previousVelocity, TryMoveData? tryMove);
    void HandleEntityIntersections(Entity entity, in Vec3D previousVelocity, TryMoveData? tryMove);
    bool CheckLineOfSight(Entity from, Entity to);
    bool InFieldOfView(Entity from, Entity to, double fieldOfViewRadians);
    void RadiusExplosion(Entity damageSource, Entity attackSource, int radius, int maxDamage);
    TryMoveData TryMoveXY(Entity entity, Vec2D position);
    bool IsPositionValid(Entity entity, Vec2D position);
    SectorMoveStatus MoveSectorZ(double speed, double destZ, SectorMoveSpecial moveSpecial);
    void HandleEntityDeath(Entity deathEntity, Entity? deathSource, bool gibbed);
    void DisplayMessage(string message);
    void DisplayMessage(Player? player, Player? other, string message);
    // Checks if the entity will be blocked by another entity at the given position. Will use the entity definition's height and solid values.
    bool IsPositionBlockedByEntity(Entity entity, in Vec3D position);
    bool IsPositionBlocked(Entity entity);
    void CreateTeleportFog(in Vec3D pos, bool playSound = true);
    void ActivateCheat(Player player, ICheat cheat);
    bool IsSectorIdValid(int sectorId) => sectorId >= 0 && sectorId < Sectors.Count;
    bool IsLineIdValid(int lineId) => lineId >= 0 && lineId < Lines.Count;
    int EntityAliveCount(int entityDefinitionId);
    void NoiseAlert(Entity target, Entity source);
    void BossDeath(Entity entity);
    Player? GetLineOfSightPlayer(Entity entity, bool allaround);
    Entity? GetLineOfSightEnemy(Entity entity, bool allaround);
    double GetMoveFactor(Entity entity);
    bool HealChase(Entity entity, EntityFrame healState, string healSound);
    void TracerSeek(Entity entity, double threshold, double maxTurnAngle, GetTracerVelocityZ velocityZ);
    void SetNewTracerTarget(Entity entity, double fieldOfView, double radius);
    void SetSideTexture(Side side, WallLocation location, int textureHandle);
    void SetPlaneTexture(SectorPlane plane, int textureHandle);
    void SetSectorLightLevel(Sector sector, short lightLevel);
    void SetSideScroll(Side side, SideTexture textures);
    void SetSectorPlaneScroll(SectorPlane plane);
    void SetEntityPosition(Entity entity, Vec3D pos);
    void ToggleChaseCameraMode();
    void SectorInstantKillEffect(Entity entity, InstantKillEffect effect);

    WorldModel ToWorldModel();
    GameFilesModel GetGameFilesModel();
    Player GetCameraPlayer();
    ListenerParams GetListener();
}
