using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;
using MoreSlugcats;
using Watcher;

namespace LBMergedMods.Creatures;

sealed class DivingBeetleCritob : Critob
{
    internal DivingBeetleCritob() : base(CreatureTemplateType.DivingBeetle)
    {
        Icon = new SimpleIcon("Kill_DivingBeetle", Color.Lerp(DivingBeetleGraphics.BugCol, Color.white, .2f));
        ShelterDanger = ShelterDanger.Hostile;
        RegisterUnlock(KillScore.Configurable(6), SandboxUnlockID.DivingBeetle);
        SandboxPerformanceCost = new(.5f, .5f);
        LoadedPerformanceCost = 20f;
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.LikesWater,
        RoomAttractivenessPanel.Category.Swimming
    ];

    public override int ExpeditionScore() => 6;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.Lerp(DivingBeetleGraphics.BugCol, Color.white, .2f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "dvb";

    public override IEnumerable<string> WorldFileAliases() => ["divingbeetle", "diving beetle"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(this)
        {
            TileResistances = new()
            {
                OffScreen = new(1f, Allowed),
                Air = new(1f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OpenDiagonal = new(1f, Allowed),
                ReachUp = new(1f, Allowed),
                ReachDown = new(1f, Allowed),
                SemiDiagonalReach = new(1f, Allowed),
                DropToWater = new(1f, Allowed),
                ShortCut = new(1.5f, Allowed),
                NPCTransportation = new(3f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(5f, Allowed),
                SeaHighway = new(1f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1f),
            DamageResistances = new() { Base = 1.1f, Water = 200f },
            StunResistances = new() { Base = .7f, Water = 200f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Leech)
        }.IntoTemplate();
        t.instantDeathDamageLimit = float.MaxValue;
        t.roamBetweenRoomsChance = .03f;
        t.offScreenSpeed = .3f;
        t.waterPathingResistance = 2f;
        t.communityInfluence = .15f;
        t.waterVision = 2f;
        t.movementBasedVision = .95f;
        t.dangerousToPlayer = .5f;
        t.pickupAction = "Grab";
        t.visualRadius = 1000f;
        t.scaryness = 1.1f;
        t.throughSurfaceVision = .678f;
        t.BlizzardAdapted = true;
        t.BlizzardWanderer = true;
        t.waterRelationship = CreatureTemplate.WaterRelationship.WaterOnly;
        t.abstractedLaziness = 200;
        t.requireAImap = true;
        t.bodySize = 1.2f;
        t.stowFoodInDen = true;
        t.shortcutSegments = 2;
        t.grasps = 1;
        t.canSwim = true;
        t.meatPoints = 3;
        t.throwAction = "Release";
        t.usesNPCTransportation = true;
        t.lungCapacity = float.PositiveInfinity;
        return t;
    }

    public override void EstablishRelationships()
    {
        var dvb = new Relationships(Type);
        dvb.FearedBy(CreatureTemplate.Type.LanternMouse, .7f);
        dvb.Eats(CreatureTemplate.Type.LanternMouse, .7f);
        dvb.FearedBy(CreatureTemplate.Type.CicadaA, .5f);
        dvb.Eats(CreatureTemplate.Type.CicadaA, .5f);
        dvb.EatenBy(CreatureTemplate.Type.DaddyLongLegs, .1f);
        dvb.Fears(CreatureTemplate.Type.DaddyLongLegs, .5f);
        dvb.FearedBy(CreatureTemplate.Type.Slugcat, 1f);
        dvb.Eats(CreatureTemplate.Type.Slugcat, .8f);
        dvb.FearedBy(CreatureTemplate.Type.Scavenger, .55f);
        dvb.Eats(CreatureTemplate.Type.Scavenger, .55f);
        dvb.IgnoredBy(CreatureTemplate.Type.BigSpider);
        dvb.Ignores(CreatureTemplate.Type.BigSpider);
        dvb.Fears(CreatureTemplate.Type.TentaclePlant, .6f);
        dvb.EatenBy(CreatureTemplate.Type.TentaclePlant, .6f);
        dvb.Fears(CreatureTemplate.Type.PoleMimic, .3f);
        dvb.EatenBy(CreatureTemplate.Type.PoleMimic, .3f);
        dvb.Eats(CreatureTemplate.Type.SmallNeedleWorm, .15f);
        dvb.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, .6f);
        dvb.Eats(CreatureTemplate.Type.SmallCentipede, .1f);
        dvb.FearedBy(CreatureTemplate.Type.SmallCentipede, .55f);
        dvb.Fears(CreatureTemplate.Type.RedCentipede, .8f);
        dvb.EatenBy(CreatureTemplate.Type.RedCentipede, .8f);
        dvb.Ignores(CreatureTemplate.Type.Leech);
        dvb.IgnoredBy(CreatureTemplate.Type.Leech);
        dvb.Ignores(CreatureTemplate.Type.JetFish);
        dvb.IgnoredBy(CreatureTemplate.Type.JetFish);
        dvb.Ignores(Type);
        dvb.Ignores(CreatureTemplate.Type.Vulture);
        dvb.IgnoredBy(CreatureTemplate.Type.Vulture);
        dvb.Ignores(CreatureTemplateType.FatFireFly);
        dvb.IgnoredBy(CreatureTemplateType.FatFireFly);
        dvb.Ignores(CreatureTemplate.Type.Deer);
        dvb.IgnoredBy(CreatureTemplate.Type.Deer);
        dvb.Ignores(CreatureTemplate.Type.Hazer);
        dvb.IgnoredBy(CreatureTemplate.Type.Hazer);
        dvb.Ignores(CreatureTemplateType.HazerMom);
        dvb.IgnoredBy(CreatureTemplateType.HazerMom);
        dvb.Ignores(CreatureTemplate.Type.GarbageWorm);
        dvb.IgnoredBy(CreatureTemplate.Type.GarbageWorm);
        dvb.IgnoredBy(CreatureTemplate.Type.DropBug);
        dvb.Ignores(CreatureTemplate.Type.DropBug);
        dvb.Eats(CreatureTemplate.Type.LizardTemplate, .5f);
        dvb.FearedBy(CreatureTemplate.Type.LizardTemplate, .5f);
        dvb.Ignores(CreatureTemplate.Type.CyanLizard);
        dvb.IgnoredBy(CreatureTemplate.Type.CyanLizard);
        dvb.Fears(CreatureTemplate.Type.RedLizard, .5f);
        dvb.EatenBy(CreatureTemplate.Type.RedLizard, .5f);
        dvb.Fears(CreatureTemplate.Type.GreenLizard, .25f);
        dvb.EatenBy(CreatureTemplate.Type.GreenLizard, .25f);
        dvb.Fears(CreatureTemplate.Type.MirosBird, .8f);
        dvb.EatenBy(CreatureTemplate.Type.MirosBird, .8f);
        dvb.Fears(CreatureTemplate.Type.BigEel, 1f);
        dvb.EatenBy(CreatureTemplate.Type.BigEel, 1f);
        dvb.FearedBy(CreatureTemplate.Type.EggBug, 1f);
        dvb.Ignores(CreatureTemplate.Type.Overseer);
        if (ModManager.DLCShared)
        {
            dvb.Fears(DLCSharedEnums.CreatureTemplateType.StowawayBug, .9f);
            dvb.Fears(DLCSharedEnums.CreatureTemplateType.BigJelly, .2f);
            dvb.EatenBy(DLCSharedEnums.CreatureTemplateType.MirosVulture, .4f);
            dvb.Fears(DLCSharedEnums.CreatureTemplateType.MirosVulture, 1f);
            dvb.FearedBy(DLCSharedEnums.CreatureTemplateType.Yeek, 1f);
        }
        if (ModManager.MSC)
            dvb.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, .5f);
        if (ModManager.Watcher)
        {
            dvb.Fears(WatcherEnums.CreatureTemplateType.Angler, 1f);
            dvb.EatenBy(WatcherEnums.CreatureTemplateType.Angler, 1f);
        }
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new DivingBeetleAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new DivingBeetle(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.JetFish;
}