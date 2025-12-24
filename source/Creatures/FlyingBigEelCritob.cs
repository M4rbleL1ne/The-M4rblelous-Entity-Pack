using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using System.Collections.Generic;
using RWCustom;
using DevInterface;

namespace LBMergedMods.Creatures;

sealed class FlyingBigEelCritob : Critob
{
    internal FlyingBigEelCritob() : base(CreatureTemplateType.FlyingBigEel)
    {
        Icon = new SimpleIcon("Kill_BigEel", RainWorld.GoldRGB + new Color(.2f, .2f, .2f));
        ShelterDanger = ShelterDanger.TooLarge;
        SandboxPerformanceCost = new(4f, 1.2f);
        LoadedPerformanceCost = 300f;
        RegisterUnlock(KillScore.Configurable(25), SandboxUnlockID.FlyingBigEel);
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Flying,
        RoomAttractivenessPanel.Category.LikesOutside
    ];

    public override int ExpeditionScore() => 25;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => RainWorld.GoldRGB + new Color(.2f, .2f, .2f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "FlEel";

    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow) => allow = map.getTerrainProximity(tilePos) > 4;

    public override IEnumerable<string> WorldFileAliases() => ["flyingleviathan", "flyinglev", "flyingbigeel", "flying leviathan", "flying lev", "flying big eel", "flying bigeel", "flyingbig eel"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.BigEel, Type, "FlyingBigEel")
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
            DamageResistances = new() { Base = 1000f },
            StunResistances = new() { Base = 1000f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.BigEel)
        }.IntoTemplate();
        t.abstractedLaziness = 100;
        t.requireAImap = true;
        t.offScreenSpeed = 1f;
        t.bodySize = 75f;
        t.grasps = 1;
        t.stowFoodInDen = true;
        t.visualRadius = 1450f;
        t.waterVision = 0f;
        t.throughSurfaceVision = 0f;
        t.movementBasedVision = 0f;
        t.hibernateOffScreen = true;
        t.dangerousToPlayer = .8f;
        t.communityID = CreatureCommunities.CommunityID.None;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
        t.canFly = true;
        t.canSwim = false;
        t.isTooCloseToTerrain = (aimap, pos) => aimap.getTerrainProximity(pos) <= 4 ? 1 : 0;
        return t;
    }

    public override void EstablishRelationships()
    {
        var b = new Relationships(Type);
        var entries = CreatureTemplate.Type.values.entries;
        for (var i = 0; i < entries.Count; i++)
            b.FearedBy(new(entries[i]), 1f);
        b.IgnoredBy(CreatureTemplate.Type.TempleGuard);
        b.IgnoredBy(CreatureTemplate.Type.GarbageWorm);
        b.IgnoredBy(CreatureTemplate.Type.Leech);
        b.IgnoredBy(CreatureTemplate.Type.SeaLeech);
        b.IgnoredBy(CreatureTemplate.Type.BigEel);
        b.IgnoredBy(CreatureTemplate.Type.JetFish);
        b.IgnoredBy(CreatureTemplate.Type.Hazer);
        b.IgnoredBy(CreatureTemplate.Type.Snail);
        b.IgnoredBy(CreatureTemplate.Type.Salamander);
        b.IgnoredBy(CreatureTemplate.Type.TentaclePlant);
        b.IgnoredBy(CreatureTemplate.Type.PoleMimic);
        b.Ignores(CreatureTemplate.Type.TempleGuard);
        b.Ignores(CreatureTemplate.Type.GarbageWorm);
        b.Ignores(CreatureTemplate.Type.Leech);
        b.Ignores(CreatureTemplate.Type.SeaLeech);
        b.Ignores(CreatureTemplate.Type.BigEel);
        b.Ignores(CreatureTemplate.Type.JetFish);
        b.Ignores(CreatureTemplate.Type.Hazer);
        b.Ignores(CreatureTemplate.Type.Snail);
        b.Ignores(CreatureTemplate.Type.Salamander);
        b.Ignores(CreatureTemplate.Type.TentaclePlant);
        b.Ignores(CreatureTemplate.Type.PoleMimic);
        b.Ignores(CreatureTemplate.Type.Overseer);
        b.Ignores(Type);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new BigEelAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new FlyingBigEel(acrit, acrit.world);

    public override AbstractCreatureAI CreateAbstractAI(AbstractCreature acrit) => new BigEelAbstractAI(acrit.world, acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.BigEel;
}