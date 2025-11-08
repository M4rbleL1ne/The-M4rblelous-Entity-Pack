using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;
using Watcher;

namespace LBMergedMods.Creatures;

sealed class MiniFlyingBigEelCritob : Critob
{
    internal MiniFlyingBigEelCritob() : base(CreatureTemplateType.MiniFlyingBigEel)
    {
        Icon = new SimpleIcon("Kill_MiniLeviathan", RainWorld.GoldRGB + new Color(.2f, .2f, .2f));
        ShelterDanger = ShelterDanger.TooLarge;
        SandboxPerformanceCost = new(.5f, .5f);
        LoadedPerformanceCost = 50f;
        RegisterUnlock(KillScore.Configurable(10), SandboxUnlockID.MiniFlyingBigEel, SandboxUnlockID.FlyingBigEel);
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Flying,
        RoomAttractivenessPanel.Category.LikesOutside
    ];

    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow) => allow = map.getTerrainProximity(tilePos) > 1;

    public override int ExpeditionScore() => 10;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => RainWorld.GoldRGB + new Color(.2f, .2f, .2f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "SFlEel";

    public override IEnumerable<string> WorldFileAliases() => ["miniflyingleviathan", "miniflyinglev", "miniflyingbigeel", "mini flying leviathan", "mini flying lev", "mini flying big eel", "miniflying leviathan", "miniflying lev", "miniflying big eel", "mini flyingleviathan", "mini flyinglev", "mini flyingbig eel", "miniflyingbig eel", "mini flyingbigeel", "mini flying bigeel", "miniflying bigeel"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.BigEel, Type, "MiniFlyingBigEel")
        {
            TileResistances = new()
            {
                Air = new(1f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OutsideRoom = new(1f, Allowed),
                SkyHighway = new(100000f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(10f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1f),
            DamageResistances = new() { Base = 8f },
            StunResistances = new() { Base = 8f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.BigEel)
        }.IntoTemplate();
        t.abstractedLaziness = 10;
        t.requireAImap = true;
        t.offScreenSpeed = 1f;
        t.bodySize = 5f;
        t.grasps = 1;
        t.stowFoodInDen = true;
        t.visualRadius = 450f;
        t.waterVision = 0f;
        t.throughSurfaceVision = 0f;
        t.movementBasedVision = 0f;
        t.hibernateOffScreen = true;
        t.dangerousToPlayer = .8f;
        t.communityID = CreatureCommunities.CommunityID.None;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
        t.canFly = true;
        t.canSwim = false;
        t.meatPoints = 6;
        t.shortcutColor = RainWorld.GoldRGB + new Color(.2f, .2f, .2f);
        return t;
    }

    public override void EstablishRelationships()
    {
        var l = new Relationships(Type);
        l.Ignores(Type);
        l.Ignores(CreatureTemplate.Type.TentaclePlant);
        l.Ignores(CreatureTemplate.Type.PoleMimic);
        l.Ignores(CreatureTemplate.Type.Centipede);
        l.Eats(CreatureTemplate.Type.SmallCentipede, 1f);
        l.Ignores(CreatureTemplate.Type.MirosBird);
        l.Ignores(CreatureTemplate.Type.Vulture);
        l.Ignores(CreatureTemplate.Type.KingVulture);
        l.Ignores(CreatureTemplate.Type.TempleGuard);
        l.Ignores(CreatureTemplate.Type.DaddyLongLegs);
        l.Ignores(CreatureTemplate.Type.BrotherLongLegs);
        l.Ignores(CreatureTemplate.Type.Deer);
        l.Ignores(CreatureTemplate.Type.BigEel);
        l.Ignores(CreatureTemplate.Type.GreenLizard);
        l.Ignores(CreatureTemplate.Type.RedLizard);
        l.Ignores(CreatureTemplate.Type.Leech);
        l.FearedBy(CreatureTemplate.Type.TentaclePlant, 1f);
        l.FearedBy(CreatureTemplate.Type.PoleMimic, 1f);
        l.IgnoredBy(CreatureTemplate.Type.Centipede);
        l.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        l.IgnoredBy(CreatureTemplate.Type.MirosBird);
        l.IgnoredBy(CreatureTemplate.Type.Vulture);
        l.IgnoredBy(CreatureTemplate.Type.KingVulture);
        l.IgnoredBy(CreatureTemplate.Type.TempleGuard);
        l.IgnoredBy(CreatureTemplate.Type.DaddyLongLegs);
        l.IgnoredBy(CreatureTemplate.Type.BrotherLongLegs);
        l.IgnoredBy(CreatureTemplate.Type.Deer);
        l.Ignores(CreatureTemplate.Type.Deer);
        l.IgnoredBy(CreatureTemplate.Type.BigEel);
        l.IgnoredBy(CreatureTemplate.Type.GreenLizard);
        l.IgnoredBy(CreatureTemplate.Type.RedLizard);
        l.IgnoredBy(CreatureTemplate.Type.Leech);
        l.IgnoredBy(CreatureTemplateType.FlyingBigEel);
        l.IgnoredBy(CreatureTemplateType.MiniLeviathan);
        l.Ignores(CreatureTemplateType.FlyingBigEel);
        l.Ignores(CreatureTemplateType.MiniLeviathan);
        l.Ignores(Type);
        if (ModManager.Watcher)
            l.Ignores(WatcherEnums.CreatureTemplateType.Angler);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new MiniFlyingBigEelAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new MiniFlyingBigEel(acrit, acrit.world);

    public override AbstractCreatureAI CreateAbstractAI(AbstractCreature acrit) => new BigEelAbstractAI(acrit.world, acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Centiwing;
}