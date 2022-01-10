using System;
using System.Collections.Generic;
using System.Linq;
using Helion.Models;
using Helion.World.Entities.Definition;
using Helion.World.Entities.Definition.Composer;
using Helion.World.Entities.Definition.Flags;
using Helion.World.Entities.Inventories.Powerups;
using Helion.World.Entities.Players;

namespace Helion.World.Entities.Inventories;

public class Inventory
{
    public static readonly string AmmoClassName = "AMMO";
    public static readonly string BackPackBaseClassName = "BACKPACKITEM";
    public static readonly string WeaponClassName = "WEAPON";
    public static readonly string HealthClassName = "HEALTH";
    public static readonly string ArmorClassName = "ARMOR";
    public static readonly string BasicArmorBonusClassName = "BASICARMORBONUS";
    public static readonly string BasicArmorPickupClassName = "BASICARMORPICKUP";
    public static readonly string KeyClassName = "KEY";
    public static readonly string PowerupClassName = "POWERUPGIVER";
    public static readonly string RadSuitClassName = "RADSUIT";

    private static readonly List<string> PowerupEnumStringValues = GetPowerEnumValues();

    private static List<string> GetPowerEnumValues()
    {
        List<string> values = new();
        var enumValues = Enum.GetValues(typeof(PowerupType));
        foreach (PowerupType value in enumValues)
            values.Add(value.ToString());
        return values;
    }

    /// <summary>
    /// All of the items owned by the player that are not a special type of
    /// item (ex: weapons, which need more logic).
    /// </summary>
    private readonly Dictionary<string, InventoryItem> Items = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<InventoryItem> ItemList = new();
    private readonly List<InventoryItem> Keys = new();
    private readonly EntityDefinitionComposer EntityDefinitionComposer;
    private readonly Player Owner;

    /// <summary>
    /// All of the weapons owned by the player.
    /// </summary>
    public readonly Weapons Weapons;

    public readonly List<IPowerup> Powerups = new();

    public IPowerup? PowerupEffectColor { get; private set; }
    public IPowerup? PowerupEffectColorMap { get; private set; }

    public Inventory(Player owner, EntityDefinitionComposer composer)
    {
        Owner = owner;
        EntityDefinitionComposer = composer;
        Weapons = new Weapons(owner.World.GameInfo.WeaponSlots);
    }

    public Inventory(PlayerModel playerModel, Player owner, EntityDefinitionComposer composer)
    {
        Owner = owner;
        EntityDefinitionComposer = composer;
        Weapons = new Weapons(owner.World.GameInfo.WeaponSlots);

        for (int i = 0; i < playerModel.Inventory.Items.Count; i++)
        {
            InventoryItemModel item = playerModel.Inventory.Items[i];
            EntityDefinition? definition = EntityDefinitionComposer.GetByName(item.Name);
            if (definition != null)
            {
                InventoryItem inventoryItem = new(definition, item.Amount);
                AddItem(definition, inventoryItem);
            }
        }

        for (int i = 0; i < playerModel.Inventory.Weapons.Count; i++)
        {
            string weaponName = playerModel.Inventory.Weapons[i];
            EntityDefinition? definition = EntityDefinitionComposer.GetByName(weaponName);
            if (definition != null)
            {
                if (weaponName.Equals(playerModel.AnimationWeapon, StringComparison.OrdinalIgnoreCase))
                    Weapons.Add(definition, owner, owner.World.EntityManager, playerModel.AnimationWeaponFrame, playerModel.WeaponFlashFrame);
                else
                    Weapons.Add(definition, owner, owner.World.EntityManager);
            }
        }

        for (int i = 0; i < playerModel.Inventory.Powerups.Count; i++)
        {
            var powerupModel = playerModel.Inventory.Powerups[i];
            EntityDefinition? definition = EntityDefinitionComposer.GetByName(powerupModel.Name);
            if (definition == null)
                continue;

            Powerups.Add(new PowerupBase(owner, definition, powerupModel));
        }

        SortKeys();
        SetPriorityPowerupEffects();
    }

    public InventoryModel ToInventoryModel()
    {
        List<InventoryItemModel> inventoryItems = new();
        for (int i = 0; i < ItemList.Count; i++)
        {
            var item = ItemList[i];
            inventoryItems.Add(new InventoryItemModel()
            {
                Name = item.Definition.Name.ToString(),
                Amount = item.Amount
            });
        }

        List<PowerupModel> powerupModels = new();
        for (int i = 0; i < Powerups.Count; i++)
            powerupModels.Add(Powerups[i].ToPowerupModel());

        return new InventoryModel()
        {
            Items = inventoryItems,
            Weapons = Weapons.GetOwnedWeaponNames(),
            Powerups = powerupModels,
        };
    }

    public static string GetBaseInventoryName(EntityDefinition definition)
    {
        int index = definition.ParentClassNames.FindIndex(x => x.Equals(AmmoClassName, StringComparison.OrdinalIgnoreCase));
        if (index > 0 && index < definition.ParentClassNames.Count - 1)
            return definition.ParentClassNames[index + 1];

        return definition.Name;
    }

    // See TODO in Inventory.Add for this berserk check
    public static bool IsPowerup(EntityDefinition def) =>
        def.IsType(PowerupClassName) ||
        !string.IsNullOrEmpty(def.Properties.Powerup.Type) ||
        def.Name.Equals("BERSERK", StringComparison.OrdinalIgnoreCase) ||
        def.IsType("MapRevealer");

    public bool IsPowerupActive(PowerupType type)
    {
        if (type == PowerupType.ComputerAreaMap)
            return HasItemOfClass("MapRevealer");

        return Powerups.Any(x => x.PowerupType == type);
    }

    public IPowerup? GetPowerup(PowerupType type) => Powerups.FirstOrDefault(x => x.PowerupType == type);

    public void RemovePowerup(IPowerup powerup)
    {
        Powerups.Remove(powerup);
        SetPriorityPowerupEffects();
    }

    public void ClearPowerups()
    {
        for (int i = 0; i < Powerups.Count; i++)
            Remove(Powerups[i].EntityDefinition.Name, 1);
        Powerups.Clear();
        PowerupEffectColor = null;
        PowerupEffectColorMap = null;
    }

    public void Tick()
    {
        bool setPriority = false;
        for (int i = 0; i < Powerups.Count; i++)
        {
            IPowerup powerup = Powerups[i];
            if (powerup.Tick(Owner) == InventoryTickStatus.Destroy)
            {
                Remove(powerup.EntityDefinition.Name, 1);
                Powerups.RemoveAt(i);
                i--;
                setPriority = true;
            }

            if (!powerup.DrawEffectActive && (ReferenceEquals(powerup, PowerupEffectColor) || ReferenceEquals(powerup, PowerupEffectColorMap)))
                setPriority = true;
        }

        if (setPriority)
            SetPriorityPowerupEffects();
    }

    public bool Add(string name, int amount, EntityFlags? flags = null)
    {
        if (amount <= 0)
            return false;

        if (!Items.TryGetValue(name, out InventoryItem? item))
            return false;

        return Add(item.Definition, amount, flags);
    }

    public bool Add(EntityDefinition definition, int amount, EntityFlags? flags = null)
    {
        if (amount <= 0)
            return false;

        // TODO test hack until A_GiveInventory and Pickup states are implemented
        bool overridehack = false;
        if (definition.Name.Equals("BERSERK", StringComparison.OrdinalIgnoreCase))
        {
            overridehack = true;
            GiveBerserk(definition);
        }
        else if (definition.Name.Equals("MEGASPHERE", StringComparison.OrdinalIgnoreCase))
        {
            GiveMegasphere();
        }

        if (definition.IsType(PowerupClassName) || overridehack)
            AddPowerup(definition);

        string name = GetBaseInventoryName(definition);
        int maxAmount = definition.Properties.Inventory.MaxAmount;
        if (definition.IsType(AmmoClassName) && HasItemOfClass(BackPackBaseClassName) && definition.Properties.Ammo.BackpackMaxAmount > maxAmount)
            maxAmount = definition.Properties.Ammo.BackpackMaxAmount;

        bool isKey = definition.IsType(KeyClassName);

        if (Items.TryGetValue(name, out InventoryItem? item))
        {
            // If the player is maxed on this item, return true if AlwaysPickup is set to remove from the world
            bool alwaysPickup = flags != null && flags.Value.InventoryAlwaysPickup;
            if (isKey || item.Amount >= maxAmount)
                return alwaysPickup;

            item.Amount += amount;
            if (item.Amount > maxAmount)
                item.Amount = maxAmount;

            return true;
        }
        else
        {
            EntityDefinition? findDefinition = EntityDefinitionComposer.GetByName(name);
            if (findDefinition == null)
                return false;

            InventoryItem inventoryItem = new(findDefinition, isKey ? 1 : amount);
            AddItem(findDefinition, inventoryItem);

            if (isKey)
                SortKeys();
        }

        return true;
    }

    private void GiveMegasphere()
    {
        EntityDefinition? definition = EntityDefinitionComposer.GetByName("BlueArmorForMegasphere");
        if (definition != null)
            Owner.GiveItem(definition, null);
        definition = EntityDefinitionComposer.GetByName("MegasphereHealth");
        if (definition != null)
            Owner.GiveItem(definition, null);
    }

    private void GiveBerserk(EntityDefinition definition)
    {
        definition.Properties.Powerup.Type = "Strength";
        Weapon? fist = Owner.Inventory.Weapons.GetWeapon("FIST");
        if (fist != null)
            Owner.ChangeWeapon(fist);
        if (Owner.Health < 100)
            Owner.Health = 100;
    }

    public bool SetAmount(EntityDefinition definition, int amount)
    {
        if (!Items.TryGetValue(definition.Name, out InventoryItem? item))
            return false;

        item.Amount = amount;
        return true;
    }

    private void AddPowerup(EntityDefinition definition)
    {
        EntityDefinition? powerupDef = EntityDefinitionComposer.GetByName("Power" + definition.Properties.Powerup.Type);
        if (powerupDef == null)
            return;

        PowerupType powerupType = GetPowerupType(definition.Properties.Powerup.Type);
        if (powerupType == PowerupType.None)
            return;

        IPowerup? existingPowerup = GetPowerup(powerupType);
        if (existingPowerup != null)
        {
            existingPowerup.Reset();
            return;
        }

        Powerups.Add(new PowerupBase(Owner, powerupDef, powerupType));
        SetPriorityPowerupEffects();
    }

    private static PowerupType GetPowerupType(string type)
    {
        for (int i = 0; i < PowerupEnumStringValues.Count; i++)
        {
            if (PowerupEnumStringValues[i].Equals(type, StringComparison.OrdinalIgnoreCase))
                return (PowerupType)i;
        }

        return PowerupType.None;
    }

    private void SetPriorityPowerupEffects()
    {
        PowerupEffectColor = Powerups.Where(x => x.EffectType == PowerupEffectType.Color && x.DrawEffectActive).OrderBy(y => (int)y.PowerupType).FirstOrDefault();
        PowerupEffectColorMap = Powerups.Where(x => x.EffectType == PowerupEffectType.ColorMap).OrderBy(y => (int)y.PowerupType).FirstOrDefault();
    }

    public void AddBackPackAmmo(EntityDefinitionComposer definitionComposer)
    {
        HashSet<string> addedBaseNames = new(StringComparer.OrdinalIgnoreCase);
        List<EntityDefinition> ammoDefinitions = GetAmmoTypes(definitionComposer).Where(x => x.Properties.Ammo.BackpackAmount > 0).ToList();
        foreach (EntityDefinition ammo in ammoDefinitions)
        {
            string baseName = GetBaseInventoryName(ammo);
            if (addedBaseNames.Contains(baseName))
                continue;

            Add(ammo, ammo.Properties.Ammo.BackpackAmount);
            addedBaseNames.Add(baseName);
        }
    }

    public void GiveAllAmmo(EntityDefinitionComposer definitionComposer)
    {
        EntityDefinition? backpackDef = definitionComposer.GetByName(BackPackBaseClassName);
        if (backpackDef != null)
            Add(backpackDef, 1);
        List<EntityDefinition> ammoDefinitions = GetAmmoTypes(definitionComposer).ToList();
        foreach (EntityDefinition ammo in ammoDefinitions)
            Add(ammo, Math.Max(ammo.Properties.Ammo.BackpackMaxAmount, ammo.Properties.Inventory.Amount));
    }

    public void GiveAllKeys(EntityDefinitionComposer definitionComposer)
    {
        List<EntityDefinition> keys = definitionComposer.GetEntityDefinitions().Where(x => x.IsType(KeyClassName) && x.EditorId.HasValue).ToList();
        keys.ForEach(x => Add(x, 1));
    }

    public void ClearKeys()
    {
        for (int i = 0; i < Keys.Count; i++)
            RemoveItem(Keys[i].Definition.Name);
        Keys.Clear();
    }

    public void Clear()
    {
        Items.Clear();
        ItemList.Clear();
        Keys.Clear();
    }

    public bool HasItem(string name) => Items.ContainsKey(name);

    public bool HasAnyItem(IEnumerable<string> names) => names.Any(x => HasItem(x));

    public bool HasItemOfClass(string name) => ItemList.Any(x => x.Definition.IsType(name));

    public int Amount(string name) => Items.TryGetValue(name, out var item) ? item.Amount : 0;

    public void Remove(string name, int amount)
    {
        if (amount <= 0)
            return;

        if (Items.TryGetValue(name, out InventoryItem? item))
        {
            if (amount < item.Amount)
                item.Amount -= amount;
            else
                RemoveItem(name);

            if (item.Definition.IsType(KeyClassName))
            {
                Keys.Remove(item);
                SortKeys();
            }

            return;
        }

        // If we didn't find it, then it's possibly indexed in some other
        // data structure (ex: weapons).
        Weapons.Remove(name);
    }

    public List<InventoryItem> GetInventoryItems() => ItemList;
    public List<InventoryItem> GetKeys() => Keys;

    private static IEnumerable<EntityDefinition> GetAmmoTypes(EntityDefinitionComposer definitionComposer)
    {
        return definitionComposer.GetEntityDefinitions().Where(x => x.IsType(AmmoClassName));
    }

    private void SortKeys()
    {
        Keys.Sort((i1, i2) =>
        {
            if (!i1.Definition.EditorId.HasValue || !i2.Definition.EditorId.HasValue)
                return 1;

            return i1.Definition.EditorId.Value.CompareTo(i2.Definition.EditorId.Value);
        });
    }

    private void RemoveItem(string name)
    {
        if (!Items.TryGetValue(name, out InventoryItem? item))
            return;

        Items.Remove(name);
        ItemList.Remove(item);
    }

    private void AddItem(EntityDefinition definition, InventoryItem item)
    {
        if (Items.ContainsKey(definition.Name))
            return;

        Items[definition.Name] = item;
        ItemList.Add(item);
        if (definition.IsType(KeyClassName))
            Keys.Add(item);
    }
}
