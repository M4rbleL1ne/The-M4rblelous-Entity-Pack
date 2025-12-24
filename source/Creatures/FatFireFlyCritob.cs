using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;
using RWCustom;
using Random = UnityEngine.Random;
using System;

namespace LBMergedMods.Creatures;

sealed class FatFireFlyCritob : Critob, ISandboxHandler
{
    internal FatFireFlyCritob() : base(CreatureTemplateType.FatFireFly)
    {
        Icon = new SimpleIcon("Kill_FatFireFly", new(.75f, .15f, 0f));
        LoadedPerformanceCost = 100f;
        SandboxPerformanceCost = new(1.1f, .65f);
        RegisterUnlock(KillScore.Configurable(23), SandboxUnlockID.FatFireFly);
    }

    public override int ExpeditionScore() => 23;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(.75f, .15f, 0f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "FFF";

    public override IEnumerable<string> WorldFileAliases() => ["fatfirefly", "fat fire fly", "fat firefly", "fatfire fly"];

    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow) => allow = map.getTerrainProximity(tilePos) > 1;

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Flying,
        RoomAttractivenessPanel.Category.LikesOutside,
        RoomAttractivenessPanel.Category.Dark
    ];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.Vulture, Type, "FatFireFly")
        {
            TileResistances = new()
            {
                Air = new(1f, Allowed),
                OffScreen = new(1f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OutsideRoom = new(1f, Allowed),
                SkyHighway = new(1f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(10f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f),
            DamageResistances = new() { Base = 7f, Explosion = 102f, Electric = 51f },
            StunResistances = new() { Base = 6f, Explosion = 102f, Electric = 51f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Vulture)
        }.IntoTemplate();
        t.abstractedLaziness = 10;
        t.canSwim = false;
        t.canFly = true;
        t.offScreenSpeed = 1f;
        t.bodySize = 6f;
        t.grasps = 1;
        t.stowFoodInDen = true;
        t.shortcutSegments = 5;
        t.visualRadius = 12000f;
        t.movementBasedVision = .4f;
        t.waterVision = 0f;
        t.throughSurfaceVision = 0f;
        t.hibernateOffScreen = true;
        t.dangerousToPlayer = 1f;
        t.communityInfluence = .25f;
        t.socialMemory = true;
        t.meatPoints = 15;
        t.lungCapacity = 300f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        t.BlizzardAdapted = true;
        t.BlizzardWanderer = true;
        t.canAutoAbstractPath = false;
        t.isTooCloseToTerrain = (aimap, pos) => aimap.getTerrainProximity(pos) <= 1 ? 1 : 0;
        return t;
    }

    public override void EstablishRelationships()
    {
        var fff = new Relationships(Type);
        fff.FearedBy(CreatureTemplate.Type.LizardTemplate, 1f);
        fff.IgnoredBy(CreatureTemplate.Type.RedLizard);
        fff.Eats(CreatureTemplate.Type.CicadaA, 1f);
        fff.Eats(CreatureTemplate.Type.LizardTemplate, .7f);
        fff.Ignores(CreatureTemplate.Type.RedLizard);
        fff.Eats(CreatureTemplate.Type.Centipede, 1f);
        fff.Eats(CreatureTemplate.Type.RedCentipede, .7f);
        fff.Eats(CreatureTemplate.Type.Slugcat, 1f);
        fff.Eats(CreatureTemplate.Type.Scavenger, 1f);
        fff.EatenBy(CreatureTemplate.Type.DaddyLongLegs, .15f);
        fff.Fears(CreatureTemplate.Type.DaddyLongLegs, .4f);
        fff.IgnoredBy(CreatureTemplate.Type.BrotherLongLegs);
        fff.Eats(CreatureTemplate.Type.BigSpider, 1f);
        fff.Eats(CreatureTemplate.Type.SpitterSpider, .85f);
        fff.Eats(CreatureTemplate.Type.DropBug, .85f);
        fff.FearedBy(CreatureTemplate.Type.BigSpider, 1f);
        fff.FearedBy(CreatureTemplate.Type.SpitterSpider, 1f);
        fff.FearedBy(CreatureTemplate.Type.DropBug, 1f);
        fff.FearedBy(CreatureTemplate.Type.BigNeedleWorm, 1f);
        fff.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        fff.FearedBy(CreatureTemplate.Type.Centipede, 1f);
        fff.FearedBy(CreatureTemplate.Type.RedCentipede, .75f);
        fff.FearedBy(CreatureTemplate.Type.Scavenger, 1f);
        fff.FearedBy(CreatureTemplate.Type.Slugcat, 1f);
        fff.FearedBy(CreatureTemplate.Type.CicadaA, 1f);
        fff.Eats(CreatureTemplate.Type.BigNeedleWorm, 1f);
        fff.Ignores(CreatureTemplate.Type.SmallNeedleWorm);
        fff.AttackedBy(CreatureTemplate.Type.JetFish, .5f);
        fff.Fears(CreatureTemplate.Type.BigEel, 1f);
        fff.Fears(CreatureTemplate.Type.JetFish, .5f);
        fff.Fears(CreatureTemplate.Type.Leech, .4f);
        fff.EatenBy(CreatureTemplate.Type.BigEel, 1f);
        fff.IgnoredBy(CreatureTemplate.Type.MirosBird);
        if (ModManager.DLCShared)
        {
            fff.Ignores(DLCSharedEnums.CreatureTemplateType.MirosVulture);
            fff.IgnoredBy(DLCSharedEnums.CreatureTemplateType.MirosVulture);
        }
        fff.Ignores(Type);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new VultureAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new FatFireFly(acrit, acrit.world);

    public override AbstractCreatureAI? CreateAbstractAI(AbstractCreature acrit) => new VultureAbstractAI(acrit.world, acrit);

    public override CreatureState CreateState(AbstractCreature acrit) => new Vulture.VultureState(acrit) { mask = false };

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Vulture;

    AbstractWorldEntity ISandboxHandler.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
    {
        var text = data.CustomData + "SandboxData<cC>" + unlock.Data + "<cB>";
        var abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(data.Type.CritType), null, data.Pos, data.ID) { pos = data.Pos };
        abstractCreature.state.LoadFromString(text.Split(["<cB>"], StringSplitOptions.RemoveEmptyEntries));
        abstractCreature.setCustomFlags();
        var state = Random.state;
        Random.InitState(data.ID.RandomSeed);
        if (Random.value < .1f)
            abstractCreature.superSizeMe = true;
        if (Random.value < .08f && AbsProps.TryGetValue(abstractCreature, out var props))
            props.Albino = true;
        Random.state = state;
        return abstractCreature;
    }
}