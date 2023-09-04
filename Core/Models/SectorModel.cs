namespace Helion.Models;

public class SectorModel
{
    public int Id;
    public int SoundValidationCount;
    public int SoundBlock;
    public int? SoundTarget;

    public int SectorDataChanges;
    public double? FloorZ;
    public double? CeilingZ;
    public short? LightLevel;
    public short? FloorLightLevel;
    public short? CeilingLightLevel;
    // Integer textures handles are deprecated here. Keeping for backwards compatibility.
    public int? FloorTexture;
    public int? CeilingTexture;
    public string? FloorTex;
    public string? CeilingTex;
    public int? SectorSpecialType;
    public int? SkyTexture;
    public bool Secret;
    public int DamageAmount;
    public int? TransferFloorLight;
    public int? TransferCeilingLight;
    public int? TransferHeights;
    public double? Friction;
}
